using System.Text;

namespace Nekote.Platform;

/// <summary>
/// Provides path manipulation utilities not available in <see cref="System.IO.Path"/>.
/// </summary>
/// <remarks>
/// This class only includes functionality that .NET's <see cref="System.IO.Path"/> class does not provide.
/// For standard path operations (Combine, GetFileName, GetExtension, etc.), use <see cref="System.IO.Path"/> directly.
/// </remarks>
public static class PathHelper
{
    #region Convenience Methods

    /// <summary>
    /// Converts all path separators in the specified path to Unix-style forward slashes (<c>/</c>).
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The path with all backslashes replaced by forward slashes.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="NormalizeSeparators"/> with <see cref="PathSeparatorMode.Unix"/>.
    /// </remarks>
    public static string ToUnixPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return NormalizeSeparators(path, PathSeparatorMode.Unix);
    }

    /// <summary>
    /// Converts all path separators in the specified path to Windows-style backslashes (<c>\</c>).
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The path with all forward slashes replaced by backslashes.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="NormalizeSeparators"/> with <see cref="PathSeparatorMode.Windows"/>.
    /// </remarks>
    public static string ToWindowsPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return NormalizeSeparators(path, PathSeparatorMode.Windows);
    }

    /// <summary>
    /// Converts all path separators in the specified path to the native separator for the current platform.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>
    /// On Windows, returns the path with forward slashes replaced by backslashes.
    /// On Unix-like systems, returns the path with backslashes replaced by forward slashes.
    /// </returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="NormalizeSeparators"/> with <see cref="PathSeparatorMode.Native"/>.
    /// </remarks>
    public static string ToNativePath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return NormalizeSeparators(path, PathSeparatorMode.Native);
    }

    /// <summary>
    /// Ensures the path has a trailing separator.
    /// </summary>
    /// <param name="path">The path to process.</param>
    /// <returns>The path with a trailing separator added if not already present.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="HandleTrailingSeparator"/> with <see cref="TrailingSeparatorHandling.Ensure"/>.
    /// The native platform separator is used when adding a trailing separator.
    /// </remarks>
    public static string EnsureTrailingSeparator(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return HandleTrailingSeparator(path, TrailingSeparatorHandling.Ensure);
    }

    /// <summary>
    /// Removes the trailing separator from the path if present.
    /// </summary>
    /// <param name="path">The path to process.</param>
    /// <returns>The path with any trailing separator removed.</returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="HandleTrailingSeparator"/> with <see cref="TrailingSeparatorHandling.Remove"/>.
    /// </remarks>
    public static string RemoveTrailingSeparator(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return HandleTrailingSeparator(path, TrailingSeparatorHandling.Remove);
    }

    #endregion

    #region Atomic Normalization Operations

    /// <summary>
    /// Normalizes Unicode characters in a path to NFC (Canonical Composition) form.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The path with Unicode characters normalized to NFC form.</returns>
    /// <remarks>
    /// <para>
    /// This normalization is critical for cross-platform applications, particularly when working with macOS.
    /// macOS file systems store filenames in NFD (decomposed) form, where characters like "café" are stored
    /// as separate base + combining characters (e + ´). This can cause string comparison failures and dictionary
    /// lookup misses when comparing paths across platforms.
    /// </para>
    /// <para>
    /// Normalizing to NFC ensures consistent string representation across all platforms, preventing
    /// "file not found" errors when paths are constructed on one platform and used on another.
    /// </para>
    /// </remarks>
    public static string NormalizeUnicode(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return path.Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Normalizes path structure by resolving <c>.</c> (current directory) and <c>..</c> (parent directory) segments.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The path with <c>.</c> and <c>..</c> segments resolved.</returns>
    /// <remarks>
    /// <para>
    /// This method resolves relative path segments while preserving the relative nature of the path:
    /// <list type="bullet">
    /// <item><c>.</c> (current directory) - removed from path</item>
    /// <item><c>..</c> (parent directory) - collapses with previous segment</item>
    /// </list>
    /// </para>
    /// <para>
    /// Example: <c>dir1/./dir2/../dir3</c> becomes <c>dir1/dir3</c>
    /// </para>
    /// <para>
    /// Correctly handles absolute paths including UNC network paths:
    /// <list type="bullet">
    /// <item>Unix absolute: <c>/usr/./bin</c> becomes <c>/usr/bin</c></item>
    /// <item>UNC network: <c>\\server\share\..\other</c> becomes <c>\\server\other</c></item>
    /// </list>
    /// </para>
    /// <para>
    /// Unlike <see cref="Path.GetFullPath"/>, this method does NOT convert the path to an absolute path.
    /// It only normalizes the structure of the existing path.
    /// </para>
    /// </remarks>
    public static string NormalizeStructure(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        // Handle Device Paths explicitly to preserve the "\\.\" or "\\?\" prefix
        // which contains a dot that would otherwise be removed.
        // Support both backslash and forward slash variants.
        string? prefix = null;
        string pathProcess = path;

        if (path.StartsWith(@"\\.\") || path.StartsWith(@"\\?\") ||
            path.StartsWith(@"//./") || path.StartsWith(@"//?/"))
        {
            prefix = path.Substring(0, 4);
            pathProcess = path.Substring(4);
        }

        // Determine separator style to preserve
        char separator = pathProcess.Contains('/') ? '/' : '\\';

        // Split the path into segments, keeping empty segments to preserve structure
        var segments = pathProcess.Split(['/', '\\']);
        var stack = new List<string>(segments.Length);

        // Determine if the path is rooted (absolute) to handle ".." clamping
        // Rooted if starts with empty (leading separator) or drive letter (X: at start)
        // Check for drive letter: single char followed by colon (e.g., "C:")
        bool isRooted = segments.Length > 0 &&
                       (segments[0] == "" ||
                        (segments[0].Length >= 2 && segments[0][1] == ':' && char.IsLetter(segments[0][0])));

        foreach (var segment in segments)
        {
            if (segment == ".")
            {
                // Current directory reference - skip it
                continue;
            }
            else if (segment == "..")
            {
                // Parent directory reference - try to pop
                // Only pop if there's a non-empty, non-".." segment to remove
                if (stack.Count > 0 && stack[^1] != "" && stack[^1] != "..")
                {
                    stack.RemoveAt(stack.Count - 1);
                }
                else
                {
                    // Can't pop.
                    // If we are NOT rooted, we must preserve ".." (e.g. "../../file.txt")
                    // If we ARE rooted, ".." at root is ignored (clamped) (e.g. "/../file" -> "/file")
                    if (!isRooted)
                    {
                        stack.Add(segment);
                    }
                }
            }
            else
            {
                // Regular segment (including empty ones that preserve leading separators)
                stack.Add(segment);
            }
        }

        var normalized = string.Join(separator, stack);

        return prefix != null ? prefix + normalized : normalized;
    }

    /// <summary>
    /// Normalizes path separators according to the specified mode.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <param name="mode">The separator normalization mode.</param>
    /// <returns>The path with separators normalized according to the specified mode.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item><see cref="PathSeparatorMode.Preserve"/> - No changes to separators</item>
    /// <item><see cref="PathSeparatorMode.Native"/> - Convert to platform-native separator</item>
    /// <item><see cref="PathSeparatorMode.Unix"/> - Convert all to forward slash (<c>/</c>)</item>
    /// <item><see cref="PathSeparatorMode.Windows"/> - Convert all to backslash (<c>\</c>)</item>
    /// </list>
    /// </remarks>
    public static string NormalizeSeparators(string path, PathSeparatorMode mode)
    {
        ArgumentNullException.ThrowIfNull(path);

        return mode switch
        {
            PathSeparatorMode.Preserve => path,
            PathSeparatorMode.Unix => path.Replace(PathSeparators.Windows, PathSeparators.Unix),
            PathSeparatorMode.Windows => path.Replace(PathSeparators.Unix, PathSeparators.Windows),
            PathSeparatorMode.Native => PathSeparators.Native == PathSeparators.Windows
                ? path.Replace(PathSeparators.Unix, PathSeparators.Windows)
                : path.Replace(PathSeparators.Windows, PathSeparators.Unix),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Invalid PathSeparatorMode value.")
        };
    }

    /// <summary>
    /// Handles trailing path separators according to the specified mode.
    /// </summary>
    /// <param name="path">The path to process.</param>
    /// <param name="handling">The trailing separator handling mode.</param>
    /// <returns>The path with trailing separator handled according to the specified mode.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item><see cref="TrailingSeparatorHandling.Preserve"/> - No changes to trailing separator</item>
    /// <item><see cref="TrailingSeparatorHandling.Remove"/> - Remove trailing separator if present</item>
    /// <item><see cref="TrailingSeparatorHandling.Ensure"/> - Ensure trailing separator is present</item>
    /// </list>
    /// </remarks>
    public static string HandleTrailingSeparator(string path, TrailingSeparatorHandling handling)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        return handling switch
        {
            TrailingSeparatorHandling.Preserve => path,
            TrailingSeparatorHandling.Remove => path.TrimEnd('/', '\\'),
            TrailingSeparatorHandling.Ensure => path.TrimEnd('/', '\\') + PathSeparators.Native,
            _ => throw new ArgumentOutOfRangeException(nameof(handling), handling, "Invalid TrailingSeparatorHandling value.")
        };
    }

    #endregion

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
        // The key insight: IsPathRooted catches dangerous Windows-specific "partially qualified" paths
        // (like "C:file.txt" and "\file.txt") that IsPathFullyQualified would miss. These paths are
        // technically "rooted" but not "fully qualified", and they cause silent bugs when passed to Path.Combine
        // because they depend on ambient state (current drive, current directory on a drive).
        //
        // For subsequent segments, we want the STRICTEST possible check to reject anything that could
        // potentially override the base path. IsPathRooted provides this by catching all forms of
        // rooted paths, including the ambiguous Windows-specific ones.
        //
        // Example dangerous scenario we prevent:
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
