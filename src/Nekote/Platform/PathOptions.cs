namespace Nekote.Platform;

/// <summary>
/// Provides configuration options for path combining and normalization operations.
/// </summary>
/// <remarks>
/// This record defines a reusable policy for how paths should be processed, validated, and normalized.
/// All properties are required and must be explicitly set. Use predefined presets for common scenarios
/// or create custom instances using the <c>with</c> expression to modify presets.
/// When used as a parameter in <see cref="PathHelper"/> methods, passing <c>null</c> defaults to <see cref="Default"/>.
/// </remarks>
public record PathOptions
{
    /// <summary>
    /// Gets or sets the target operating system for path interpretation and validation.
    /// </summary>
    /// <remarks>
    /// Controls how path roots are detected and interpreted during validation and normalization.
    /// When set to a specific operating system, paths are validated according to that system's rules
    /// regardless of the current runtime platform.
    /// 
    /// Cross-Platform Path Validation:
    /// 
    /// This enables scenarios such as:
    /// - Validating Windows paths (<c>C:\path</c>, <c>\\server\share</c>) while running on Linux or macOS
    /// - Validating Unix paths (<c>/usr/bin</c>) while running on Windows
    /// - Storing paths in databases with platform-specific validation before deployment
    /// - Building cross-platform tools that work with paths from multiple operating systems
    /// 
    /// Platform-Specific Path Semantics:
    /// 
    /// Windows (<see cref="OperatingSystemType.Windows"/>):
    /// - <c>C:\path</c> - Fully qualified (drive letter with separator)
    /// - <c>C:path</c> - Rooted but NOT fully qualified (drive-relative, depends on current directory)
    /// - <c>\path</c> - Rooted but NOT fully qualified (root-relative, depends on current drive)
    /// - <c>\\server\share</c> - Fully qualified (UNC path)
    /// - <c>\\.\device</c>, <c>\\?\path</c> - Fully qualified (device and extended-length paths)
    /// - <c>/path</c> - Rooted but NOT fully qualified (root-relative with forward slash)
    /// 
    /// Unix (<see cref="OperatingSystemType.Linux"/> or <see cref="OperatingSystemType.MacOS"/>):
    /// - <c>/path</c> - Fully qualified (absolute path from root)
    /// - <c>\path</c> - Fully qualified (backslash is treated as separator, equivalent to forward slash)
    /// - <c>relative/path</c> - Relative path (not rooted)
    /// 
    /// Security: Windows-Only Path Formats on Unix
    /// 
    /// When <see cref="TargetOperatingSystem"/> is set to <see cref="OperatingSystemType.Linux"/>
    /// or <see cref="OperatingSystemType.MacOS"/>, the following Windows-specific path formats
    /// will throw <see cref="ArgumentException"/> to prevent silent misinterpretation:
    /// - <c>C:\path</c> - Drive letter paths (no drive concept on Unix)
    /// - <c>\\server\share</c> - UNC paths (network shares work differently on Unix)
    /// - <c>\\.\device</c> - DOS device paths (Windows-only device namespace)
    /// - <c>\\?\path</c>, <c>\??\path</c> - Extended-length paths (Windows-only feature)
    /// 
    /// This fail-fast behavior prevents dangerous scenarios where a developer expects UNC network
    /// access but the path is silently interpreted as a local filesystem path. It's better to
    /// throw an explicit exception than to allow cross-platform path confusion.
    /// 
    /// When <c>null</c>, uses the current runtime platform from <see cref="OperatingSystem.Current"/>.
    /// This is the default behavior and appropriate for most applications.
    /// </remarks>
    public required OperatingSystemType? TargetOperatingSystem { get; init; }

    /// <summary>
    /// Gets or sets whether to throw an exception on null, empty, or whitespace-only path segments.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, throws <see cref="ArgumentException"/> if any segment is <c>null</c>, empty,
    /// or whitespace-only. Use this for strict validation when all segments must be meaningful.
    /// 
    /// When <c>false</c>, silently ignores and filters out segments that are <c>null</c>, empty strings,
    /// or contain only whitespace. This is useful for handling optional path components.
    /// </remarks>
    public required bool ThrowOnEmptySegments { get; init; }

    /// <summary>
    /// Gets or sets whether to trim leading and trailing whitespace from each path segment.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, applies <see cref="string.Trim"/> to each segment before processing.
    /// This helps handle paths that may have been constructed with formatting whitespace.
    /// </remarks>
    public required bool TrimSegments { get; init; }

    /// <summary>
    /// Gets or sets whether to require at least one non-empty segment after filtering.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, throws <see cref="ArgumentException"/> if all segments are filtered out.
    /// Set to <c>false</c> if you want to allow operations on empty segment arrays.
    /// </remarks>
    public required bool RequireAtLeastOneSegment { get; init; }

    /// <summary>
    /// Gets or sets whether to require the first path segment to be an absolute (fully qualified) path.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, validates the first path segment using <see cref="Path.IsPathFullyQualified"/>,
    /// which requires a complete, unambiguous path specification. This is stricter than
    /// <see cref="Path.IsPathRooted"/> and only accepts truly absolute paths.
    /// 
    /// When <c>false</c>, allows the first segment to be any path (relative or absolute).
    /// 
    /// Set to <c>true</c> when you want to ensure path combining always starts from a known absolute location,
    /// preventing ambiguity about where the final path will resolve.
    /// </remarks>
    public required bool RequireAbsoluteFirstSegment { get; init; }

    /// <summary>
    /// Gets or sets whether to validate that subsequent path segments are relative paths.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, throws <see cref="ArgumentException"/> if any segment after the first
    /// is detected as an absolute path using <see cref="Path.IsPathRooted"/>.
    /// 
    /// This validation prevents silent path replacement bugs where <see cref="Path.Combine"/>
    /// would discard previous segments when encountering an absolute path. It catches dangerous
    /// Windows-specific paths:
    /// - Drive-relative paths (<c>C:file.txt</c>) - relative to current directory on drive C:
    /// - Root-relative paths (<c>\file.txt</c>) - relative to current drive root
    /// </remarks>
    public required bool ValidateSubsequentPathsRelative { get; init; }

    /// <summary>
    /// Gets or sets whether to normalize path structure by resolving <c>.</c> and <c>..</c> segments.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, resolves:
    /// - <c>.</c> (current directory) - removed from path
    /// - <c>..</c> (parent directory) - collapses with previous segment
    /// 
    /// Example: <c>dir1/./dir2/../dir3</c> becomes <c>dir1/dir3</c>
    /// 
    /// Note: This normalization preserves relative paths. To convert relative paths to absolute paths,
    /// use <see cref="Path.GetFullPath"/> after normalization.
    /// </remarks>
    public required bool NormalizeStructure { get; init; }

    /// <summary>
    /// Gets or sets whether to normalize Unicode strings to NFC (Canonical Composition) form.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, applies <see cref="string.Normalize(System.Text.NormalizationForm.FormC)"/> to the result.
    /// This is critical for cross-platform applications, particularly when working with macOS.
    /// 
    /// macOS file systems store filenames in NFD (decomposed) form, where characters like "café"
    /// are stored as separate base + combining characters. This can cause string comparison failures
    /// and dictionary lookup misses. Normalizing to NFC ensures consistent string representation
    /// across all platforms.
    /// </remarks>
    public required bool NormalizeUnicode { get; init; }

    /// <summary>
    /// Gets or sets how path separators should be normalized.
    /// </summary>
    /// <remarks>
    /// Controls whether mixed separators (<c>/</c> and <c>\</c>) are converted to a standard form.
    /// 
    /// <see cref="PathSeparatorMode.Preserve"/> is often the best default choice for cross-platform
    /// applications. Unix-style forward slashes (<c>/</c>) work correctly on all modern platforms
    /// (Windows, Linux, macOS) and serve as a canonical representation for storing paths in databases
    /// or configuration files. Convert to platform-native separators only when actually accessing
    /// the filesystem if needed, though even on Windows, forward slashes are now widely supported
    /// and provide better roundtrip consistency.
    /// </remarks>
    public required PathSeparatorMode NormalizeSeparators { get; init; }

    /// <summary>
    /// Gets or sets how trailing path separators should be handled.
    /// </summary>
    /// <remarks>
    /// Controls whether the final path ends with a separator character.
    /// Removing trailing separators (<see cref="TrailingSeparatorHandling.Remove"/>) produces
    /// cleaner, more canonical path representations.
    /// </remarks>
    public required TrailingSeparatorHandling TrailingSeparator { get; init; }

    /// <summary>
    /// Gets default path options with balanced safety and normalization.
    /// </summary>
    /// <remarks>
    /// Enables filtering and validation with structural normalization, but preserves
    /// original separator style for maximum compatibility.
    /// </remarks>
    public static PathOptions Default { get; } = new()
    {
        TargetOperatingSystem = null,
        ThrowOnEmptySegments = false,
        TrimSegments = true,
        RequireAtLeastOneSegment = true,
        RequireAbsoluteFirstSegment = false,
        ValidateSubsequentPathsRelative = true,
        NormalizeStructure = true,
        NormalizeUnicode = true,
        NormalizeSeparators = PathSeparatorMode.Preserve,
        TrailingSeparator = TrailingSeparatorHandling.Remove
    };

    /// <summary>
    /// Gets path options with platform-native separators.
    /// </summary>
    /// <remarks>
    /// Uses the native separator for the current platform (backslash on Windows, forward slash on Unix).
    /// Enables all normalizations for maximum compatibility across Windows, Linux, and macOS.
    /// </remarks>
    public static PathOptions Native { get; } = new()
    {
        TargetOperatingSystem = null,
        ThrowOnEmptySegments = false,
        TrimSegments = true,
        RequireAtLeastOneSegment = true,
        RequireAbsoluteFirstSegment = false,
        ValidateSubsequentPathsRelative = true,
        NormalizeStructure = true,
        NormalizeUnicode = true,
        NormalizeSeparators = PathSeparatorMode.Native,
        TrailingSeparator = TrailingSeparatorHandling.Remove
    };

    /// <summary>
    /// Gets path options with Windows-style separators.
    /// </summary>
    /// <remarks>
    /// Forces all separators to backslash (<c>\</c>). Useful when generating paths
    /// specifically for Windows systems or Windows-formatted configuration files.
    /// Uses Windows path interpretation rules for validation.
    /// </remarks>
    public static PathOptions Windows { get; } = new()
    {
        TargetOperatingSystem = OperatingSystemType.Windows,
        ThrowOnEmptySegments = false,
        TrimSegments = true,
        RequireAtLeastOneSegment = true,
        RequireAbsoluteFirstSegment = false,
        ValidateSubsequentPathsRelative = true,
        NormalizeStructure = true,
        NormalizeUnicode = true,
        NormalizeSeparators = PathSeparatorMode.Windows,
        TrailingSeparator = TrailingSeparatorHandling.Remove
    };

    /// <summary>
    /// Gets path options with Unix-style separators.
    /// </summary>
    /// <remarks>
    /// Forces all separators to forward slash (<c>/</c>). Useful when generating paths
    /// for Unix systems, URLs, or Unix-formatted configuration files.
    /// Uses Linux path interpretation rules for validation (MacOS behavior is identical).
    /// </remarks>
    public static PathOptions Unix { get; } = new()
    {
        TargetOperatingSystem = OperatingSystemType.Linux,
        ThrowOnEmptySegments = false,
        TrimSegments = true,
        RequireAtLeastOneSegment = true,
        RequireAbsoluteFirstSegment = false,
        ValidateSubsequentPathsRelative = true,
        NormalizeStructure = true,
        NormalizeUnicode = true,
        NormalizeSeparators = PathSeparatorMode.Unix,
        TrailingSeparator = TrailingSeparatorHandling.Remove
    };

    /// <summary>
    /// Gets minimal path options with validation but no normalization.
    /// </summary>
    /// <remarks>
    /// Provides safety validation (filtering, trimming, relative path checks)
    /// but preserves path structure and formatting exactly as provided.
    /// Use when you need validation without transformation.
    /// </remarks>
    public static PathOptions Minimal { get; } = new()
    {
        TargetOperatingSystem = null,
        ThrowOnEmptySegments = false,
        TrimSegments = true,
        RequireAtLeastOneSegment = true,
        RequireAbsoluteFirstSegment = false,
        ValidateSubsequentPathsRelative = true,
        NormalizeStructure = false,
        NormalizeUnicode = false,
        NormalizeSeparators = PathSeparatorMode.Preserve,
        TrailingSeparator = TrailingSeparatorHandling.Preserve
    };
}
