using System.Text;

namespace Nekote.Platform;

public static partial class PathHelper
{
    #region Path Combining

    /// <summary>
    /// Combines two path segments into a normalized path.
    /// </summary>
    /// <param name="options">The path options controlling filtering, validation, and normalization behavior. If <c>null</c>, defaults to <see cref="PathOptions.Default"/>.</param>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <returns>A normalized path.</returns>
    public static string Combine(PathOptions? options, string? path1, string? path2)
    {
        options ??= PathOptions.Default;
        var processed = ProcessSegments(options, path1, path2);
        var combined = CombineInternal(options, processed);
        return ApplyNormalization(options, combined);
    }

    /// <summary>
    /// Combines three path segments into a normalized path.
    /// </summary>
    /// <param name="options">The path options controlling filtering, validation, and normalization behavior. If <c>null</c>, defaults to <see cref="PathOptions.Default"/>.</param>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <param name="path3">The third path segment.</param>
    /// <returns>A normalized path.</returns>
    public static string Combine(PathOptions? options, string? path1, string? path2, string? path3)
    {
        options ??= PathOptions.Default;
        var processed = ProcessSegments(options, path1, path2, path3);
        var combined = CombineInternal(options, processed);
        return ApplyNormalization(options, combined);
    }

    /// <summary>
    /// Combines four path segments into a normalized path.
    /// </summary>
    /// <param name="options">The path options controlling filtering, validation, and normalization behavior. If <c>null</c>, defaults to <see cref="PathOptions.Default"/>.</param>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <param name="path3">The third path segment.</param>
    /// <param name="path4">The fourth path segment.</param>
    /// <returns>A normalized path.</returns>
    public static string Combine(PathOptions? options, string? path1, string? path2, string? path3, string? path4)
    {
        options ??= PathOptions.Default;
        var processed = ProcessSegments(options, path1, path2, path3, path4);
        var combined = CombineInternal(options, processed);
        return ApplyNormalization(options, combined);
    }

    /// <summary>
    /// Combines any number of path segments into a normalized path.
    /// </summary>
    /// <param name="options">The path options controlling filtering, validation, and normalization behavior. If <c>null</c>, defaults to <see cref="PathOptions.Default"/>.</param>
    /// <param name="paths">Path segments to combine.</param>
    /// <returns>A normalized path.</returns>
    public static string Combine(PathOptions? options, params string?[] paths)
    {
        options ??= PathOptions.Default;
        var processed = ProcessSegments(options, paths);
        var combined = CombineInternal(options, processed);
        return ApplyNormalization(options, combined);
    }

    #endregion

    #region Convenience Overloads

    /// <summary>
    /// Combines two path segments using native platform separators.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <returns>A normalized path with native platform separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?)"/> with <see cref="PathOptions.Native"/>.
    /// </remarks>
    public static string CombineNative(string? path1, string? path2)
        => Combine(PathOptions.Native, path1, path2);

    /// <summary>
    /// Combines three path segments using native platform separators.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <param name="path3">The third path segment.</param>
    /// <returns>A normalized path with native platform separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?, string?)"/> with <see cref="PathOptions.Native"/>.
    /// </remarks>
    public static string CombineNative(string? path1, string? path2, string? path3)
        => Combine(PathOptions.Native, path1, path2, path3);

    /// <summary>
    /// Combines four path segments using native platform separators.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <param name="path3">The third path segment.</param>
    /// <param name="path4">The fourth path segment.</param>
    /// <returns>A normalized path with native platform separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?, string?, string?)"/> with <see cref="PathOptions.Native"/>.
    /// </remarks>
    public static string CombineNative(string? path1, string? path2, string? path3, string? path4)
        => Combine(PathOptions.Native, path1, path2, path3, path4);

    /// <summary>
    /// Combines any number of path segments using native platform separators.
    /// </summary>
    /// <param name="paths">Path segments to combine.</param>
    /// <returns>A normalized path with native platform separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?[])"/> with <see cref="PathOptions.Native"/>.
    /// </remarks>
    public static string CombineNative(params string?[] paths)
        => Combine(PathOptions.Native, paths);

    /// <summary>
    /// Combines two path segments using Windows-style separators (backslashes).
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <returns>A normalized path with Windows-style separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?)"/> with <see cref="PathOptions.Windows"/>.
    /// </remarks>
    public static string CombineWindows(string? path1, string? path2)
        => Combine(PathOptions.Windows, path1, path2);

    /// <summary>
    /// Combines three path segments using Windows-style separators (backslashes).
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <param name="path3">The third path segment.</param>
    /// <returns>A normalized path with Windows-style separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?, string?)"/> with <see cref="PathOptions.Windows"/>.
    /// </remarks>
    public static string CombineWindows(string? path1, string? path2, string? path3)
        => Combine(PathOptions.Windows, path1, path2, path3);

    /// <summary>
    /// Combines four path segments using Windows-style separators (backslashes).
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <param name="path3">The third path segment.</param>
    /// <param name="path4">The fourth path segment.</param>
    /// <returns>A normalized path with Windows-style separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?, string?, string?)"/> with <see cref="PathOptions.Windows"/>.
    /// </remarks>
    public static string CombineWindows(string? path1, string? path2, string? path3, string? path4)
        => Combine(PathOptions.Windows, path1, path2, path3, path4);

    /// <summary>
    /// Combines any number of path segments using Windows-style separators (backslashes).
    /// </summary>
    /// <param name="paths">Path segments to combine.</param>
    /// <returns>A normalized path with Windows-style separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?[])"/> with <see cref="PathOptions.Windows"/>.
    /// </remarks>
    public static string CombineWindows(params string?[] paths)
        => Combine(PathOptions.Windows, paths);

    /// <summary>
    /// Combines two path segments using Unix-style separators (forward slashes).
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <returns>A normalized path with Unix-style separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?)"/> with <see cref="PathOptions.Unix"/>.
    /// </remarks>
    public static string CombineUnix(string? path1, string? path2)
        => Combine(PathOptions.Unix, path1, path2);

    /// <summary>
    /// Combines three path segments using Unix-style separators (forward slashes).
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <param name="path3">The third path segment.</param>
    /// <returns>A normalized path with Unix-style separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?, string?)"/> with <see cref="PathOptions.Unix"/>.
    /// </remarks>
    public static string CombineUnix(string? path1, string? path2, string? path3)
        => Combine(PathOptions.Unix, path1, path2, path3);

    /// <summary>
    /// Combines four path segments using Unix-style separators (forward slashes).
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <param name="path3">The third path segment.</param>
    /// <param name="path4">The fourth path segment.</param>
    /// <returns>A normalized path with Unix-style separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?, string?, string?)"/> with <see cref="PathOptions.Unix"/>.
    /// </remarks>
    public static string CombineUnix(string? path1, string? path2, string? path3, string? path4)
        => Combine(PathOptions.Unix, path1, path2, path3, path4);

    /// <summary>
    /// Combines any number of path segments using Unix-style separators (forward slashes).
    /// </summary>
    /// <param name="paths">Path segments to combine.</param>
    /// <returns>A normalized path with Unix-style separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?[])"/> with <see cref="PathOptions.Unix"/>.
    /// </remarks>
    public static string CombineUnix(params string?[] paths)
        => Combine(PathOptions.Unix, paths);

    #endregion

    #region Internal Implementation

    private static List<string> ProcessSegments(PathOptions options, params string?[] paths)
    {
        // Filter and trim segments
        var segments = paths
            .Where(p => p is not null)
            .Select(p => options.TrimSegments ? p!.Trim() : p!)
            .ToList();

        // Remove empty segments unless throwing is required
        if (options.ThrowOnEmptySegments)
        {
            if (segments.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException(
                    "Path segments cannot be null, empty, or whitespace when ThrowOnEmptySegments is true.",
                    nameof(paths));
            }
        }
        else
        {
            segments.RemoveAll(string.IsNullOrWhiteSpace);
        }

        // Require at least one segment
        if (options.RequireAtLeastOneSegment && segments.Count == 0)
        {
            throw new ArgumentException(
                "At least one non-empty path segment is required when RequireAtLeastOneSegment is true.",
                nameof(paths));
        }

        // Path validation strategy: IsPathFullyQualified vs IsPathRooted
        //
        // We use different validation methods for first vs subsequent segments because they serve different purposes:
        //
        // 1. FIRST SEGMENT (RequireAbsoluteFirstSegment): Uses IsPathFullyQualified
        //    Purpose: Ensure the base path is TRULY absolute with no ambiguity
        //    Strictness: STRICT - only accepts complete, unambiguous absolute paths
        //
        //    Windows - Fully qualified returns TRUE for:
        //      ✓ C:\path        (drive-based absolute)
        //      ✓ \\server\share (UNC path)
        //      ✓ \\.\device     (device path)
        //      ✓ \\?\path       (extended-length path)
        //
        //    Windows - Fully qualified returns FALSE for (DANGEROUS, ambiguous):
        //      ✗ \path          (root-relative - depends on current drive: C:\path or D:\path?)
        //      ✗ C:path         (drive-relative - depends on current directory on C:)
        //      ✗ /path          (root-relative on Windows)
        //
        //    Unix - Fully qualified returns TRUE for:
        //      ✓ /path          (absolute)
        //
        //    Unix - Fully qualified returns FALSE for:
        //      ✗ path           (relative)
        //
        // 2. SUBSEQUENT SEGMENTS (ValidateSubsequentPathsRelative): Uses IsPathRooted
        //    Purpose: Detect ANY rooted path that would cause Path.Combine to discard previous segments
        //    Strictness: LOOSE - catches anything that starts with a separator or drive letter
        //
        //    Windows - Rooted (rootLength > 0) returns TRUE for:
        //      ✓ C:\path        (absolute - would discard previous)
        //      ✓ \path          (root-relative - would discard previous)
        //      ✓ C:path         (drive-relative - would discard previous)
        //      ✓ /path          (root-relative - would discard previous)
        //      ✓ \\server\share (UNC - would discard previous)
        //
        //    Unix - Rooted (rootLength > 0) returns TRUE for:
        //      ✓ /path          (absolute - would discard previous)
        //      ✓ \path          (accepted as separator - would discard previous)
        //
        // WHY THIS MATTERS:
        //
        // Path.Combine("C:\\base", "\\other") → "\\other" (base path LOST!)
        // Path.Combine("C:\\base", "D:\\other") → "D:\\other" (base path LOST!)
        //
        // By using IsPathRooted for subsequent segments, we catch ALL rooted variants that would
        // cause Path.Combine to discard previous segments, preventing silent data loss.
        //
        // By using IsPathFullyQualified for the first segment, we ensure the base path is truly
        // absolute and unambiguous, preventing dependency on process-specific state (current drive,
        // current directory on a specific drive, etc.).
        //
        // CROSS-PLATFORM: Our implementation respects PathOptions.TargetOperatingSystem, so you can
        // validate Windows paths on Unix and vice versa. For example, C:\path is fully qualified
        // when TargetOperatingSystem=Windows, even when running on macOS.

        // Validate first segment is absolute if required
        if (options.RequireAbsoluteFirstSegment && segments.Count > 0)
        {
            if (!IsPathFullyQualified(segments[0], options))
            {
                throw new ArgumentException(
                    $"The first path segment must be an absolute (fully qualified) path when RequireAbsoluteFirstSegment is true. " +
                    $"Got: {segments[0]}",
                    nameof(paths));
            }
        }

        // Validate subsequent segments are relative
        if (options.ValidateSubsequentPathsRelative && segments.Count > 1)
        {
            for (int i = 1; i < segments.Count; i++)
            {
                if (IsPathRooted(segments[i], options))
                {
                    throw new ArgumentException(
                        $"Path segment at index {i} must be a relative path when ValidateSubsequentPathsRelative is true. " +
                        $"Got: {segments[i]}",
                        nameof(paths));
                }
            }
        }

        return segments;
    }

    /// <summary>
    /// Internal method to combine processed path segments using the appropriate separator for the target OS.
    /// </summary>
    /// <param name="options">Path options specifying the target operating system and separator behavior.</param>
    /// <param name="segments">Pre-validated, filtered path segments to combine.</param>
    /// <returns>Combined path string with appropriate separators but before normalization.</returns>
    /// <remarks>
    /// This method replaces the use of <see cref="Path.Combine(string[])"/> to avoid platform-specific
    /// behavior and mixed-separator issues. Instead of relying on the runtime platform's separator,
    /// we use the separator specified by <see cref="PathOptions.TargetOperatingSystem"/>.
    /// 
    /// Why not use Path.Combine?
    /// 
    /// - Path.Combine uses the runtime platform's separator, which creates mixed-separator paths
    /// when combining Windows-style paths (C:\base) on Unix (/), resulting in "C:\base/dir/file".
    /// - These mixed-separator intermediate paths fail validation in GetRootLength when checking
    /// for Windows-specific syntax on non-Windows platforms.
    /// - By using the target OS separator from the start, we avoid creating invalid intermediate
    /// paths and provide predictable cross-platform behavior.
    /// 
    /// Security Note: Unlike Path.Combine, this method does NOT restart on rooted segments.
    /// Path.Combine's behavior of discarding previous segments when encountering an absolute path is a
    /// security risk (path traversal attack vector). Instead, we rely on <see cref="PathOptions.ValidateSubsequentPathsRelative"/>
    /// to explicitly reject rooted subsequent segments when security is required. If validation is disabled,
    /// segments are concatenated as-is, making the behavior explicit rather than silent.
    /// 
    /// Implementation: Simply joins segments with the appropriate separator character.
    /// Structure normalization (handling .., ., etc.) is handled by <see cref="ApplyNormalization"/>.
    /// </remarks>
    private static string CombineInternal(PathOptions options, List<string> segments)
    {
        if (segments.Count == 0)
        {
            return string.Empty;
        }

        if (segments.Count == 1)
        {
            return segments[0];
        }

        // Determine separator based on target OS
        // When TargetOperatingSystem is null, use the current platform's separator
        var targetOS = options.TargetOperatingSystem ?? OperatingSystem.Current;
        char separator = targetOS == OperatingSystemType.Windows
            ? PathSeparators.Windows
            : PathSeparators.Unix;

        // Build the combined path
        var builder = new StringBuilder(segments[0]);

        // Check if first segment already has a trailing separator to avoid duplication
        bool needsSeparator = segments[0].Length == 0 || !IsSeparator(segments[0][^1]);

        for (int i = 1; i < segments.Count; i++)
        {
            if (needsSeparator)
            {
                builder.Append(separator);
            }

            builder.Append(segments[i]);

            // Check if this segment ends with a separator for the next iteration
            needsSeparator = segments[i].Length == 0 || !IsSeparator(segments[i][^1]);
        }

        return builder.ToString();
    }

    #endregion
}
