# Nekote.Platform

Provides platform-specific constants and OS detection for cross-platform applications, focusing on line endings, path separators, and operating system identification.

## Foundation: Platform Constants

- **LineEndings.cs** - Standard line ending sequences for text files.
  - Provides `CrLf` (`\r\n`), `Lf` (`\n`), `Cr` (`\r`), and `Native`.
  - Immutable string constants enabling explicit control over line endings in text file I/O.
  - Used by NINI file format implementations to enforce consistent line ending behavior.

- **PathSeparators.cs** - Directory separator characters for different file systems.
  - Provides `Windows` (`\`), `Unix` (`/`), and `Native`.
  - Used by PathHelper for string-based path normalization.
  - Does not perform file system operations - purely character constants.

## Core: Operating System Detection

- **OperatingSystemType.cs** - Enumeration of operating system types.
  - Values: `Windows`, `Linux`, `MacOS`, `Unknown`.
  - `Unknown` represents unsupported platforms (FreeBSD, mobile, browser environments).
  - Mobile (Android, iOS) and browser (WebAssembly) support planned for future releases.

- **OperatingSystem.cs** - Cached operating system detection.
  - Boolean properties: `IsWindows`, `IsLinux`, `IsMacOS` for platform-specific branching.
  - `Current` property returns `OperatingSystemType` enum (cached at startup for performance).
  - Wraps `System.OperatingSystem` APIs with consistent naming and caching behavior.
  - Architecture properties (bit width, processor count) deferred following YAGNI - will be added when image processing or parallel operations require them.

## Utilities: Path Manipulation

- **PathHelper.cs** - Cross-platform path separator conversion.
  - Methods: `ToUnixPath`, `ToWindowsPath`, `ToNativePath`.
  - Pure string transformation using `PathSeparators` constants - no file system access.
  - Complements `System.IO.Path` without duplicating it (use `Path.Combine`, `Path.GetFileName`, etc. for standard operations).
