# Code Review Report: `Nekote.Core.Randomization`

This report covers the code review for the `Nekote.Core.Randomization` components. This feature provides an abstraction for random number generation. The review was conducted based on the guidelines specified in `PLAYBOOK.md`.

**Overall Assessment:**
While the component correctly identifies the need to abstract `System.Random` for testability, its default implementation contains a **critical design and performance flaw**. It prioritizes test-time determinism over production performance, resulting in an inefficient implementation for the main application.

---

## 1. `IRandomProvider.cs`

This file defines the interface for the random number provider.

### 1.1. Positive Findings

*   **Excellent Abstraction:** The interface is a well-designed abstraction of `System.Random`. It correctly provides a contract for consumers to program against, enabling dependency injection and testability.
*   **Modern API:** The inclusion of `Span<T>`-based overloads and the `GetItems`/`Shuffle` methods makes the interface powerful and aligned with modern .NET capabilities.

### 1.2. Issues and Recommendations

*   None. The interface is excellent.

---

## 2. `SystemRandomProvider.cs`

This file provides the default implementation of `IRandomProvider`.

### 2.1. Positive Findings

*   The class correctly implements all methods of the `IRandomProvider` interface.

### 2.2. Issues and Recommendations

#### 2.2.1. Critical: Inefficient Thread-Safety Model

*   **Observation:** The class attempts to make the non-thread-safe `System.Random` class safe for concurrent use by wrapping every single method call in a `lock`.
*   **Problem:** This approach, while functional, creates a severe performance bottleneck. It serializes all access to random number generation, causing threads to wait and contention on the single lock. Modern .NET provides a vastly superior alternative.
*   **Recommendation:** The `System.Random` instance and the manual locking should be replaced entirely with calls to `System.Random.Shared`. The `Random.Shared` property provides a thread-safe, highly-optimized `Random` instance specifically for this purpose. A refactored class would be stateless and lock-free, with methods simply delegating to `Random.Shared` (e.g., `public int Next() => Random.Shared.Next();`).

#### 2.2.2. Major: Problematic Seeded Constructor

*   **Observation:** The class provides a public constructor that accepts a `seed`. The DI registration extensions allow for this seeded instance to be registered as a singleton for the entire application.
*   **Problem:** A seeded random number generator produces a predictable, deterministic sequence. Using one as a shared singleton for a whole application is almost always a bug, as it means the application will exhibit the exact same "random" behavior on every run. The justification for this is to support testing, but it comes at the cost of correct production behavior and performance.

---

## 3. `DependencyInjection/RandomProviderServiceCollectionExtensions.cs`

This file contains the DI registration helpers.

### 3.1. Positive Findings

*   The extension methods follow the standard `IServiceCollection` pattern correctly.

### 3.2. Issues and Recommendations

#### 3.2.1. Major: Promotes Flawed Design

*   **Observation:** The XML documentation in this file confirms that the inefficient, lock-based `SystemRandomProvider` was deliberately chosen over `Random.Shared` in order to support seeded generation for testing.
*   **Problem:** This violates the **Separation of Concerns** principle. The need for test determinism should not dictate the implementation or performance of a production service. The current design forces the production application to use a slow, outdated implementation for a problem that has a modern, high-performance solution.
*   **Recommendation (Refactoring):**
    1.  **Create a Production Provider:** Create a new, stateless class `SharedRandomProvider : IRandomProvider` in `Nekote.Core`. Its methods should simply delegate to `Random.Shared`. It will have no locks and no constructors.
    2.  **Update DI for Production:** The `AddSystemRandomProvider()` extension method should be renamed to `AddSharedRandomProvider()` and changed to register `SharedRandomProvider` as the singleton `IRandomProvider`. The overload that accepts a seed should be removed from this production-facing extension.
    3.  **Move the Test Provider:** The existing `SystemRandomProvider` class is a perfect utility *for testing*. It should be physically moved from the `Nekote.Core` project to the `Nekote.Core.Tests` project.
    4.  **Update Tests:** Tests that require a deterministic random sequence can now manually register their provider: `services.AddSingleton<IRandomProvider>(new SystemRandomProvider(seed: 123));`.

---

## 4. Conclusion

The `Randomization` component requires a significant refactoring. The current implementation introduces a serious performance bottleneck into the application by using an outdated locking pattern instead of `Random.Shared`. This flawed design stems from mixing the concerns of production code with the concerns of test code. The recommended refactoring will align the component with modern .NET best practices and the project's own principle of Separation of Concerns, resulting in a fast provider for production and a deterministic provider for tests.
