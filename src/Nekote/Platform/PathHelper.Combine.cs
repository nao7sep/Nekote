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
    /// Combines multiple path segments into a normalized path.
    /// </summary>
    /// <param name="options">The path options controlling filtering, validation, and normalization behavior. If <c>null</c>, defaults to <see cref="PathOptions.Default"/>.</param>
    /// <param name="paths">The path segments to combine.</param>
    /// <returns>A normalized path.</returns>
    public static string Combine(PathOptions? options, params string?[] paths)
    {
        options ??= PathOptions.Default;
        ArgumentNullException.ThrowIfNull(paths);
        var processed = ProcessSegments(options, paths);
        var combined = Path.Combine(processed.ToArray());
        return ApplyNormalization(options, combined);
    }

    #endregion

    #region Path Combining (Convenience Methods)

    /// <summary>
    /// Combines two path segments using native platform conventions.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <returns>A normalized path with platform-native separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?)"/> with <see cref="PathOptions.Native"/>.
    /// </remarks>
    public static string CombineNative(string? path1, string? path2)
    {
        return Combine(PathOptions.Native, path1, path2);
    }

    /// <summary>
    /// Combines three path segments using native platform conventions.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <param name="path3">The third path segment.</param>
    /// <returns>A normalized path with platform-native separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?, string?)"/> with <see cref="PathOptions.Native"/>.
    /// </remarks>
    public static string CombineNative(string? path1, string? path2, string? path3)
    {
        return Combine(PathOptions.Native, path1, path2, path3);
    }

    /// <summary>
    /// Combines four path segments using native platform conventions.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <param name="path3">The third path segment.</param>
    /// <param name="path4">The fourth path segment.</param>
    /// <returns>A normalized path with platform-native separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?, string?, string?)"/> with <see cref="PathOptions.Native"/>.
    /// </remarks>
    public static string CombineNative(string? path1, string? path2, string? path3, string? path4)
    {
        return Combine(PathOptions.Native, path1, path2, path3, path4);
    }

    /// <summary>
    /// Combines multiple path segments using native platform conventions.
    /// </summary>
    /// <param name="paths">The path segments to combine.</param>
    /// <returns>A normalized path with platform-native separators.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?[])"/> with <see cref="PathOptions.Native"/>.
    /// </remarks>
    public static string CombineNative(params string?[] paths)
    {
        return Combine(PathOptions.Native, paths);
    }

    /// <summary>
    /// Combines two path segments using Windows conventions.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <returns>A normalized path with Windows-style backslashes.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?)"/> with <see cref="PathOptions.Windows"/>.
    /// </remarks>
    public static string CombineWindows(string? path1, string? path2)
    {
        return Combine(PathOptions.Windows, path1, path2);
    }

    /// <summary>
    /// Combines three path segments using Windows conventions.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <param name="path3">The third path segment.</param>
    /// <returns>A normalized path with Windows-style backslashes.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?, string?)"/> with <see cref="PathOptions.Windows"/>.
    /// </remarks>
    public static string CombineWindows(string? path1, string? path2, string? path3)
    {
        return Combine(PathOptions.Windows, path1, path2, path3);
    }

    /// <summary>
    /// Combines four path segments using Windows conventions.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <param name="path3">The third path segment.</param>
    /// <param name="path4">The fourth path segment.</param>
    /// <returns>A normalized path with Windows-style backslashes.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?, string?, string?)"/> with <see cref="PathOptions.Windows"/>.
    /// </remarks>
    public static string CombineWindows(string? path1, string? path2, string? path3, string? path4)
    {
        return Combine(PathOptions.Windows, path1, path2, path3, path4);
    }

    /// <summary>
    /// Combines multiple path segments using Windows conventions.
    /// </summary>
    /// <param name="paths">The path segments to combine.</param>
    /// <returns>A normalized path with Windows-style backslashes.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?[])"/> with <see cref="PathOptions.Windows"/>.
    /// </remarks>
    public static string CombineWindows(params string?[] paths)
    {
        return Combine(PathOptions.Windows, paths);
    }

    /// <summary>
    /// Combines two path segments using Unix conventions.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <returns>A normalized path with Unix-style forward slashes.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?)"/> with <see cref="PathOptions.Unix"/>.
    /// </remarks>
    public static string CombineUnix(string? path1, string? path2)
    {
        return Combine(PathOptions.Unix, path1, path2);
    }

    /// <summary>
    /// Combines three path segments using Unix conventions.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <param name="path3">The third path segment.</param>
    /// <returns>A normalized path with Unix-style forward slashes.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?, string?)"/> with <see cref="PathOptions.Unix"/>.
    /// </remarks>
    public static string CombineUnix(string? path1, string? path2, string? path3)
    {
        return Combine(PathOptions.Unix, path1, path2, path3);
    }

    /// <summary>
    /// Combines four path segments using Unix conventions.
    /// </summary>
    /// <param name="path1">The first path segment.</param>
    /// <param name="path2">The second path segment.</param>
    /// <param name="path3">The third path segment.</param>
    /// <param name="path4">The fourth path segment.</param>
    /// <returns>A normalized path with Unix-style forward slashes.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?, string?, string?, string?)"/> with <see cref="PathOptions.Unix"/>.
    /// </remarks>
    public static string CombineUnix(string? path1, string? path2, string? path3, string? path4)
    {
        return Combine(PathOptions.Unix, path1, path2, path3, path4);
    }

    /// <summary>
    /// Combines multiple path segments using Unix conventions.
    /// </summary>
    /// <param name="paths">The path segments to combine.</param>
    /// <returns>A normalized path with Unix-style forward slashes.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="Combine(PathOptions?, string?[])"/> with <see cref="PathOptions.Unix"/>.
    /// </remarks>
    public static string CombineUnix(params string?[] paths)
    {
        return Combine(PathOptions.Unix, paths);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Processes path segments according to the specified options.
    /// Applies filtering, trimming, and validation.
    /// </summary>
    private static List<string> ProcessSegments(PathOptions options, params string?[] paths)
    {
        var meaningful = new List<string>(paths.Length);

        foreach (var path in paths)
        {
            // Check for empty segments
            if (string.IsNullOrWhiteSpace(path))
            {
                if (options.ThrowOnEmptySegments)
                {
                    // Throw on empty segments when strict validation is required
                    throw new ArgumentException(
                        "Path segment is null, empty, or whitespace-only. " +
                        "Set ThrowOnEmptySegments to false to allow optional path segments.");
                }
                else
                {
                    // Silently skip empty segments
                    continue;
                }
            }

            // Process the segment
            var processed = options.TrimSegments ? path.Trim() : path;
            meaningful.Add(processed);
        }

        // Require at least one segment
        if (options.RequireAtLeastOneSegment && meaningful.Count == 0)
        {
            throw new ArgumentException("At least one non-empty path segment is required.");
        }

        // Validate first segment is absolute if required
        //
        // IMPORTANT: We use Path.IsPathFullyQualified() for the FIRST segment because it's the STRICTEST check.
        // It only returns true for paths that are complete and unambiguous on the current platform:
        //
        // On Windows, IsPathFullyQualified returns TRUE only for:
        //   - Absolute paths with drive letter: "C:\Windows\System32"
        //   - UNC network paths: "\\server\share\file.txt"
        //   - Device paths: "\\.\COM1", "\\?\C:\file.txt"
        //
        // On Windows, IsPathFullyQualified returns FALSE for:
        //   - Drive-relative paths: "C:file.txt" (relative to current directory on drive C:)
        //   - Root-relative paths: "\file.txt" (relative to current drive root)
        //   - Relative paths: "folder\file.txt", "..\file.txt"
        //
        // On Unix, IsPathFullyQualified returns TRUE only for:
        //   - Absolute paths: "/usr/bin/bash"
        //
        // On Unix, IsPathFullyQualified returns FALSE for:
        //   - Relative paths: "folder/file.txt", "../file.txt"
        //
        // This strict validation ensures the first segment provides an unambiguous starting point
        // for path resolution, preventing errors from accidentally using platform-specific relative paths.
        if (options.RequireAbsoluteFirstSegment && meaningful.Count > 0)
        {
            if (!Path.IsPathFullyQualified(meaningful[0]))
            {
                throw new ArgumentException(
                    $"First path segment '{meaningful[0]}' is not a fully qualified absolute path. " +
                    $"Set RequireAbsoluteFirstSegment to false to allow relative first segments.");
            }
        }

        // Validate subsequent segments are relative
        //
        // IMPORTANT: We use Path.IsPathRooted() for SUBSEQUENT segments because it's LOOSER than IsPathFullyQualified.
        // This means it catches MORE cases as "rooted" (which we want to reject), providing stricter validation.
        //
        // On Windows, IsPathRooted returns TRUE for:
        //   - Absolute paths with drive letter: "C:\Windows\System32" ✓
        //   - UNC network paths: "\\server\share\file.txt" ✓
        //   - Device paths: "\\.\COM1", "\\?\C:\file.txt" ✓
        //   - Drive-relative paths: "C:file.txt" ✓ (IMPORTANT: IsPathFullyQualified returns FALSE for this!)
        //   - Root-relative paths: "\file.txt" ✓ (IMPORTANT: IsPathFullyQualified returns FALSE for this!)
        //
        // On Windows, IsPathRooted returns FALSE only for:
        //   - Relative paths: "folder\file.txt", "..\file.txt"
        //
        // On Unix, IsPathRooted returns TRUE for:
        //   - Absolute paths: "/usr/bin" ✓
        //
        // On Unix, IsPathRooted returns FALSE for:
        //   - Relative paths: "folder/file.txt", "../file.txt"
        //
        // The key insight: IsPathRooted catches dangerous Windows-specific "partially qualified" paths
        // (like "C:file.txt" and "\file.txt") that IsPathFullyQualified would miss. These paths are
        // technically "rooted" but not "fully qualified", and they cause silent bugs when passed to Path.Combine
        // because they depend on ambient state (current drive, current directory on a drive).
        //
        // For subsequent segments, we want the STRICTEST possible check to reject anything that could
        // potentially override the base path. IsPathRooted provides this by catching all forms of
        // rooted paths, including the ambiguous Windows-specific ones.
        //
        // Example dangerous scenarios we prevent:
        //   Path.Combine("C:\\Base", "D:file.txt") → "D:file.txt" (silently discards "C:\\Base"!)
        //   Path.Combine("C:\\Base", "\\file.txt") → "\\file.txt" (silently discards "C:\\Base"!)
        if (options.ValidateSubsequentPathsRelative)
        {
            for (int i = 1; i < meaningful.Count; i++)
            {
                if (Path.IsPathRooted(meaningful[i]))
                {
                    throw new ArgumentException(
                        $"Path segment '{meaningful[i]}' at index {i} is an absolute or rooted path. " +
                        $"Only the first path segment can be absolute. Subsequent segments must be relative paths.");
                }
            }
        }

        return meaningful;
    }

    /// <summary>
    /// Applies normalization operations to a combined path according to the specified options.
    /// </summary>
    private static string ApplyNormalization(PathOptions options, string path)
    {
        var result = path;

        // Apply structural normalization (. and .. resolution)
        if (options.NormalizeStructure)
        {
            result = NormalizeStructure(result);
        }

        // Apply Unicode normalization (NFC)
        if (options.NormalizeUnicode)
        {
            result = NormalizeUnicode(result);
        }

        // Apply separator normalization
        if (options.NormalizeSeparators != PathSeparatorMode.Preserve)
        {
            result = NormalizeSeparators(result, options.NormalizeSeparators);
        }

        // Handle trailing separator
        if (options.TrailingSeparator != TrailingSeparatorHandling.Preserve)
        {
            result = HandleTrailingSeparator(result, options.TrailingSeparator);
        }

        return result;
    }

    #endregion
}
