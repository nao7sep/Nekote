# Nekote.Platform

Platform-specific constants, operating system detection, and cross-platform path manipulation utilities.

## Current Segments

### Platform Constants
- **LineEndings.cs** - Standard line ending sequences for text files.
  - Provides `CrLf` (`\r\n`), `Lf` (`\n`), `Cr` (`\r`), and `Native`.
  - Immutable string constants enabling explicit control over line endings in text file I/O.
  - Used by NINI format implementations to enforce consistent line ending behavior across platforms.
- **PathSeparators.cs** - Directory separator characters for different file systems.
  - Provides `Windows` (`\`), `Unix` (`/`), and `Native`.
  - Pure character constants with no file system operations.
  - Used by PathHelper for string-based path normalization.

### Operating System Detection
- **OperatingSystemType.cs** - Enumeration of operating system families.
  - Values: `Windows`, `Linux`, `MacOS`, `Unknown`.
  - `Unknown` represents unsupported platforms (FreeBSD, mobile, browser environments).
- **OperatingSystem.cs** - Cached operating system detection.
  - Boolean properties (`IsWindows`, `IsLinux`, `IsMacOS`) for platform-specific branching.
  - `Current` property returns cached `OperatingSystemType` enum.
  - Wraps `System.OperatingSystem` APIs with consistent naming and startup caching for performance.

### Path Normalization Configuration
- **PathSeparatorMode.cs** - Enumeration for separator normalization strategies.
  - Values: `Preserve`, `Native`, `Unix`, `Windows`.
  - Used by PathHelper to control separator conversion behavior.
- **TrailingSeparatorHandling.cs** - Enumeration for trailing separator behavior.
  - Values: `Preserve`, `Remove`, `Ensure`.
  - Used by PathHelper to control presence of trailing separators.
- **PathOptions.cs** - Configuration record for path combining and normalization.
  - Record type with `required` properties - all settings must be explicitly initialized.
  - Groups validation settings (`ThrowOnEmptySegments`, `TrimSegments`, `RequireAbsoluteFirstSegment`, etc.) and normalization settings (`NormalizeUnicode`, `NormalizeStructure`, etc.).
  - Provides presets: `Default`, `Native`, `Windows`, `Unix`, `Minimal`.
  - When passed as `null` to PathHelper methods, defaults to `Default` preset (safe for 95% of use cases).

### Path Manipulation
- **PathHelper.cs** - Cross-platform path normalization and combining utilities.
  - Atomic operations: `NormalizeUnicode` (NFC form for macOS compatibility), `NormalizeStructure` (resolves `.` and `..`), `NormalizeSeparators`, `HandleTrailingSeparator`.
  - Separator convenience wrappers: `ToUnixPath`, `ToWindowsPath`, `ToNativePath`.
  - Trailing separator convenience wrappers: `EnsureTrailingSeparator`, `RemoveTrailingSeparator`.
  - Core combining: `Combine` methods (2, 3, 4, and params overloads) accepting nullable `PathOptions` parameter (defaults to `PathOptions.Default` when `null`).
  - Convenience combining methods with preset options:
    - `CombineNative` (4 overloads) - Uses `PathOptions.Native` for platform-native separators.
    - `CombineWindows` (4 overloads) - Uses `PathOptions.Windows` for Windows-style backslashes.
    - `CombineUnix` (4 overloads) - Uses `PathOptions.Unix` for Unix-style forward slashes.
  - All combining methods provide configurable validation through `PathOptions` properties like `RequireAbsoluteFirstSegment` and `ValidateSubsequentPathsRelative`.
  - Pure string transformations with no file system access - complements `System.IO.Path`.




