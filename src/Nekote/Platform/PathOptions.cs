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
    /// Gets or sets whether to throw an exception on null, empty, or whitespace-only path segments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <c>true</c>, throws <see cref="ArgumentException"/> if any segment is <c>null</c>, empty,
    /// or whitespace-only. Use this for strict validation when all segments must be meaningful.
    /// </para>
    /// <para>
    /// When <c>false</c>, silently ignores and filters out segments that are <c>null</c>, empty strings,
    /// or contain only whitespace. This is useful for handling optional path components.
    /// </para>
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
    /// Gets or sets whether to validate that subsequent path segments are relative paths.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <c>true</c>, throws <see cref="ArgumentException"/> if any segment after the first
    /// is detected as an absolute path using <see cref="Path.IsPathRooted"/>.
    /// </para>
    /// <para>
    /// This validation prevents silent path replacement bugs where <see cref="Path.Combine"/>
    /// would discard previous segments when encountering an absolute path. It catches dangerous
    /// Windows-specific paths:
    /// <list type="bullet">
    /// <item>Drive-relative paths (<c>C:file.txt</c>) - relative to current directory on drive C:</item>
    /// <item>Root-relative paths (<c>\file.txt</c>) - relative to current drive root</item>
    /// </list>
    /// </para>
    /// </remarks>
    public required bool ValidateSubsequentPathsRelative { get; init; }

    /// <summary>
    /// Gets or sets whether to normalize Unicode strings to NFC (Canonical Composition) form.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <c>true</c>, applies <see cref="string.Normalize(System.Text.NormalizationForm.FormC)"/> to the result.
    /// This is critical for cross-platform applications, particularly when working with macOS.
    /// </para>
    /// <para>
    /// macOS file systems store filenames in NFD (decomposed) form, where characters like "café"
    /// are stored as separate base + combining characters. This can cause string comparison failures
    /// and dictionary lookup misses. Normalizing to NFC ensures consistent string representation
    /// across all platforms.
    /// </para>
    /// </remarks>
    public required bool NormalizeUnicode { get; init; }

    /// <summary>
    /// Gets or sets whether to normalize path structure by resolving <c>.</c> and <c>..</c> segments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <c>true</c>, resolves:
    /// <list type="bullet">
    /// <item><c>.</c> (current directory) - removed from path</item>
    /// <item><c>..</c> (parent directory) - collapses with previous segment</item>
    /// </list>
    /// </para>
    /// <para>
    /// Example: <c>dir1/./dir2/../dir3</c> becomes <c>dir1/dir3</c>
    /// </para>
    /// <para>
    /// Note: This normalization preserves relative paths. Use <c>CombineToAbsolute</c>
    /// with <see cref="Path.GetFullPath"/> to convert to absolute paths.
    /// </para>
    /// </remarks>
    public required bool NormalizeStructure { get; init; }

    /// <summary>
    /// Gets or sets how path separators should be normalized.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Controls whether mixed separators (<c>/</c> and <c>\</c>) are converted to a standard form.
    /// </para>
    /// <para>
    /// <see cref="PathSeparatorMode.Preserve"/> is often the best default choice for cross-platform
    /// applications. Unix-style forward slashes (<c>/</c>) work correctly on all modern platforms
    /// (Windows, Linux, macOS) and serve as a canonical representation for storing paths in databases
    /// or configuration files. Convert to platform-native separators only when actually accessing
    /// the filesystem if needed, though even on Windows, forward slashes are now widely supported
    /// and provide better roundtrip consistency.
    /// </para>
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
        ThrowOnEmptySegments = false,
        TrimSegments = true,
        RequireAtLeastOneSegment = true,
        ValidateSubsequentPathsRelative = true,
        NormalizeUnicode = true,
        NormalizeStructure = true,
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
        ThrowOnEmptySegments = false,
        TrimSegments = true,
        RequireAtLeastOneSegment = true,
        ValidateSubsequentPathsRelative = true,
        NormalizeUnicode = true,
        NormalizeStructure = true,
        NormalizeSeparators = PathSeparatorMode.Native,
        TrailingSeparator = TrailingSeparatorHandling.Remove
    };

    /// <summary>
    /// Gets path options with Windows-style separators.
    /// </summary>
    /// <remarks>
    /// Forces all separators to backslash (<c>\</c>). Useful when generating paths
    /// specifically for Windows systems or Windows-formatted configuration files.
    /// </remarks>
    public static PathOptions Windows { get; } = new()
    {
        ThrowOnEmptySegments = false,
        TrimSegments = true,
        RequireAtLeastOneSegment = true,
        ValidateSubsequentPathsRelative = true,
        NormalizeUnicode = true,
        NormalizeStructure = true,
        NormalizeSeparators = PathSeparatorMode.Windows,
        TrailingSeparator = TrailingSeparatorHandling.Remove
    };

    /// <summary>
    /// Gets path options with Unix-style separators.
    /// </summary>
    /// <remarks>
    /// Forces all separators to forward slash (<c>/</c>). Useful when generating paths
    /// for Unix systems, URLs, or Unix-formatted configuration files.
    /// </remarks>
    public static PathOptions Unix { get; } = new()
    {
        ThrowOnEmptySegments = false,
        TrimSegments = true,
        RequireAtLeastOneSegment = true,
        ValidateSubsequentPathsRelative = true,
        NormalizeUnicode = true,
        NormalizeStructure = true,
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
        ThrowOnEmptySegments = false,
        TrimSegments = true,
        RequireAtLeastOneSegment = true,
        ValidateSubsequentPathsRelative = true,
        NormalizeUnicode = false,
        NormalizeStructure = false,
        NormalizeSeparators = PathSeparatorMode.Preserve,
        TrailingSeparator = TrailingSeparatorHandling.Preserve
    };
}
