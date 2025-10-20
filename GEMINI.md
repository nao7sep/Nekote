## Gemini Directives

### Language & Communication

* **User Interaction**: All conversations with the user (this chat) must be in **English**.
* **Source Code Comments**: All generated source code comments (e.g., `//`, `/// <summary>`, ``) must be in **Japanese**.
* **Internal Messages**: All log messages, exception messages, and internal metadata must be in **English**.

### C# Coding Standards

* **Namespaces**: Always use bracketed namespaces (e.g., `namespace MyNamespace { ... }`). Do not use file-scoped namespaces.
* **Class Member Organization**: When adding a class member (method, property, etc.), insert it in a logical position within the class based on existing structure and conventions. Do not just append it to the end. Refactor if necessary to maintain clarity.
* **String Checks**: When checking for null or empty strings, always use `string.IsNullOrWhiteSpace` in preference to `string.IsNullOrEmpty` to ensure whitespace-only strings are also handled.

### Naming Conventions

* **Path vs. Name**: Variable names must clearly distinguish between paths and names.
    * Use 'Path' for paths (e.g., `directoryPath`).
    * Use 'Name' for names (e.g., `fileName`).
    * Avoid ambiguous names like `imageDirectory`.
* **Object Type Clarity**: Variable names for complex objects should reflect the object's type.
    * Example: `FileInfo fileInfo` (not `FileInfo file`).
    * Example: `DirectoryInfo directoryInfo` (not `DirectoryInfo directory`).

### File & Project Structure

* **One Type Per File**: Enforce a strict "one type per file" rule for C# code.
* **File Naming**: The file name must be identical to the public type it contains (e.g., class `MyClass` must be in `MyClass.cs`).
* **File Headers**: Do not add comments containing the file path to the beginning of generated code files.
* **Test Projects**: The folder structure of a test project must mirror the folder structure of the project it is testing.
