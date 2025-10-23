## Gemini Directives

### Language & Communication

* **User Interaction**: All conversations with the user (this chat) must be in **English**.
* **Source Code Comments**: All generated source code comments (e.g., `//`, `/// <summary>`, ``) must be in **Japanese**.
* **Internal Messages**: All log messages, exception messages, and internal metadata must be in **English**.

### C# Coding Standards

* **Namespaces**: Always use bracketed namespaces (e.g., `namespace MyNamespace { ... }`). Do not use file-scoped namespaces.
* **Class Member Organization**: Position new class members logically within the class structure rather than appending to the end. Refactor when necessary to maintain clarity.
* **String Validation**: Always use `string.IsNullOrWhiteSpace` in preference to `string.IsNullOrEmpty` when checking for null or empty strings to ensure whitespace-only strings are also handled.
* **XML Documentation Comments**: Avoid HTML formatting tags (e.g., `<b>`, `<i>`) within XML documentation comments. Use plain text for emphasis. The `<see>` tag is permitted for referencing other code elements.
* **Comment Formatting**: Wrap long comment lines to maintain readability, targeting a soft limit of 120 characters. Avoid very short "orphan" lines; exceeding the 120-character limit slightly is preferable to ensure natural paragraph flow.

### Naming Conventions

* **Path vs. Name**: Variable names must clearly distinguish between paths and names.
    * Use 'Path' for paths (e.g., `directoryPath`).
    * Use 'Name' for names (e.g., `fileName`).
    * Avoid ambiguous names like `imageDirectory`.
* **Object Type Clarity**: Variable names for complex objects should reflect the object's type.
    * Example: `FileInfo fileInfo` (not `FileInfo file`).
    * Example: `DirectoryInfo directoryInfo` (not `DirectoryInfo directory`).
* **Meaningful Names**: Avoid cryptic variable names like `dt`, `s`, or `str`. Use descriptive names like `dateTime`, `result`, or `connectionString`.

### Data Handling & Validation

* **Path Validation**: Validate path parameters for their expected type (relative vs absolute) using `Path.IsFullyQualified()` with appropriate exception handling.
* **Null Semantics**: Use `null` to represent "value not set or unclear" while empty strings represent "value explicitly set as empty."
* **Lazy Initialization**: Use `Lazy<T>` appropriately when initialization should be delayed AND the result may be null, or when values can become null again after initialization.
* **Enum Parameter Validation**: Methods accepting enum parameters must explicitly validate all values and throw exceptions for undefined values. Never assume that if a value is not A, it must be B, as enum types may be extended with additional values.

### Async Programming

* **ConfigureAwait Usage**: All `await` calls must include `.ConfigureAwait(false)`.
* **CancellationToken Integration**: Async methods must accept `CancellationToken` parameters where cancellation semantics are meaningful.
* **Asynchronous Operation Assessment**: Evaluate whether computationally intensive or I/O-bound operations should be implemented asynchronously.

### File & Project Structure

* **One Type Per File**: Enforce a strict "one type per file" rule for C# code.
* **File Naming**: The file name must be identical to the public type it contains (e.g., class `MyClass` must be in `MyClass.cs`).
* **File Headers**: Do not add comments containing the file path to the beginning of generated code files.
* **Test Projects**: The folder structure of a test project must mirror the folder structure of the project it is testing.
