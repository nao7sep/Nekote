# Nekote.Platform

Platform-specific constants, operating system detection, and cross-platform path manipulation utilities.

## Current Segments

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
- **PathHelper.cs** - Atomic path normalization operations and convenience wrappers.
  - Atomic operations: `NormalizeUnicode` (NFC form), `NormalizeStructure` (resolves `.` and `..`, removes consecutive separators), `NormalizeSeparators`, `HandleTrailingSeparator`.
  - Convenience wrappers: `ToUnixPath`, `ToWindowsPath`, `ToNativePath`, `EnsureTrailingSeparator`, `RemoveTrailingSeparator`.
  - Handles device paths (`\\.\COM1`, `\\?\C:\path`), UNC paths (`\\server\share`), and tolerates forward-slash variants from systematic normalization errors (`//./COM1`).
  - Invalid Unix double-slash (`//usr`) normalizes to single (`/usr`).
  - Separator preservation: Forward slash checked first (cross-platform default), device paths prefer backslash.
  - Pure string transformations - no file system access.
  - Declared as `partial class` - extended by `PathHelper.Combine.cs`.
- **PathHelper.Combine.cs** - Path combining with validation and normalization.
  - Core `Combine` methods: 4 overloads (2, 3, 4, params) accepting nullable `PathOptions` parameter.
  - Convenience wrappers with presets: `CombineNative`, `CombineWindows`, `CombineUnix` (4 overloads each).
  - Replaces `Path.Combine` with safer, normalized alternatives.
  - Validation: `RequireAbsoluteFirstSegment` uses `Path.IsPathFullyQualified` (strictest), `ValidateSubsequentPathsRelative` uses `Path.IsPathRooted` (catches dangerous Windows drive-relative `C:file.txt` and root-relative `\file.txt` paths).
  - Workflow: Filter empty segments → Validate → Delegate to `Path.Combine` → Apply normalizations.
  - Private helpers: `ProcessSegments` (filters/validates), `ApplyNormalization` (applies structure, Unicode, separator, trailing normalizations in sequence).
  - 60+ lines of validation comments explaining `IsPathFullyQualified` vs `IsPathRooted` behavior.