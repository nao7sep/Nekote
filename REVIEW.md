## Review Checklist

### Class Organization & Logic

* **Member Placement**: Verify new class members are inserted in logical positions within the class structure, not simply appended to the end.
* **Member Ordering**: Check if type members follow a logical sequence that doesn't require manual reorganization.

### Variable Naming & Semantics

* **Meaningful Names**: Check for cryptic variable names like `dt`, `s`, or `str` that should be replaced with descriptive names like `dateTime`, `result`, or `connectionString`.
* **Null Semantics**: Confirm `null` represents "value not set or unclear" while empty strings represent "value explicitly set as empty."
* **Lazy Usage**: Verify `Lazy<T>` is used appropriately when initialization should be delayed AND the result may be null, or when values can become null again after initialization.

### Path Validation

* **Path Type Enforcement**: Ensure path parameters are validated for their expected type (relative vs absolute) using `Path.IsFullyQualified()` with appropriate exceptions.

### Async Implementation

* **Async Necessity**: Evaluate whether potentially heavy operations should be made asynchronous.
* **ConfigureAwait Usage**: Verify all `await` calls include `.ConfigureAwait(false)`.
* **CancellationToken Support**: Check that async methods accept `CancellationToken` parameters where cancellation makes sense.

### Documentation, Comments & Messaging

* **Comment Language**: Ensure all XML documentation (`/// <summary>`) and inline source code comments are written in clear, idiomatic Japanese.
* **Message and Metadata Language**: Verify that user-facing text, such as exception messages and log output, is written in clear and precise English.
* **Test Comments**: Confirm that test methods are structured with the "Arrange, Act, Assert" pattern, using English comments (`// Arrange`, `// Act`, `// Assert`).
* **Comment Accuracy**: Identify and correct comments that are outdated or no longer match the current code implementation.
* **Documentation Completeness**: Verify that all public members are fully documented and that complex logic is explained with inline comments.

### Testing Coverage

* **Edge Case Analysis**: Identify methods or classes that handle complex logic or edge cases but lack corresponding test coverage.
* **Test Necessity**: Distinguish between code that requires testing (complex logic, edge cases) and code that is obviously sound and doesn't need additional test coverage.
