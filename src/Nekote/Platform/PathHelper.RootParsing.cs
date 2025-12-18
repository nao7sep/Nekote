namespace Nekote.Platform;

/// <summary>
/// Root detection and parsing methods for PathHelper.
/// Handles Windows path root formats: device paths, extended-length paths, UNC paths, drive letters, and simple roots.
/// </summary>
public static partial class PathHelper
{
    #region Root Length Detection

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
    /// <item><c>\\.\COM1</c> - Physical device (serial port). Root length: 8 (4 for prefix + 4 for 'COM1'), or 9 with trailing separator</item>
    /// <item><c>\\.\PhysicalDisk0</c> - Physical disk. Root length: 18 (4 for prefix + 14 for 'PhysicalDisk0'), or 19 with trailing separator</item>
    /// <item><c>\\.\C:\path</c> - Drive via device. Root length: 7 (includes drive and separator)</item>
    /// <item><c>//./COM1</c> - Same as above with forward slashes (extension for cross-platform tolerance)</item>
    /// </list>
    /// Purpose: Direct access to hardware devices, bypassing filesystem layers.
    /// Note: Root length includes the full device name, not just the 4-character prefix. If a trailing separator
    /// follows the device name, it is included in the root length so subsequent path operations start fresh.
    /// </para>
    /// <para>
    /// <strong>2. Extended-Length Paths</strong> (<c>\\?</c> prefix):
    /// <list type="bullet">
    /// <item><c>\\?\C:\very\long\path</c> - Bypasses MAX_PATH (260 char) limit. Root length: 7 (includes trailing separator)</item>
    /// <item><c>\\?\UNC\server\share\file</c> - Extended UNC path. Root length: includes server and share with trailing separator if present</item>
    /// <item><c>//?/C:/path</c> - Same with forward slashes (extension)</item>
    /// </list>
    /// Purpose: Access paths longer than 260 characters. Windows itself disables path normalization (no . or .. processing),
    /// but PathHelper normalizes them when requested via <see cref="NormalizeStructure"/> regardless of prefix type.
    /// This design follows .NET's PathInternal.RemoveRelativeSegments method (see https://source.dot.net/#System.Private.CoreLib/src/libraries/Common/src/System/IO/PathInternal.cs),
    /// which accepts a rootLength parameter and normalizes relative segments (. and ..) without caring about the root's
    /// content or whether it's an extended-length path. The method signature is:
    /// <c>internal static bool RemoveRelativeSegments(ReadOnlySpan&lt;char&gt; path, int rootLength, ref ValueStringBuilder sb)</c>
    /// PathHelper's normalization behavior is intentionally based on this design, not an oversight.
    /// Note: Any trailing separator after the root components is included in the root length.
    /// </para>
    /// <para>
    /// <strong>3. UNC (Universal Naming Convention) Paths</strong>:
    /// <list type="bullet">
    /// <item><c>\\server\share\path</c> - Network share. Root length: includes <c>\\server\share</c> plus trailing separator if present</item>
    /// <item><c>//server/share/path</c> - Same with forward slashes</item>
    /// <item>Server name: computer name or IP address</item>
    /// <item>Share name: shared folder name</item>
    /// </list>
    /// Purpose: Access network resources. Root includes server and share to uniquely identify network location.
    /// Note: The trailing separator after share name (if present) is included in the root length.
    /// </para>
    /// <para>
    /// <strong>4. Drive Letter Paths</strong>:
    /// <list type="bullet">
    /// <item><c>C:\path</c> - Absolute path on drive C. Root length: 3 (includes trailing separator)</item>
    /// <item><c>C:path</c> - Relative to current directory on drive C (NOT fully qualified). Root length: 2 (no separator)</item>
    /// <item><c>D:/path</c> - Absolute with forward slash. Root length: 3 (includes trailing separator)</item>
    /// </list>
    /// Note: <c>C:</c> without separator is relative to current directory on that drive (dangerous, often unexpected).
    /// The trailing separator (if present) is included in the root length.
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
    /// <para>
    /// <strong>Trailing Separator Handling:</strong>
    /// </para>
    /// <para>
    /// When a trailing separator follows any root component (device name, share name, drive letter with separator),
    /// it is included in the root length. This ensures subsequent path operations start with a clean slate.
    /// For example: <c>\\server\share\</c> includes the final separator in the root, so the next segment
    /// can be appended directly without concerns about separator doubling.
    /// </para>
    /// </remarks>
    internal static int GetRootLength(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return 0;
        }

        ReadOnlySpan<char> span = path.AsSpan();

        // Try each root type in order of specificity
        if (TryParseDeviceOrExtendedSegment(span, out int length)) return length;
        if (TryParseDriveSegment(span, out length)) return length;
        if (TryParseUncRootSegment(span, out length)) return length;
        if (TryParseRootSegment(span, out length)) return length;

        return 0; // Relative path
    }

    #endregion

    #region Root Detection (Is* predicates)

    /// <summary>
    /// Checks if the path starts with a DOS device namespace prefix (\\.).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>DOS Device Namespace Specification:</strong>
    /// </para>
    /// <para>
    /// Format: <c>\\.\DeviceName</c> or <c>\\.\DriveLetter:\path</c>
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>\\.\COM1</c> - Serial port device</item>
    /// <item><c>\\.\PhysicalDisk0</c> - Physical disk access</item>
    /// <item><c>\\.\C:\path</c> - Drive through device namespace</item>
    /// <item><c>\\.\UNC\server\share</c> - UNC through device namespace (.NET explicitly supports this)</item>
    /// <item><c>//./COM1</c> - Cross-platform variant (accepted for tolerance)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Purpose: Direct access to hardware devices and drivers, bypassing normal filesystem layers.
    /// </para>
    /// <para>
    /// Note: This accepts any separator in all 4 positions (\, /) for cross-platform tolerance,
    /// even though Windows only recognizes backslashes. This enables reversible path normalization.
    /// .NET's Path implementation explicitly recognizes both <c>\\?\UNC\</c> and <c>\\.\UNC\</c>
    /// as valid device UNC paths.
    /// </para>
    /// </remarks>
    private static bool IsDeviceSegment(ReadOnlySpan<char> path)
    {
        // Pattern: sep-sep-dot-sep (where sep can be / or \)
        return path.Length >= 4 &&
               IsSeparator(path[0]) &&
               IsSeparator(path[1]) &&
               path[2] == '.' &&
               IsSeparator(path[3]);
    }

    /// <summary>
    /// Checks if the path starts with an extended-length or NT native path prefix (\\?\ or \??\).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Extended-Length and NT Native Path Specification:</strong>
    /// </para>
    /// <para>
    /// <strong>1. Win32 File Namespace (Extended-Length): <c>\\?\</c></strong>
    /// </para>
    /// <para>
    /// Format: <c>\\?\path</c>
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>\\?\C:\very\long\path</c> - Bypass MAX_PATH (260 char) limit</item>
    /// <item><c>\\?\UNC\server\share</c> - Extended-length UNC path</item>
    /// <item><c>//?/C:/path</c> - Cross-platform variant (accepted for tolerance)</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>2. NT Native Path Prefix: <c>\??\</c></strong>
    /// </para>
    /// <para>
    /// Format: <c>\??\path</c>
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>\??\C:\path</c> - NT native path to drive</item>
    /// <item><c>\??\UNC\server\share</c> - NT native UNC path</item>
    /// <item><c>/??/C:/path</c> - Cross-platform variant (accepted for tolerance)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Purpose: Both prefixes disable Windows path processing (no . or .. resolution,
    /// no / to \ conversion, no MAX_PATH limit). The <c>\??\</c> prefix is the internal
    /// NT object manager format that <c>\\?\</c> gets converted to by Windows. .NET's Path
    /// implementation explicitly recognizes both forms.
    /// </para>
    /// <para>
    /// Note: This accepts separators and question marks in positions 0-1 for cross-platform
    /// tolerance, even though Windows only recognizes exact backslashes. While Windows will
    /// only skip normalization for exactly <c>\\?\</c> or <c>\??\</c>, accepting mixed
    /// separators enables reversible path normalization in cross-platform scenarios.
    /// </para>
    /// </remarks>
    private static bool IsExtendedSegment(ReadOnlySpan<char> path)
    {
        // Pattern: sep-[sep|?]-question-sep
        // Accepts: \\?\, \??\, //?/, /??\, etc.
        return path.Length >= 4 &&
               IsSeparator(path[0]) &&
               (IsSeparator(path[1]) || path[1] == '?') &&
               path[2] == '?' &&
               IsSeparator(path[3]);
    }

    /// <summary>
    /// Checks if the path starts with a drive letter (e.g., C:, D:).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Drive Letter Path Specification:</strong>
    /// </para>
    /// <para>
    /// Format: <c>[A-Za-z]:</c> followed by optional separator
    /// </para>
    /// <para>
    /// Two variants:
    /// <list type="bullet">
    /// <item><c>C:\path</c> - Absolute path on drive C (separator present)</item>
    /// <item><c>C:path</c> - Relative to current directory on drive C (NO separator - dangerous!)</item>
    /// </list>
    /// </para>
    /// <para>
    /// The drive-relative form (<c>C:path</c>) is problematic because its meaning depends on
    /// the current directory on that drive, which is process-specific state. Most code should
    /// avoid this form.
    /// </para>
    /// <para>
    /// This method only checks for the <c>Letter:</c> part. The caller must check for the
    /// trailing separator to distinguish absolute from drive-relative.
    /// </para>
    /// </remarks>
    private static bool IsDriveSegment(ReadOnlySpan<char> path)
    {
        // Pattern: [A-Za-z]:
        return path.Length >= 2 &&
               IsValidDriveChar(path[0]) &&
               path[1] == ':';
    }

    /// <summary>
    /// Checks if the path starts with a UNC (Universal Naming Convention) prefix (\\).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>UNC Path Specification:</strong>
    /// </para>
    /// <para>
    /// Format: <c>\\server\share\path</c> or <c>//server/share/path</c>
    /// </para>
    /// <para>
    /// Components:
    /// <list type="bullet">
    /// <item><strong>Server</strong>: Computer name, IP address, or hostname</item>
    /// <item><strong>Share</strong>: Shared folder name on the server</item>
    /// </list>
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>\\server\share\file.txt</c> - Standard UNC path</item>
    /// <item><c>//server/share/file.txt</c> - Cross-platform variant</item>
    /// <item><c>\\192.168.1.100\share</c> - UNC with IP address</item>
    /// </list>
    /// </para>
    /// <para>
    /// Purpose: Access network resources. The root includes both server and share because
    /// together they uniquely identify a network location.
    /// </para>
    /// <para>
    /// Note: This only checks for the double-separator prefix. Full validation (non-empty
    /// server/share) happens in TryParseUncRootSegment.
    /// </para>
    /// </remarks>
    private static bool IsUncRootSegment(ReadOnlySpan<char> path)
    {
        // Pattern: sep-sep (where sep can be / or \)
        return path.Length >= 2 &&
               IsSeparator(path[0]) &&
               IsSeparator(path[1]);
    }

    /// <summary>
    /// Checks if the path starts with a single separator (/ or \).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Root-Relative Path Specification:</strong>
    /// </para>
    /// <para>
    /// Meaning varies by platform:
    /// <list type="bullet">
    /// <item><strong>Unix/Linux/macOS</strong>: <c>/path</c> is absolute (root of filesystem)</item>
    /// <item><strong>Windows</strong>: <c>\path</c> is relative to current drive's root (dangerous!)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>/usr/bin</c> - Absolute on Unix, root-relative on Windows</item>
    /// <item><c>\Windows</c> - Root-relative (if current drive is C:, means C:\Windows)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Windows danger: The meaning of <c>\path</c> depends on the current drive. If the
    /// current drive is D:, <c>\path</c> means <c>D:\path</c>. This is process-specific state.
    /// </para>
    /// <para>
    /// Note: This method doesn't distinguish between Unix absolute and Windows root-relative.
    /// That distinction requires OS context.
    /// </para>
    /// </remarks>
    private static bool IsRootSegment(ReadOnlySpan<char> path)
    {
        // Pattern: single separator (/ or \)
        return path.Length >= 1 && IsSeparator(path[0]);
    }

    #endregion

    #region Root Parsing (TryParse* methods)

    /// <summary>
    /// Parses a delimited segment from the start of the path.
    /// A segment is a sequence of non-separator characters optionally followed by a separator.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Segment Parsing Specification:</strong>
    /// </para>
    /// <para>
    /// This is the core primitive for parsing path components like server names, share names,
    /// and device names. A "delimited segment" consists of:
    /// <list type="number">
    /// <item>One or more non-separator characters (the segment content)</item>
    /// <item>Optionally: a trailing separator (/ or \)</item>
    /// </list>
    /// </para>
    /// <para>
    /// The <paramref name="requireDelimiter"/> parameter controls end-of-path behavior:
    /// <list type="bullet">
    /// <item><c>requireDelimiter = true</c>: Segment MUST have trailing separator. Used when
    /// more components expected (e.g., after UNC server, before share).</item>
    /// <item><c>requireDelimiter = false</c>: Trailing separator is optional. Used for the
    /// final component (e.g., UNC share name, device name).</item>
    /// </list>
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>"server\"</c>, requireDelimiter=true → length=7 (includes separator)</item>
    /// <item><c>"server"</c>, requireDelimiter=true → returns false (no separator)</item>
    /// <item><c>"share"</c>, requireDelimiter=false → length=5 (end of string)</item>
    /// <item><c>"share\path"</c>, requireDelimiter=false → length=6 (includes separator)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Validation: Returns false if the segment is empty (no non-separator characters found).
    /// This prevents accepting paths like <c>\\\share</c> (empty server name).
    /// </para>
    /// </remarks>
    /// <param name="path">The path to parse. Should be sliced to start at the desired position.</param>
    /// <param name="requireDelimiter">Whether the segment must be followed by a separator.
    /// No default value is provided - callers must explicitly choose based on context.</param>
    /// <param name="length">Receives the length of the segment (and separator if present).</param>
    /// <returns>True if a valid non-empty segment was found; otherwise, false.</returns>
    private static bool TryParseDelimitedSegment(
        ReadOnlySpan<char> path,
        bool requireDelimiter,
        out int length)
    {
        length = 0;

        if (path.Length == 0)
        {
            return false; // No content to parse
        }

        int i = 0;

        // Scan until we hit a separator or end of path
        while (i < path.Length && !IsSeparator(path[i]))
        {
            i++;
        }

        // Check if segment is empty
        if (i == 0)
        {
            return false; // Empty segment
        }

        // Check delimiter requirement
        if (i < path.Length)
        {
            // Separator found - include it
            i++;
            length = i;
            return true;
        }
        else
        {
            // Reached end of path without separator
            if (requireDelimiter)
            {
                return false; // Delimiter was required but not found
            }

            length = i;
            return true;
        }
    }

    /// <summary>
    /// Parses a device (\\.\), extended-length (\\?\), or NT native (\??\) path prefix,
    /// including any embedded UNC or drive components.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Device and Extended-Length Path Specification:</strong>
    /// </para>
    /// <para>
    /// <strong>1. DOS Device Namespace (\\.\)</strong>
    /// </para>
    /// <para>
    /// Format: <c>\\.\DeviceName</c>
    /// </para>
    /// <para>
    /// Purpose: Direct access to devices and drivers. Examples:
    /// <list type="bullet">
    /// <item><c>\\.\COM1</c> - Serial port</item>
    /// <item><c>\\.\PhysicalDisk0</c> - Raw disk access</item>
    /// <item><c>\\.\C:</c> - Drive through device namespace</item>
    /// <item><c>\\.\UNC\server\share</c> - UNC through device namespace</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>2. Win32 File Namespace (Extended-Length): <c>\\?\</c></strong>
    /// </para>
    /// <para>
    /// Format: <c>\\?\path</c>
    /// </para>
    /// <para>
    /// Purpose: Bypass Windows path processing:
    /// <list type="bullet">
    /// <item>No . or .. resolution</item>
    /// <item>No / to \ conversion</item>
    /// <item>No MAX_PATH (260 char) limit - supports up to 32,767 chars</item>
    /// <item>No short name (~1 style) generation</item>
    /// </list>
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>\\?\C:\very\long\path\exceeding\260\characters</c> - Long path</item>
    /// <item><c>\\?\UNC\server\share</c> - Extended-length UNC</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>3. NT Native Path Prefix: <c>\??\</c></strong>
    /// </para>
    /// <para>
    /// Format: <c>\??\path</c>
    /// </para>
    /// <para>
    /// Purpose: Internal NT object manager format. Provides same benefits as <c>\\?\</c>.
    /// Windows internally converts <c>\\?\</c> to <c>\??\</c>. .NET explicitly supports both.
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>\??\C:\path</c> - NT native path</item>
    /// <item><c>\??\UNC\server\share</c> - NT native UNC</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>4. Device/Extended UNC Paths</strong>
    /// </para>
    /// <para>
    /// All namespaces support UNC: <c>\\.\UNC\server\share</c>, <c>\\?\UNC\server\share</c>,
    /// and <c>\??\UNC\server\share</c>
    /// </para>
    /// <para>
    /// Note: Windows documentation is inconsistent about whether <c>\\.\UNC\</c> is valid,
    /// but .NET accepts all forms, so we do too. The "UNC" marker is case-insensitive.
    /// </para>
    /// <para>
    /// <strong>CRITICAL SECURITY BOUNDARY:</strong> For device/extended UNC paths, the share
    /// is a permission boundary. Path traversal with <c>..</c> must never escape above
    /// <c>\\?\UNC\server\share\</c> as this would enable unauthorized access to other shares.
    /// See <c>TryParseUncRootSegment</c> documentation for details.
    /// </para>
    /// <para>
    /// <strong>5. Device/Extended Drive Paths</strong>
    /// </para>
    /// <para>
    /// Drive letters can appear after any prefix: <c>\\.\C:\path</c>, <c>\\?\C:\path</c>,
    /// or <c>\??\C:\path</c>. The drive root (e.g., <c>C:\</c>) is also an immutable boundary
    /// for path normalization.
    /// </para>
    /// <para>
    /// <strong>Implementation Details:</strong>
    /// </para>
    /// <para>
    /// This method handles all variants:
    /// <list type="number">
    /// <item>Check for device (\\.\), extended (\\?\), or NT native (\??\) prefix (4 chars)</item>
    /// <item>Slice to get the rest after the prefix</item>
    /// <item>Check for drive letter first (e.g., C:)</item>
    /// <item>If not drive: try to parse a delimited segment with delimiter required</item>
    /// <item>If segment is exactly 4 chars and starts with "UNC" (case-insensitive), parse UNC server\share</item>
    /// <item>Otherwise: include the device name as part of the root</item>
    /// </list>
    /// </para>
    /// <para>
    /// Cross-platform tolerance: This method accepts mixed separators in the UNC server/share
    /// parsing to enable reversible normalization of user-provided paths.
    /// </para>
    /// </remarks>
    /// <param name="path">The path to parse.</param>
    /// <param name="length">Receives the length of the complete device/extended root.</param>
    /// <returns>True if the path starts with a device or extended-length prefix; otherwise, false.</returns>
    private static bool TryParseDeviceOrExtendedSegment(ReadOnlySpan<char> path, out int length)
    {
        length = 0;

        if (!IsDeviceSegment(path) && !IsExtendedSegment(path))
        {
            return false;
        }

        int i = 4; // Length of prefix: \\.\, \\?\, or \??\

        // Slice to get the rest after the prefix
        ReadOnlySpan<char> rest = path.Slice(4);

        // Check for drive letter first
        if (TryParseDriveSegment(rest, out int driveLength))
        {
            i += driveLength;
            length = i;
            return true;
        }

        // Try to parse next segment with delimiter required
        if (TryParseDelimitedSegment(rest, requireDelimiter: true, out int segmentLength))
        {
            // Check if it's the UNC marker (4 chars: U, N, C, separator)
            if (segmentLength == 4 &&
                (rest[0] == 'U' || rest[0] == 'u') &&
                (rest[1] == 'N' || rest[1] == 'n') &&
                (rest[2] == 'C' || rest[2] == 'c'))
            {
                // Device/Extended UNC - parse server\share
                i += segmentLength; // Include \\.\UNC\ or \\?\UNC\ or \??\UNC\

                rest = path.Slice(i);

                // Parse server (required, delimiter optional at end)
                if (!TryParseDelimitedSegment(rest, requireDelimiter: false, out int serverLength))
                {
                    // Malformed device/extended UNC path - we've committed to UNC format but server is missing
                    throw new ArgumentException($"Malformed device or extended-length UNC path: missing server name after UNC marker. Path: {path.ToString()}", nameof(path));
                }

                i += serverLength;

                // Parse share (optional - may not be present)
                if (i < path.Length)
                {
                    rest = path.Slice(i);
                    if (TryParseDelimitedSegment(rest, requireDelimiter: false, out int shareLength))
                    {
                        i += shareLength;
                    }
                }

                length = i;
                return true;
            }
        }

        // Otherwise, parse device name (everything until next separator or end)
        if (TryParseDelimitedSegment(rest, requireDelimiter: false, out int deviceLength))
        {
            i += deviceLength;
            length = i;
            return true;
        }

        // Malformed device/extended path - prefix detected but no valid content after
        throw new ArgumentException($"Malformed device or extended-length path: no device name or drive letter after prefix. Path: {path.ToString()}", nameof(path));
    }

    /// <summary>
    /// Parses a drive letter segment: C:\ (absolute) or C: (drive-relative).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Drive Letter Path Specification:</strong>
    /// </para>
    /// <para>
    /// Format: <c>[A-Za-z]:</c> followed by optional separator
    /// </para>
    /// <para>
    /// Two forms with very different semantics:
    /// <list type="bullet">
    /// <item><c>C:\path</c> - <strong>Absolute</strong>: Path from root of drive C</item>
    /// <item><c>C:path</c> - <strong>Drive-relative</strong>: Path relative to current directory on drive C</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Drive-Relative Danger:</strong>
    /// </para>
    /// <para>
    /// The form <c>C:path</c> is problematic because:
    /// <list type="bullet">
    /// <item>Its meaning depends on per-drive current directory (process-specific state)</item>
    /// <item>If process current dir on C: is <c>C:\Users\Alice</c>, then <c>C:file.txt</c> means <c>C:\Users\Alice\file.txt</c></item>
    /// <item>If process current dir on C: is <c>C:\Windows</c>, then <c>C:file.txt</c> means <c>C:\Windows\file.txt</c></item>
    /// <item>Most applications should avoid this form and use absolute paths instead</item>
    /// </list>
    /// </para>
    /// <para>
    /// This method handles both forms. The caller is responsible for checking whether
    /// a separator follows the colon to distinguish absolute from drive-relative.
    /// </para>
    /// </remarks>
    /// <param name="path">The path to parse.</param>
    /// <param name="length">Receives the length of the drive segment (2 or 3 chars: letter + colon + optional separator).</param>
    /// <returns>True if a valid drive letter segment was found; otherwise, false.</returns>
    private static bool TryParseDriveSegment(ReadOnlySpan<char> path, out int length)
    {
        length = 0;

        if (!IsDriveSegment(path))
        {
            return false;
        }

        int i = 2; // Drive letter + colon

        // Include separator if present (distinguishes C:\ from C:)
        if (i < path.Length && IsSeparator(path[i]))
        {
            i++;
        }

        length = i;
        return true;
    }

    /// <summary>
    /// Parses a regular UNC (Universal Naming Convention) path: \\server\share
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>UNC Path Root Specification:</strong>
    /// </para>
    /// <para>
    /// Format: <c>\\server\share</c> or <c>//server/share</c>
    /// </para>
    /// <para>
    /// Components:
    /// <list type="bullet">
    /// <item><strong>Server</strong>: Computer name, IP address, or hostname (REQUIRED, must be non-empty)</item>
    /// <item><strong>Share</strong>: Shared folder name on the server (OPTIONAL - incomplete paths allowed)</item>
    /// </list>
    /// </para>
    /// <para>
    /// The root of a UNC path includes BOTH the server and share because together they form
    /// a unique network location identifier. The share is like a "drive" on the remote server.
    /// </para>
    /// <para>
    /// <strong>CRITICAL SECURITY BOUNDARY:</strong>
    /// </para>
    /// <para>
    /// The share is a <strong>permission boundary</strong>. Different shares on the same server
    /// may have different access permissions. Path traversal with <c>..</c> must NEVER be allowed
    /// to escape above the share level:
    /// <list type="bullet">
    /// <item><c>\\server\share1\..</c> → INVALID (cannot go above share1)</item>
    /// <item><c>\\server\share1\..\share2</c> → INVALID (privilege escalation vulnerability!)</item>
    /// <item><c>\\server\share1\folder\..</c> → <c>\\server\share1\</c> (valid - stays within share)</item>
    /// </list>
    /// </para>
    /// <para>
    /// During path normalization, the root (server + share) must be treated as immutable.
    /// Allowing <c>..</c> to traverse above the share would enable unauthorized access to
    /// other shares that the user may not have permission to access.
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>\\server\share</c> - Complete UNC root (server + share, no trailing separator)</item>
    /// <item><c>\\server\share\</c> - Complete UNC root with trailing separator</item>
    /// <item><c>\\server</c> - Incomplete but valid (server only, no share)</item>
    /// <item><c>\\192.168.1.100\share</c> - UNC with IP address</item>
    /// <item><c>//server/share</c> - Cross-platform variant</item>
    /// </list>
    /// </para>
    /// <para>
    /// Validation:
    /// <list type="bullet">
    /// <item>Server name MUST be non-empty (\\\\share is invalid)</item>
    /// <item>Share name is optional (allows incomplete paths like \\server during typing)</item>
    /// <item>Both components may contain any characters except separators</item>
    /// </list>
    /// </para>
    /// <para>
    /// Security note: UNC paths can access remote network resources. Applications should
    /// validate that network access is intentional and authorized.
    /// </para>
    /// </remarks>
    /// <param name="path">The path to parse.</param>
    /// <param name="length">Receives the length of the UNC root (including server and share if present).</param>
    /// <returns>True if the path starts with a valid UNC prefix; otherwise, false.</returns>
    private static bool TryParseUncRootSegment(ReadOnlySpan<char> path, out int length)
    {
        length = 0;

        if (!IsUncRootSegment(path))
        {
            return false;
        }

        int i = 2; // Length of UNC prefix: \\

        // Slice to get the rest after \\
        ReadOnlySpan<char> rest = path.Slice(2);

        // Parse server (required, delimiter optional at end)
        if (!TryParseDelimitedSegment(rest, requireDelimiter: false, out int serverLength))
        {
            // Malformed UNC path - we've committed to UNC format (\\) but server is missing
            throw new ArgumentException($"Malformed UNC path: missing server name after UNC prefix. Path: {path.ToString()}", nameof(path));
        }

        i += serverLength;

        // Parse share (optional - may not be present)
        if (i < path.Length)
        {
            rest = path.Slice(i);
            if (TryParseDelimitedSegment(rest, requireDelimiter: false, out int shareLength))
            {
                i += shareLength;
            }
        }

        length = i;
        return true;
    }

    /// <summary>
    /// Parses a simple root segment: / or \
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Root-Only Path Specification:</strong>
    /// </para>
    /// <para>
    /// Format: Single separator character at the start of the path
    /// </para>
    /// <para>
    /// Platform-dependent semantics:
    /// <list type="bullet">
    /// <item><strong>Unix/Linux/macOS</strong>: <c>/path</c> is <strong>absolute</strong> - starts from filesystem root</item>
    /// <item><strong>Windows</strong>: <c>\path</c> is <strong>root-relative</strong> - starts from current drive's root</item>
    /// </list>
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    /// <item>Unix: <c>/usr/bin</c> always means the same location</item>
    /// <item>Windows: <c>\Windows</c> means <c>C:\Windows</c> if current drive is C:, or <c>D:\Windows</c> if current drive is D:</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Windows Root-Relative Danger:</strong>
    /// </para>
    /// <para>
    /// On Windows, <c>\path</c> is problematic because:
    /// <list type="bullet">
    /// <item>Its meaning depends on the current drive (process-specific state)</item>
    /// <item>Different processes may interpret the same path differently</item>
    /// <item>Most Windows applications should use absolute paths like <c>C:\path</c> instead</item>
    /// </list>
    /// </para>
    /// <para>
    /// This method doesn't distinguish between Unix absolute and Windows root-relative paths.
    /// That distinction requires OS context at runtime.
    /// </para>
    /// </remarks>
    /// <param name="path">The path to parse.</param>
    /// <param name="length">Receives 1 if the path starts with a separator; otherwise, 0.</param>
    /// <returns>True if the path starts with a single separator; otherwise, false.</returns>
    private static bool TryParseRootSegment(ReadOnlySpan<char> path, out int length)
    {
        if (IsRootSegment(path))
        {
            length = 1;
            return true;
        }

        length = 0;
        return false;
    }

    #endregion
}