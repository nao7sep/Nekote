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

### Documentation & Comments

* **Missing Documentation**: Verify all public members have XML documentation comments (`/// <summary>`) in Japanese, as AI often generates code with minimal commenting.
* **Outdated Comments**: Identify comments that no longer match the current implementation due to code maintenance.
* **Documentation Accuracy**: Ensure XML documentation reflects actual method behavior and parameters.
* **Comment Completeness**: Check that complex logic includes explanatory inline comments in Japanese.

### Testing Coverage

* **Edge Case Analysis**: Identify methods or classes that handle complex logic or edge cases but lack corresponding test coverage.
* **Test Necessity**: Distinguish between code that requires testing (complex logic, edge cases) and code that is obviously sound and doesn't need additional test coverage.
