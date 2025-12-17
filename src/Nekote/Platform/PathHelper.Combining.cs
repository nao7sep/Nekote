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
        var combined = Path.Combine(processed.ToArray());
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
        var combined = Path.Combine(processed.ToArray());
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
        var combined = Path.Combine(processed.ToArray());
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
        var combined = Path.Combine(processed.ToArray());
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

        // Validate first segment is absolute if required
        if (options.RequireAbsoluteFirstSegment && segments.Count > 0)
        {
            if (!Path.IsPathFullyQualified(segments[0]))
            {
                // Path.IsPathFullyQualified requires a complete, absolute path specification.
                //
                // On Windows, this means:
                // - Drive-based absolute paths: C:\path, D:\path
                // - UNC paths: \\server\share\path
                // - Device paths: \\.\device, \\?\path
                //
                // It does NOT accept "rooted but not absolute" paths:
                // - \path (root-relative, depends on current drive)
                // - C:path (drive-relative, depends on current directory on drive C:)
                // - /path (on Windows, treated as root-relative)
                //
                // On Unix-like systems:
                // - /path is fully qualified (absolute)
                // - path is NOT fully qualified (relative)
                //
                // Path.IsPathRooted is less strict and accepts any path starting with a separator:
                //
                // On Windows, Path.IsPathRooted returns true for:
                // - C:\path (absolute)
                // - \path (root-relative)
                // - C:path (drive-relative)
                // - /path (root-relative)
                //
                // On Unix-like systems, Path.IsPathRooted returns true for:
                // - /path (absolute)
                // - \path (absolute, backslash accepted as separator)
                //
                // When RequireAbsoluteFirstSegment is true, we enforce Path.IsPathFullyQualified
                // to prevent ambiguity about where the path resolves. This catches dangerous
                // Windows-specific paths like:
                // - \path (depends on current drive: could be C:\path or D:\path)
                // - C:path (depends on current directory on C:)
                //
                // For cross-platform reliability, always use fully qualified paths when this option is enabled.

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
                if (Path.IsPathRooted(segments[i]))
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

    #endregion
}
