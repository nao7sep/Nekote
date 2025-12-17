using System.Text;

namespace Nekote.Platform;

/// <summary>
/// Provides path manipulation utilities not available in <see cref="System.IO.Path"/>.
/// </summary>
/// <remarks>
/// This class only includes functionality that .NET's <see cref="System.IO.Path"/> class does not provide.
/// For standard path operations (Combine, GetFileName, GetExtension, etc.), use <see cref="System.IO.Path"/> directly.
/// </remarks>
public static partial class PathHelper
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
    /// Normalizes path structure by resolving <c>.</c> (current directory) and <c>..</c> (parent directory) segments,
    /// and removing consecutive separators.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The path with <c>.</c> and <c>..</c> segments resolved and consecutive separators removed.</returns>
    /// <remarks>
    /// <para>
    /// This method resolves relative path segments while preserving the relative nature of the path:
    /// <list type="bullet">
    /// <item><c>.</c> (current directory) - removed from path</item>
    /// <item><c>..</c> (parent directory) - collapses with previous segment</item>
    /// <item>Consecutive separators - removed (e.g., <c>usr//bin</c> becomes <c>usr/bin</c>)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Example: <c>dir1/./dir2/../dir3//file</c> becomes <c>dir1/dir3/file</c>
    /// </para>
    /// <para>
    /// Correctly handles absolute paths including UNC network paths:
    /// <list type="bullet">
    /// <item>Unix absolute: <c>/usr/./bin</c> becomes <c>/usr/bin</c></item>
    /// <item>UNC network: <c>\\server\share\..\other</c> becomes <c>\\server\other</c></item>
    /// <item>Device paths: <c>\\.\COM1</c> and <c>\\?\C:\path</c> preserve their prefixes</item>
    /// <item>Invalid Unix double-slash: <c>//usr/bin</c> normalizes to <c>/usr/bin</c></item>
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
        string? prefix = null;
        string pathProcess = path;

        if (path.StartsWith(@"\\.\") || path.StartsWith(@"\\?\") || 
            path.StartsWith(@"//./") || path.StartsWith(@"//?/"))
        {
            prefix = path.Substring(0, 4);
            pathProcess = path.Substring(4);
        }

        // Determine separator style to preserve
        char separator = (prefix != null) 
            ? '\\' 
            : (pathProcess.Contains('/') ? '/' : '\\');

        // Split the path into segments
        var segments = pathProcess.Split(['/', '\\']);
        var stack = new List<string>(segments.Length);

        // Determine if the path is rooted (absolute) to handle ".." clamping
        // Rooted if starts with empty (leading separator) or drive letter (contains :)
        bool isRooted = segments.Length > 0 && 
                       (segments[0] == "" || 
                       (segments[0].Length >= 2 && segments[0][1] == ':' && char.IsLetter(segments[0][0])));

        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];

            if (segment == "")
            {
                // Handle empty segments (separators)
                // 1. Preserve leading separator (Root)
                if (i == 0) 
                {
                    stack.Add(segment);
                    continue;
                }
                // 2. Preserve second leading separator (UNC \\server)
                if (i == 1 && segments[0] == "")
                {
                    stack.Add(segment);
                    continue;
                }
                // 3. Preserve trailing separator (path/ -> path/)
                //    This allows TrailingSeparatorHandling options to work correctly later
                if (i == segments.Length - 1)
                {
                    stack.Add(segment);
                    continue;
                }
                
                // 4. Skip redundant internal separators (a//b -> a/b)
                continue;
            }
            
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
                // Regular segment
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
}
