namespace Nekote.Platform;

/// <summary>
/// Provides path manipulation utilities not available in <see cref="System.IO.Path"/>.
/// </summary>
/// <remarks>
/// <para>
/// This class only includes functionality that .NET's <see cref="System.IO.Path"/> class does not provide.
/// For standard path operations (Combine, GetFileName, GetExtension, etc.), use <see cref="System.IO.Path"/> directly.
/// </para>
/// <para>
/// The class is organized into partial files for AI-editing efficiency:
/// <list type="bullet">
/// <item><c>PathHelper.Combining.cs</c> - Path combining operations (complete feature following external specs)</item>
/// <item><c>PathHelper.Normalization.cs</c> - All normalization operations: structure, Unicode, separators, trailing (complete feature following external specs)</item>
/// <item><c>PathHelper.cs</c> - Utility methods that don't fit specific subcategories or aren't large enough for separate files</item>
/// </list>
/// Subcategories are created only for "complete features" that follow external specifications and are large enough to deserve their own file.
/// Anything else stays in the main file.
/// </para>
/// </remarks>
public static partial class PathHelper
{
    #region Path Analysis

    /// <summary>
    /// Gets the length of the root portion of the path.
    /// </summary>
    /// <param name="path">The path to analyze.</param>
    /// <returns>The length of the root portion, or 0 if the path is relative.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Windows Path Specifications (also supported cross-platform):</strong>
    /// </para>
    /// <para>
    /// <strong>1. DOS Device Paths</strong> (<c>\\.</c> prefix):
    /// <list type="bullet">
    /// <item><c>\\.\COM1</c> - Physical device (serial port). Root length: 4</item>
    /// <item><c>\\.\PhysicalDisk0</c> - Physical disk. Root length: 4</item>
    /// <item><c>\\.\C:\path</c> - Drive via device. Root length: 7 (includes drive and separator)</item>
    /// <item><c>//./COM1</c> - Same as above with forward slashes (extension for cross-platform tolerance)</item>
    /// </list>
    /// Purpose: Direct access to hardware devices, bypassing filesystem layers.
    /// </para>
    /// <para>
    /// <strong>2. Extended-Length Paths</strong> (<c>\\?</c> prefix):
    /// <list type="bullet">
    /// <item><c>\\?\C:\very\long\path</c> - Bypasses MAX_PATH (260 char) limit. Root length: 7</item>
    /// <item><c>\\?\UNC\server\share\file</c> - Extended UNC path. Root length: includes server and share</item>
    /// <item><c>//?/C:/path</c> - Same with forward slashes (extension)</item>
    /// </list>
    /// Purpose: Access paths longer than 260 characters. Disables path normalization (no . or .. processing by Windows).
    /// </para>
    /// <para>
    /// <strong>3. UNC (Universal Naming Convention) Paths</strong>:
    /// <list type="bullet">
    /// <item><c>\\server\share\path</c> - Network share. Root length: includes <c>\\server\share</c></item>
    /// <item><c>//server/share/path</c> - Same with forward slashes</item>
    /// <item>Server name: computer name or IP address</item>
    /// <item>Share name: shared folder name</item>
    /// </list>
    /// Purpose: Access network resources. Root includes server and share to uniquely identify network location.
    /// </para>
    /// <para>
    /// <strong>4. Drive Letter Paths</strong>:
    /// <list type="bullet">
    /// <item><c>C:\path</c> - Absolute path on drive C. Root length: 3</item>
    /// <item><c>C:path</c> - Relative to current directory on drive C (NOT fully qualified). Root length: 2</item>
    /// <item><c>D:/path</c> - Absolute with forward slash. Root length: 3</item>
    /// </list>
    /// Note: <c>C:</c> without separator is relative to current directory on that drive (dangerous, often unexpected).
    /// </para>
    /// <para>
    /// <strong>5. Root-Relative Paths</strong>:
    /// <list type="bullet">
    /// <item><c>\path</c> - Relative to current drive's root (on Windows). Root length: 1</item>
    /// <item><c>/path</c> - Absolute on Unix, root-relative on Windows. Root length: 1</item>
    /// </list>
    /// Dangerous on Windows: meaning depends on current drive.
    /// </para>
    /// <para>
    /// <strong>Cross-Platform Extensions:</strong>
    /// </para>
    /// <para>
    /// Unlike .NET's implementation (which only accepts backslashes for device paths), this accepts
    /// mixed separators (<c>/</c> and <c>\</c>) for cross-platform tolerance. This allows paths
    /// to be normalized with <c>ToUnixPath()</c> or <c>ToWindowsPath()</c> without losing semantic meaning.
    /// </para>
    /// <para>
    /// Examples: <c>//./COM1</c>, <c>\/?\C:/path</c>, <c>//server/share</c> are all recognized.
    /// </para>
    /// </remarks>
    internal static int GetRootLength(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return 0;
        }

        int pathLength = path.Length;
        int i = 0;

        // Check for device path patterns first (\\.\, \\?\, //./,  //?/)
        // This must be checked before UNC because device UNC is a special case
        bool isDevice = IsDevicePath(path);
        bool isDeviceUnc = isDevice && IsDeviceUNC(path);

        // Branch 1: UNC paths or simple rooted paths (starting with separator)
        // Handles: \\server\share, //server/share, \path, /path, \\?\UNC\server\share
        if ((!isDevice || isDeviceUnc) && pathLength > 0 && IsSeparator(path[0]))
        {
            // UNC or simple rooted path
            if (isDeviceUnc || (pathLength > 1 && IsSeparator(path[1])))
            {
                // UNC path detected (either regular or device UNC)
                // Device UNC: \\?\UNC\server\share → root includes entire \\?\UNC\server\share
                // Regular UNC: \\server\share → root includes entire \\server\share
                // Root must include server AND share because that uniquely identifies the network location

                i = isDeviceUnc ? 8 : 2; // Start after \\?\UNC\ or \\

                // Scan past server\share (two components separated by separator)
                // n=2 means we skip two separators: one after server, one after share
                // Example: \\server\share\path → scan past "server\share" → i points after second \
                int n = 2;
                while (i < pathLength && (!IsSeparator(path[i]) || --n > 0))
                {
                    i++;
                }
            }
            else
            {
                // Simple rooted path (starts with single separator)
                // Windows: \path (root-relative to current drive)
                // Unix: /path (absolute)
                // Root length: 1 (just the leading separator)
                i = 1;
            }
        }
        // Branch 2: Device paths (\\.\, \\?\) that are NOT device UNC
        // Handles: \\.\COM1, \\?\C:\path, //./PhysicalDisk0
        else if (isDevice)
        {
            // Device path: root includes the device prefix AND the device name (up to separator)
            // Examples:
            //   \\.\COM1 → root is entire path (4 chars)
            //   \\.\C:\path → root is \\.\C:\ (7 chars)
            //   \\?\C:\very\long\path → root is \\?\C:\ (7 chars)
            i = 4; // Start after \\.\  or \\?\

            // Scan to next separator (or end of path)
            // This captures the device name or drive letter
            while (i < pathLength && !IsSeparator(path[i]))
            {
                i++;
            }

            // If there's a separator after the device name, include it
            // This handles \\?\C:\ (include the \) vs \\.\COM1 (no separator to include)
            // i > 4 check ensures there's actually a device name (not just \\?\)
            if (i < pathLength && i > 4 && IsSeparator(path[i]))
            {
                i++;
            }
        }
        // Branch 3: Drive letter paths (C:, D:, etc.)
        // Handles: C:\path, D:/path, C:path (drive-relative)
        else if (pathLength >= 2 && path[1] == ':' && IsValidDriveChar(path[0]))
        {
            // Drive letter detected (A-Z or a-z followed by :)
            i = 2; // Root is at least "C:"

            // If followed by separator, include it (C:\ is absolute, C: is drive-relative)
            // C:\ → root length 3 (absolute path on C:)
            // C:path → root length 2 (relative to current directory on C:)
            // This distinction is important: C:path is dangerous because its meaning depends on current directory
            if (pathLength > 2 && IsSeparator(path[2]))
            {
                i++;
            }
        }

        // If none of the above matched, return 0 (relative path with no root)
        // Examples: path/to/file, dir\file, ../parent
        return i;
    }

    /// <summary>
    /// Checks if the path uses device path syntax.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><c>true</c> if the path starts with a device prefix; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Device Path Specifications:</strong>
    /// </para>
    /// <para>
    /// <strong>DOS Device Namespace</strong> (<c>\\.\</c>):
    /// <list type="bullet">
    /// <item>Format: <c>\\.\DeviceName</c> or <c>\\.\DriveLetter:\path</c></item>
    /// <item>Purpose: Direct access to device drivers and hardware</item>
    /// <item>Examples: <c>\\.\COM1</c>, <c>\\.\PhysicalDisk0</c>, <c>\\.\HarddiskVolume1</c></item>
    /// <item>Behavior: Bypasses normal filesystem, talks directly to device driver</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Win32 File Namespace (Extended-Length)</strong> (<c>\\?\</c>):
    /// <list type="bullet">
    /// <item>Format: <c>\\?\path</c></item>
    /// <item>Purpose: Bypass MAX_PATH limit (260 characters) and disable normalization</item>
    /// <item>Examples: <c>\\?\C:\very\long\path\...</c>, <c>\\?\UNC\server\share</c></item>
    /// <item>Behavior: No <c>.</c> or <c>..</c> processing, no forward slash conversion, no relative path support</item>
    /// <item>MAX_PATH includes null terminator, so practical limit is 259 displayable characters</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Security Note - Malicious Variants:</strong>
    /// </para>
    /// <para>
    /// Mixed-separator device paths like <c>/\\.\</c> or <c>\/?\</c> are NOT recognized by Windows
    /// but are accepted here for cross-platform tolerance. These pose minimal security risk because:
    /// <list type="bullet">
    /// <item>Windows filesystem will reject them anyway when accessed</item>
    /// <item>This library normalizes separators before filesystem access</item>
    /// <item>Pattern matching is exact (all 4 positions must match)</item>
    /// <item>The primary goal is reversible normalization (ToUnixPath ↔ ToWindowsPath)</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Cross-Platform Extension:</strong>
    /// </para>
    /// <para>
    /// Unlike .NET (which only accepts backslashes), this accepts any separator in all 4 positions.
    /// This allows: <c>ToUnixPath("\\.\COM1")</c> → <c>"//./COM1"</c> → <c>ToWindowsPath()</c> → <c>"\\.\COM1"</c>
    /// preserving semantic meaning through transformations.
    /// </para>
    /// </remarks>
    internal static bool IsDevicePath(string path)
    {
        return path.Length >= 4 &&
               IsSeparator(path[0]) &&
               IsSeparator(path[1]) &&
               (path[2] == '.' || path[2] == '?') &&
               IsSeparator(path[3]);
    }

    /// <summary>
    /// Checks if the path is a device UNC path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><c>true</c> if the path is a device UNC path; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Device UNC Path Specification:</strong>
    /// </para>
    /// <para>
    /// Format: <c>\\?\UNC\server\share\path</c> or <c>\\?\UNC\server\share</c>
    /// </para>
    /// <para>
    /// This is a hybrid format that combines:
    /// <list type="bullet">
    /// <item>Extended-length prefix (<c>\\?</c>) - Bypasses MAX_PATH limit (260 chars)</item>
    /// <item>UNC indicator (<c>UNC</c>) - Signals network path follows</item>
    /// <item>Network location (<c>server\share</c>) - Standard UNC server and share</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Why Device UNC Exists:</strong>
    /// </para>
    /// <para>
    /// Regular UNC paths (<c>\\server\share</c>) are limited to MAX_PATH (260 chars).
    /// To access long network paths, Windows requires the extended-length syntax.
    /// You can't just use <c>\\?\\server\share</c> (double separator) - that's invalid.
    /// Instead, Windows requires the explicit <c>UNC</c> marker: <c>\\?\UNC\server\share</c>.
    /// </para>
    /// <para>
    /// <strong>Conversion Examples:</strong>
    /// </para>
    /// <para>
    /// <c>\\server\share\path</c> → <c>\\?\UNC\server\share\path</c> (to bypass MAX_PATH)
    /// </para>
    /// <para>
    /// Root length includes the entire <c>\\?\UNC\server\share</c> portion (variable length)
    /// because server and share names identify the root network location.
    /// </para>
    /// <para>
    /// <strong>Case Insensitivity:</strong>
    /// </para>
    /// <para>
    /// This method accepts <c>UNC</c>, <c>unc</c>, <c>Unc</c>, etc. because Windows treats
    /// path components as case-insensitive on NTFS.
    /// </para>
    /// </remarks>
    internal static bool IsDeviceUNC(string path)
    {
        return path.Length >= 8 &&
               IsDevicePath(path) &&
               IsSeparator(path[7]) &&
               (path[4] == 'U' || path[4] == 'u') &&
               (path[5] == 'N' || path[5] == 'n') &&
               (path[6] == 'C' || path[6] == 'c');
    }

    /// <summary>
    /// Checks if a character is a valid drive letter.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns><c>true</c> if the character is A-Z or a-z; otherwise, <c>false</c>.</returns>
    internal static bool IsValidDriveChar(char c)
    {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }

    #endregion

    #region Internal Helpers

    /// <summary>
    /// Checks if a character is a path separator.
    /// </summary>
    private static bool IsSeparator(char c)
    {
        return c == '/' || c == '\\';
    }

    #endregion
}
