# Code Review Report: `Nekote.Core.IO`

This report covers the code review for the `Nekote.Core.IO` components, which include helper classes for directory, file, and path manipulations. The review was conducted based on the guidelines specified in `PLAYBOOK.md`.

**Overall Assessment:**
The `IO` component provides powerful and sophisticated helpers, especially for asynchronous file operations. The `FileHelper` class is of excellent quality. However, a **major design flaw** in `DirectoryHelper`'s move operation and a minor implementation issue in `PathHelper` have been identified.

---

## 1. `FileHelper.cs`

This class provides async, cancellable file copy and move operations.

### 1.1. Positive Findings

*   **Excellent `CopyAsync` Implementation:** The method is a textbook example of a robust, cancellable, asynchronous file copy. It correctly uses `FileStream` with `FileOptions.Asynchronous` and passes the `CancellationToken` to the underlying `CopyToAsync` call, making it efficient and responsive.
*   **Pragmatic `MoveAsync` Implementation:** The logic to first attempt a fast, synchronous `File.Move` (for same-volume operations) and then fall back to a cancellable `CopyAsync` + `Delete` for cross-volume moves is very clever.
*   **Accurate Documentation:** The XML comments for `MoveAsync` correctly describe its dual-mode behavior, which is crucial for any developer using it.
*   **Playbook Adherence:** The class fully adheres to all playbook rules.

### 1.2. Issues and Recommendations

*   None. This class is of excellent quality.

---

## 2. `DirectoryHelper.cs`

This class provides async methods to recursively copy and move directories.

### 2.1. Positive Findings

*   The recursive logic for traversing the directory structure is sound.
*   The methods correctly delegate file-level operations to `FileHelper`.
*   Cancellation tokens are checked during the recursive loop, making the operations responsive.

### 2.2. Issues and Recommendations

#### 2.2.1. Major: Non-Atomic Move Operation Risks Data Loss

*   **File:** `DirectoryHelper.cs`
*   **Method:** `MoveAsync`
*   **Observation:** This method moves a directory by recursively moving each file and then deleting the now-empty source subdirectory. Because `FileHelper.MoveAsync` falls back to a "copy-then-delete" for cross-volume moves, the `DirectoryHelper.MoveAsync` operation becomes a sequence of `(Copy File A -> Delete File A -> Copy File B -> Delete File B -> ...)`.
*   **Risk:** This operation is **not atomic**. If it is cancelled or fails (e.g., due to a permissions error or disk space issue) midway through, the source directory will be partially destroyed, and the destination will be partially created. This leaves the file system in an inconsistent state and can be considered a form of data loss.
*   **Misleading Documentation:** The method's XML comment, `このメソッドは完全にキャンセル可能です` ("This method is fully cancellable"), is highly misleading. While the method *responds* to a cancellation request, it does not do so safely. It does not roll back or guarantee a consistent state.
*   **Recommendation (CRITICAL):**
    1.  **Update Documentation:** Immediately change the XML comment for `MoveAsync` to warn developers that the operation is **not atomic** and that cancellation or failure during a cross-volume move can leave directories in a partially moved, inconsistent state.
    2.  **Consider a Safer Implementation:** A safer, though still not truly atomic, implementation would be to first perform a full `CopyAsync` of the entire directory structure, and **only after the copy is 100% successful**, delete the entire source directory. This reduces the window of vulnerability.

---

## 3. `PathHelper.cs`

This class provides static methods for path manipulation.

### 3.1. Positive Findings

*   **`MapPath`:** This is a useful and correctly implemented helper for resolving a path relative to the application's base directory. Its validation is robust.

### 3.2. Issues and Recommendations

#### 3.2.1. Minor: Flawed `NormalizeDirectorySeparators` Logic

*   **File:** `PathHelper.cs`
*   **Method:** `NormalizeDirectorySeparators`
*   **Observation:** The implementation `path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)` is incomplete. On non-Windows platforms, it will not correctly convert a path string containing backslashes (`\`) to forward slashes (`/`). The example provided in the XML comment is also incorrect for Linux.
*   **Recommendation:** A more robust, platform-agnostic implementation would normalize both separator characters.

    **Suggested Change:**
    ```csharp
    public static string NormalizeDirectorySeparators(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }
        // Normalize all backslashes to forward slashes, then normalize
        // forward slashes to the platform's specific primary separator.
        return path.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar);
    }
    ```

---

## 4. Conclusion

The `IO` component contains some excellent, high-quality code, particularly in `FileHelper.cs`. However, the critical flaw in `DirectoryHelper.MoveAsync` presents a significant risk of data corruption and must be addressed, at a minimum by correcting its documentation. The minor issue in `PathHelper` should also be corrected to improve robustness.
