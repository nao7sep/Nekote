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
    /// <item><c>C:path\..\file</c> (drive-relative) - treated as relative path (rootLength=2, but isRooted based on trailing separator)</item>
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
    public static string NormalizeStructure(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        // Get the root length (device prefix, UNC, drive, or absolute path)
        // This tells us which part of the path is immutable and can't be escaped with ..
        int rootLength = GetRootLength(path);

        if (rootLength >= path.Length)
        {
            // Path is all root (e.g., "C:\", "//server/share"), nothing to normalize
            return path;
        }

        // Determine separator to use: scan for first separator after root
        // Preserves the user's separator style (they may have mixed / and \ for a reason)
        // Example: \\?\C:/path/to/file preserves / because that's what appears first
        char separator = '\\'; // default
        for (int i = rootLength; i < path.Length; i++)
        {
            if (path[i] == '/')
            {
                separator = '/';
                break;
            }
            else if (path[i] == '\\')
            {
                separator = '\\';
                break;
            }
        }

        // Use streaming approach similar to .NET's RemoveRelativeSegments
        // Instead of tokenizing into segments, stream character-by-character with lookahead
        // This is more efficient (single-pass, single allocation) and handles edge cases better
        var sb = new StringBuilder(path.Length);

        // Append root portion
        int skip = rootLength;

        // Adjust skip position: if root ends with separator, back up one
        // so we can properly handle the separator in the main loop
        // Example: "C:\" has rootLength=3, but we want skip=2 so we can process the \ in the loop
        // This ensures consistent separator handling (consecutive separator removal, etc.)
        if (skip > 0 && IsSeparator(path[skip - 1]))
        {
            skip--;
        }

        if (skip > 0)
        {
            sb.Append(path.AsSpan(0, skip));
        }

        // isRooted determines whether we clamp .. at root (absolute) or preserve them (relative)
        // C:\, /usr, \\server\share are rooted; dir/path, C:path are not
        bool isRooted = rootLength > 0;

        // Stream through the path, handling /./  and /../ patterns
        // We only look for patterns at separators (can't have /. or /.. in middle of segment)
        for (int i = skip; i < path.Length; i++)
        {
            char c = path[i];

            // Lookahead for patterns: //, /./,  /../
            // Only check when we're at a separator and there's at least one more character
            if (IsSeparator(c) && i + 1 < path.Length)
            {
                // Pattern: // (consecutive separators)
                // Skip the current separator, continue to next iteration
                // Result: "usr//bin" → "usr/bin"
                if (IsSeparator(path[i + 1]))
                {
                    continue;
                }

                // Pattern: /./  (current directory reference)
                // Conditions: separator + dot + (separator OR end-of-string)
                // Examples: "dir/./file" → "dir/file", "dir/." → "dir"
                if ((i + 2 == path.Length || IsSeparator(path[i + 2])) &&
                    path[i + 1] == '.')
                {
                    i++; // Skip the dot
                    continue;
                }

                // Pattern: /../ (parent directory reference)
                // Conditions: separator + dot + dot + (separator OR end-of-string)
                // This is where the unwinding magic happens
                if (i + 2 < path.Length &&
                    (i + 3 == path.Length || IsSeparator(path[i + 3])) &&
                    path[i + 1] == '.' && path[i + 2] == '.')
                {
                    // Unwind back to last separator in the StringBuilder
                    // We're effectively removing the last segment
                    // Example: StringBuilder contains "usr/local/", we search back for the / before "local"
                    int s;
                    for (s = sb.Length - 1; s >= skip; s--)
                    {
                        if (IsSeparator(sb[s]))
                        {
                            // Found separator - truncate StringBuilder here
                            // Special case: if this is the last .. in the path and we're at root separator,
                            // preserve the separator (otherwise "/usr/.." would become "" instead of "/")
                            sb.Length = (i + 3 >= path.Length && s == skip) ? s + 1 : s;
                            break;
                        }
                    }

                    // Couldn't unwind (s < skip means we've searched back to the root boundary)
                    if (s < skip)
                    {
                        // Can't unwind further - behavior depends on whether path is rooted
                        if (isRooted)
                        {
                            // Rooted path: clamp at root (ignore the ..)
                            // Example: "/../../file" → "/file" (can't go above root)
                            // Security: prevents path traversal attacks, matches OS behavior
                            sb.Length = skip;
                        }
                        else
                        {
                            // Relative path: preserve the .. because we might be able to resolve it later
                            // Example: "../../file" stays as "../../file"
                            // We don't know the current directory, so we can't resolve this now
                            if (sb.Length > 0 && !IsSeparator(sb[sb.Length - 1]))
                            {
                                sb.Append(separator);
                            }
                            sb.Append("..");
                        }
                    }

                    i += 2; // Skip the two dots (..)
                    continue;
                }
            }

            // No pattern matched - append character as-is
            sb.Append(c);
        }

        // Restore root separator if we removed it during normalization
        // This handles cases where the normalized result is shorter than the root
        // Example: "C:\.." should remain "C:\" not "C:"
        if (skip != rootLength && sb.Length < rootLength)
        {
            sb.Append(path[rootLength - 1]);
        }

        return sb.ToString();
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
            PathSeparatorMode.Native => path.Replace(PathSeparators.NonNative, PathSeparators.Native),
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
            path = NormalizeStructure(path);
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
