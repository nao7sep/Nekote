## Review Checklist

### Code Structure & Organization

* **Member Placement**: Confirm that new class members are positioned logically within the class structure rather than appended arbitrarily to the end.
* **Member Ordering**: Assess whether type members follow a coherent sequence that eliminates the need for manual reorganization.
* **Separation of Concerns**: Verify that each class, method, and module maintains a single, well-defined responsibility. Identify components that mix unrelated functionality and recommend splitting them into focused, cohesive units.

### Standards Compliance

* **Coding Directive Adherence**: Verify comprehensive compliance with all coding standards specified in GEMINI.md.
* **Path Validation Implementation**: Ensure path parameters are properly validated using `Path.IsFullyQualified()` with appropriate exception handling.
* **Enum Value Validation**: Confirm that when enum values are used (not when stored), they are explicitly validated using switch statements with default cases that throw exceptions for undefined values. Never assume that if a value is not A, it must be B, as enum types may be extended with additional values. Constructors and property setters typically store enum values without validation.

### Asynchronous Programming Review

* **ConfigureAwait Consistency**: Verify that all `await` expressions include `.ConfigureAwait(false)` to prevent deadlock scenarios.
* **Cancellation Token Integration**: Assess whether async methods appropriately accept `CancellationToken` parameters where cancellation semantics are meaningful.
* **Asynchronous Operation Assessment**: Evaluate whether computationally intensive or I/O-bound operations have been appropriately converted to asynchronous implementations.

### Variable Naming & Semantic Accuracy

* **Descriptive Naming**: Identify and replace cryptic variable names (e.g., `dt`, `s`, `str`) with descriptive alternatives (e.g., `dateTime`, `result`, `connectionString`).
* **Path vs. Name Distinction**: Validate that variable names clearly distinguish between path references and name identifiers.
* **Type-Reflective Naming**: Ensure variable names for complex objects reflect their types (e.g., `FileInfo fileInfo` rather than `FileInfo file`).
* **Null Semantic Consistency**: Verify that `null` represents "value not set or unclear" while empty strings represent "value explicitly set as empty."
* **Lazy Initialization Appropriateness**: Assess whether `Lazy<T>` usage is appropriate when initialization should be delayed AND the result may be null, or when values can become null again after initialization.

### Documentation & Communication Standards

* **Source Code Commentary**: Ensure all XML documentation (`/// <summary>`) and inline comments are written in clear, idiomatic Japanese.
* **User-Facing Messaging**: Verify that exception messages, log entries, and metadata are composed in precise, professional English.
* **English Text Quality**: Verify that all English text in comments, documentation, exception messages, and variable names uses clear, precise, and professional language appropriate for technical documentation. Identify and refine any informal expressions, unclear phrasing, or awkward constructions.
* **Test Method Structure**: Confirm test methods follow the "Arrange, Act, Assert" pattern with corresponding English section comments.
* **Documentation Currency**: Identify and rectify comments that have become obsolete or inconsistent with current implementation.
* **Documentation Completeness**: Verify comprehensive documentation coverage for all public members and adequate explanation of complex algorithmic logic.
* **Comment Formatting**: Assess comment line wrapping for readability while maintaining natural paragraph flow.

### Test Coverage Analysis

* **Edge Case Coverage**: Identify methods or classes handling complex logic or boundary conditions that lack corresponding test coverage.
* **Testing Necessity Assessment**: Distinguish between code requiring rigorous testing (complex algorithms, edge cases) and code with obvious correctness that may not warrant additional test coverage.
* **Test Project Structure**: Verify that test project organization mirrors the structural hierarchy of the target project being tested.
