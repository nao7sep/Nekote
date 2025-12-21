using System.Text;

namespace Nekote.Platform;

public static partial class PathHelper
{
    #region Structure Normalization

    /// <summary>
    /// Normalizes path structure by resolving <c>.</c> (current directory) and <c>..</c> (parent directory) segments,
    /// and removing consecutive separators.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The path with <c>.</c> and <c>..</c> segments resolved and consecutive separators removed.</returns>
    /// <exception cref="ArgumentException">Thrown when the path root is malformed (e.g., invalid device path or missing UNC server name).</exception>
    /// <remarks>
    /// <para>
    /// <strong>Normalization Operations:</strong>
    /// </para>
    /// <para>
    /// 1. <strong>Current Directory (<c>.</c>) Removal:</strong>
    /// <list type="bullet">
    /// <item><c>dir/./file</c> → <c>dir/file</c> (single dot is redundant)</item>
    /// <item><c>./file</c> → <c>file</c> (leading dot removed)</item>
    /// <item><c>path/.</c> → <c>path</c> (trailing dot removed)</item>
    /// <item>Pattern matched: <c>/./</c> where both slashes must be present (single trailing dot also matched)</item>
    /// </list>
    /// </para>
    /// <para>
    /// 2. <strong>Parent Directory (<c>..</c>) Resolution:</strong>
    /// </para>
    /// <para>
    /// For absolute/rooted paths (clamped at root):
    /// <list type="bullet">
    /// <item><c>/usr/../bin</c> → <c>/bin</c> (go up from usr to root, then into bin)</item>
    /// <item><c>/../../file</c> → <c>/file</c> (can't go above root, extra .. ignored)</item>
    /// <item><c>C:\Windows\..\Users</c> → <c>C:\Users</c> (same for Windows)</item>
    /// <item><c>\\server\share\..\other</c> → <c>\\server\other</c> (share is part of root)</item>
    /// <item><c>\\?\C:\dir\..\file</c> → <c>\\?\C:\file</c> (device paths also clamp)</item>
    /// </list>
    /// </para>
    /// <para>
    /// For relative paths (preserved):
    /// <list type="bullet">
    /// <item><c>dir1/dir2/../file</c> → <c>dir1/file</c> (normal collapse)</item>
    /// <item><c>../../file</c> → <c>../../file</c> (leading .. preserved, can't resolve without filesystem context)</item>
    /// <item><c>dir1/../../file</c> → <c>../file</c> (one .. collapses with dir1, other preserved)</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Security Rationale for Root Clamping:</strong>
    /// </para>
    /// <para>
    /// Absolute paths clamp at root because:
    /// <list type="bullet">
    /// <item><c>/../../etc/passwd</c> and <c>/../../../etc/passwd</c> both resolve to <c>/etc/passwd</c> on actual filesystems</item>
    /// <item>All major OSes (Linux, Windows, macOS) perform this clamping in their kernel/filesystem layer</item>
    /// <item>Preserving extra <c>..</c> could create false security expectations (path looks safe but isn't)</item>
    /// <item>No semantic difference between <c>/../file</c> and <c>/file</c> in practice</item>
    /// </list>
    /// </para>
    /// <para>
    /// 3. <strong>Consecutive Separator Removal:</strong>
    /// <list type="bullet">
    /// <item><c>usr//bin</c> → <c>usr/bin</c> (double slash has no special meaning on Windows, same as single)</item>
    /// <item><c>dir///file</c> → <c>dir/file</c> (any number of consecutive slashes → one)</item>
    /// <item><c>//server/share</c> → <c>//server/share</c> (UNC preserved - first two slashes are root)</item>
    /// <item><c>///usr/bin</c> → <c>/usr/bin</c> (invalid Unix triple-slash, normalized to absolute)</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Algorithm Design (Inspired by .NET's RemoveRelativeSegments):</strong>
    /// </para>
    /// <para>
    /// Uses single-pass streaming with lookahead:
    /// <list type="bullet">
    /// <item>Extract root using <see cref="GetRootLength"/> (handles device, UNC, drive, absolute)</item>
    /// <item>Scan for first separator after root to determine separator style (preserve user's choice)</item>
    /// <item>Stream character-by-character with <see cref="StringBuilder"/>, checking patterns at separators</item>
    /// <item>Lookahead patterns: <c>//</c> (consecutive), <c>/./</c> (current dir), <c>/../</c> (parent dir)</item>
    /// <item>For <c>..</c>: unwind StringBuilder to previous separator (efficient in-place modification)</item>
    /// <item>Root boundary check: <c>isRooted</c> flag determines clamp vs preserve behavior</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Edge Cases Handled:</strong>
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><c>C:</c> (drive without separator) - root is 2 chars, normalized as-is</item>
    /// <item><c>C:path\..\file</c> (drive-relative) - treated as rooted path (rootLength=2, clamps at root like <c>C:file</c>)</item>
    /// <item><c>\path</c> (root-relative Windows) - root is 1 char, treated as absolute</item>
    /// <item><c>\\.\Device\..\other</c> - device path, clamps at root (can't escape device namespace)</item>
    /// <item><c>\\?\UNC\server\share\..\other</c> - device UNC, clamps at <c>\\?\UNC\server\share</c></item>
    /// <item>Empty path, whitespace-only path - returned unchanged</item>
    /// <item>Path that is all root (<c>C:\</c>) - returned unchanged</item>
    /// <item>Trailing separators - preserved if present in final segment</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Difference from .NET's Path.GetFullPath:</strong>
    /// </para>
    /// <para>
    /// <see cref="Path.GetFullPath"/> resolves to absolute path using current directory and drive context.
    /// This method only normalizes structure without filesystem access or current directory resolution.
    /// Example: <c>dir/../file</c> stays relative; <c>GetFullPath</c> would prepend <c>C:\CurrentDir\</c>.
    /// </para>
    /// </remarks>
    public static string NormalizeStructure(string path, PathOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(path);
        options ??= PathOptions.Default;

        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        // Get the root length (device prefix, UNC, drive, or absolute path)
        // This tells us which part of the path is immutable and can't be escaped with ..
        int rootLength = GetRootLength(path, options, out _);

        if (rootLength >= path.Length)
        {
            // Path is all root (e.g., "C:\", "//server/share"), nothing to normalize
            return path;
        }

        var segments = new List<string>();

        // Process segments after root using slice-based approach
        ReadOnlySpan<char> remaining = path.AsSpan(rootLength);

        while (remaining.Length > 0)
        {
            // Skip leading separators (handles consecutive separators automatically)
            while (remaining.Length > 0 && IsSeparator(remaining[0]))
            {
                remaining = remaining.Slice(1);
            }

            if (remaining.Length == 0)
            {
                break;
            }

            // Parse current segment
            if (!ParseDelimitedSegment(remaining, requireDelimiter: false, out int segmentLength))
            {
                break;
            }

            // Extract segment name (without trailing separator)
            int nameLength = segmentLength;
            if (nameLength > 0 && IsSeparator(remaining[nameLength - 1]))
            {
                nameLength--;
            }

            ReadOnlySpan<char> segmentName = remaining.Slice(0, nameLength);

            // Handle special segments
            if (segmentName.Length == 1 && segmentName[0] == '.')
            {
                // Current directory - skip
                remaining = remaining.Slice(segmentLength);
                continue;
            }

            if (segmentName.Length == 2 && segmentName[0] == '.' && segmentName[1] == '.')
            {
                // Parent directory - try to unwind
                if (segments.Count > 0)
                {
                    var lastSegment = segments[segments.Count - 1];
                    if (lastSegment.Length != 3 || lastSegment[0] != '.' || lastSegment[1] != '.')
                    {
                        // Last segment is not .., can unwind
                        segments.RemoveAt(segments.Count - 1);
                    }
                    else if (rootLength == 0)
                    {
                        // Last segment is also .., and we're relative - add another ..
                        segments.Add(remaining.Slice(0, segmentLength).ToString());
                    }
                    // else: rooted path with .. on stack - shouldn't happen, but clamp
                }
                else if (rootLength == 0)
                {
                    // Relative path, no segments - preserve the ..
                    segments.Add(remaining.Slice(0, segmentLength).ToString());
                }
                // else: rooted path at root level, can't go above root - clamp (do nothing)

                remaining = remaining.Slice(segmentLength);
                continue;
            }

            // Regular segment - add it (with trailing separator if present)
            segments.Add(remaining.Slice(0, segmentLength).ToString());

            remaining = remaining.Slice(segmentLength);
        }

        // Combine root with normalized segments
        if (rootLength > 0)
        {
            return string.Concat(path.AsSpan(0, rootLength), string.Concat(segments));
        }

        return string.Concat(segments);
    }

    #endregion

    #region Separator Normalization

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
    /// Normalizes path separators according to the specified mode.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <param name="mode">The separator mode to apply.</param>
    /// <returns>The path with separators normalized.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item><see cref="PathSeparatorMode.Preserve"/> - No changes to separators</item>
    /// <item><see cref="PathSeparatorMode.Windows"/> - All separators become backslashes (<c>\</c>)</item>
    /// <item><see cref="PathSeparatorMode.Unix"/> - All separators become forward slashes (<c>/</c>)</item>
    /// <item><see cref="PathSeparatorMode.Native"/> - All separators become the platform's default separator</item>
    /// </list>
    /// </remarks>
    public static string NormalizeSeparators(string path, PathSeparatorMode mode)
    {
        ArgumentNullException.ThrowIfNull(path);

        return mode switch
        {
            PathSeparatorMode.Preserve => path,
            PathSeparatorMode.Windows => path.Replace('/', '\\'),
            PathSeparatorMode.Unix => path.Replace('\\', '/'),
            PathSeparatorMode.Native => PathSeparators.Native == '\\'
                ? path.Replace('/', '\\')  // Native is Windows: convert Unix to Windows
                : path.Replace('\\', '/'), // Native is Unix: convert Windows to Unix
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Invalid PathSeparatorMode value.")
        };
    }

    #endregion

    #region Trailing Separator Normalization

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

    /// <summary>
    /// Handles trailing path separators according to the specified mode.
    /// </summary>
    /// <param name="path">The path to process.</param>
    /// <param name="handling">How to handle trailing separators.</param>
    /// <returns>The path with trailing separator handling applied.</returns>
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

        if (string.IsNullOrWhiteSpace(path))
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

    #region Unicode Normalization

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

    #endregion

    #region Internal Normalization Orchestration

    internal static string ApplyNormalization(PathOptions options, string path)
    {
        if (options.NormalizeStructure)
        {
            path = NormalizeStructure(path, options);
        }

        if (options.NormalizeSeparators != PathSeparatorMode.Preserve)
        {
            path = NormalizeSeparators(path, options.NormalizeSeparators);
        }

        if (options.TrailingSeparator != TrailingSeparatorHandling.Preserve)
        {
            path = HandleTrailingSeparator(path, options.TrailingSeparator);
        }

        if (options.NormalizeUnicode)
        {
            path = NormalizeUnicode(path);
        }

        return path;
    }

    #endregion
}
