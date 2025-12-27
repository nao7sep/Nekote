# Nekote.Platform

Platform-specific constants, operating system detection, and cross-platform path manipulation utilities.

## Segments

### Platform Constants
- **LineEndings.cs** - Standard line ending sequences (`CrLf`, `Lf`, `Cr`, `Native`).
  - Immutable string constants for enforcing consistent text file I/O.
  - Used by NINI format implementations.
- **PathSeparators.cs** - Directory separator characters (`Windows`, `Unix`, `Native`).
  - Pure character constants for string-based path operations.
  - Used by `PathHelper` for normalization.

### Operating System Detection
- **OperatingSystemType.cs** - Enumeration of operating system families (`Windows`, `Linux`, `MacOS`, `Unknown`).
- **OperatingSystem.cs** - Cached operating system detection.
  - Wraps `System.OperatingSystem` APIs with consistent naming.
  - Caches results at startup for performance.
  - Thread-safe static initialization.

### Path Normalization Configuration
- **PathSeparatorMode.cs** - Enumeration for separator normalization strategies (`Preserve`, `Native`, `Unix`, `Windows`).
- **TrailingSeparatorHandling.cs** - Enumeration for trailing separator behavior (`Preserve`, `Remove`, `Ensure`).
- **PathOptions.cs** - Configuration record for path combining and normalization.
  - Record type with `required` properties - all settings must be explicitly initialized.
  - Groups validation settings (`ThrowOnEmptySegments`, `TrimSegments`, `RequireAbsoluteFirstSegment`, `ValidateSubsequentPathsRelative`, `RequireAtLeastOneSegment`).
  - Groups normalization settings (`NormalizeUnicode` for macOS NFC, `NormalizeStructure` for `.` and `..` resolution, `NormalizeSeparators`, `TrailingSeparator`).
  - Provides five presets: `Default`, `Native`, `Windows`, `Unix`, `Minimal`.
  - When passed as `null` to `PathHelper` methods, defaults to `PathOptions.Default`.
  - Immutable - use `with` expressions to modify presets.

### Path Manipulation
- **PathHelper.cs** - Core utility entry point.
  - Contains shared internal helpers: `IsValidDriveChar` (validates A-Z drive letters), `IsSeparator` (checks for / or \ path separators).
  - Serves as the central definition for the static `PathHelper` class.
- **PathHelper.RootParsing.cs** - Root path detection and parsing logic.
  - Encapsulates the complexity of identifying different root types (Drive, UNC, Device, Extended).
  - Responsible for determining the immutable boundaries of a path.
  - **Invariants**: Unlike .NET's `Path` methods (which never throw), uses fail-fast validation that throws `ArgumentException` when Windows-specific formats (drive letters, UNC, device paths) are used with non-Windows target OS.
- **PathHelper.Normalization.cs** - Path structure and content normalization.
  - Handles the transformation of path strings into a canonical form (structure, separators, casing).
  - **Invariants**: For UNC paths, the share (`\\server\share`) is a permission boundary - `..` traversal never escapes above the share to prevent unauthorized access to other shares.
  - Ensures cross-platform consistency (e.g., Unicode normalization for macOS NFD compatibility).
- **PathHelper.Combining.cs** - Safe path concatenation logic.
  - Replaces standard `Path.Combine` with a safer implementation that enforces `PathOptions`.
  - Coordinates validation (e.g., absolute/relative checks) and normalization during the combination process.