# Nekote.Platform

Provides platform-specific constants, OS detection, and path manipulation utilities for cross-platform applications.

## Current Segments

### Platform Constants
- **LineEndings.cs** - Standard line ending sequences (`CrLf`, `Lf`, `Cr`, `Native`).
  - Immutable string constants for enforcing consistent text file I/O.
- **PathSeparators.cs** - Directory separator characters (`Windows`, `Unix`, `Native`).
  - Pure character constants for string-based path operations.

### Core Detection
- **OperatingSystemType.cs** - Enumeration of supported operating systems (`Windows`, `Linux`, `MacOS`, `Unknown`).
- **OperatingSystem.cs** - Cached operating system detection.
  - Wraps `System.OperatingSystem` APIs with a consistent enum-based interface.
  - Caches results at startup for performance.

### Path Manipulation
- **PathOptions.cs** - Configuration record for path normalization and combining behavior.
- **PathSeparatorMode.cs** - Enum for separator normalization strategies.
- **TrailingSeparatorHandling.cs** - Enum for trailing separator policies.
- **PathHelper.cs** - Atomic path normalization operations.
  - Handles structure normalization (../.), Unicode normalization (NFC), and separator conversion.
  - Contains robust edge-case logic for Device Paths and UNC paths.
- **PathHelper.Combine.cs** - Path combining logic.
  - Extends `PathHelper` with `Combine` methods that apply `PathOptions` to joined paths.
  - Replaces `Path.Combine` with safer, normalized alternatives.