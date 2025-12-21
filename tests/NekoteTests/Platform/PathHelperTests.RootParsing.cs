using Nekote.Platform;

namespace Nekote.Tests.Platform;

/// <summary>
/// Tests for PathHelper root parsing methods (GetRootLength and related).
/// </summary>
/// <remarks>
/// IMPORTANT: Root length behavior clarification:
/// - Device paths like \\.\ include the FULL device name in root, not just the 4-char prefix
///   Example: \\.\COM1 returns 8 (4 for prefix + 4 for "COM1")
/// - UNC paths include the trailing separator if present
///   Example: \\server\share\ returns 15 (includes trailing \)
/// - Extended/Device UNC paths also include full server\share
///   Example: \\.\UNC\server\share returns 20 (prefix + UNC + server\ + share)
/// </remarks>
public partial class PathHelperTests
{
    #region GetRootLength - Device Paths (\\.\)

    [Theory]
    [InlineData(@"\\.\COM1", 8)]  // 4 (prefix) + 4 (COM1)
    [InlineData(@"//./COM1", 8)]  // 4 (prefix) + 4 (COM1)
    [InlineData(@"\\.\PhysicalDisk0", 17)]  // 4 (prefix) + 13 (PhysicalDisk0)
    [InlineData(@"\\.\device", 10)]  // 4 (prefix) + 6 (device)
    [InlineData(@"\\.\DeviceName\path", 15)]  // 4 (prefix) + 10 (DeviceName) + 1 (separator)
    public void GetRootLength_DevicePath_ReturnsCorrectLength(string path, int expectedLength)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.Equal(expectedLength, length);
    }

    [Theory]
    [InlineData(@"\\.\C:\", 7)]
    [InlineData(@"//./C:/", 7)]
    [InlineData(@"\\.\D:\path\to\file", 7)]
    [InlineData(@"\\.\E:", 6)]
    public void GetRootLength_DevicePathWithDrive_ReturnsCorrectLength(string path, int expectedLength)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.Equal(expectedLength, length);
    }

    [Theory]
    [InlineData(@"\\.\UNC\server\share", 20)]  // 4 + 3 + 1 + 6 + 1 + 5 = prefix + UNC\ + server\ + share
    [InlineData(@"//./UNC/server/share", 20)]  // Same with forward slashes
    [InlineData(@"\\.\UNC\server\share\", 21)]  // Includes trailing separator
    [InlineData(@"\\.\UNC\192.168.1.100\files", 27)]  // 4 + 4 + 15 + 1 + 5 = \\.\ + UNC\ + 192.168.1.100\ + files
    public void GetRootLength_DeviceUncPath_ReturnsCorrectLength(string path, int expectedLength)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.Equal(expectedLength, length);
    }

    [Theory]
    [InlineData(@"\\.\UNC\server")]
    [InlineData(@"//./UNC/server")]
    public void GetRootLength_DeviceUncPathWithoutShare_ReturnsServerLength(string path)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        // Should include: \\.\UNC\ (8 chars) + "server" (6 chars) = 14
        Assert.Equal(14, length);
    }

    [Theory]
    [InlineData(@"\\.", 3)]  // Recognized as root-relative path (\\+.)
    [InlineData(@"//.", 3)]  // Same with forward slashes
    public void GetRootLength_ShortDeviceLikePath_ReturnsRootRelativeLength(string path, int expectedLength)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.Equal(expectedLength, length);
    }

    [Theory]
    [InlineData(@"\\.\UNC", 7)]  // 4 (prefix) + 3 (UNC) - no exception thrown
    [InlineData(@"//./UNC", 7)]
    public void GetRootLength_DeviceUncWithoutServer_ReturnsLength(string path, int expectedLength)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.Equal(expectedLength, length);
    }

    #endregion

    #region GetRootLength - Extended-Length Paths (\\?\)

    [Theory]
    [InlineData(@"\\?\C:\very\long\path", 7)]
    [InlineData(@"//?/C:/very/long/path", 7)]
    [InlineData(@"\\?\D:\", 7)]
    [InlineData(@"\\?\E:", 6)]
    public void GetRootLength_ExtendedLengthPathWithDrive_ReturnsCorrectLength(string path, int expectedLength)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.Equal(expectedLength, length);
    }

    [Theory]
    [InlineData(@"\\?\UNC\server\share", 20)]  // 4 + 4 + 7 + 5 = \\?\ + UNC\ + server\ + share
    [InlineData(@"//?/UNC/server/share", 20)]
    [InlineData(@"\\?\UNC\server\share\path", 21)]  // Includes trailing separator after share
    [InlineData(@"\\?\UNC\myserver\docs\", 22)]  // 4 + 4 + 9 + 5 = includes trailing separator
    public void GetRootLength_ExtendedUncPath_ReturnsCorrectLength(string path, int expectedLength)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.Equal(expectedLength, length);
    }

    [Theory]
    [InlineData(@"\??\C:\path", 7)]
    [InlineData(@"/??\C:/path", 7)]
    [InlineData(@"\??\UNC\server\share", 20)]  // Same as \\?\ - includes full server\share
    [InlineData(@"/??/UNC/server/share", 20)]
    public void GetRootLength_NtNativePathPrefix_ReturnsCorrectLength(string path, int expectedLength)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.Equal(expectedLength, length);
    }

    [Theory]
    [InlineData(@"\\?", 3)]  // Recognized as root-relative path (\\+?)
    [InlineData(@"//?", 3)]  // Same with forward slashes
    [InlineData(@"\??", 1)]  // Recognized as simple root-relative path (\)
    public void GetRootLength_ShortExtendedLikePath_ReturnsRootRelativeLength(string path, int expectedLength)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.Equal(expectedLength, length);
    }

    #endregion

    #region GetRootLength - UNC Paths (\\server\share)

    [Theory]
    [InlineData(@"\\server\share", 14)]
    [InlineData(@"//server/share", 14)]
    [InlineData(@"\\server\share\", 15)]
    [InlineData(@"\\server\share\path\to\file", 15)]
    [InlineData(@"\\192.168.1.100\backup", 22)]  // 2 + 15 + 1 + 6 = \\ + IP + \ + backup (share includes separator)
    [InlineData(@"\\myserver\docs", 15)]
    public void GetRootLength_StandardUncPath_ReturnsCorrectLength(string path, int expectedLength)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.Equal(expectedLength, length);
    }

    [Theory]
    [InlineData(@"\\server")]
    [InlineData(@"//server")]
    public void GetRootLength_UncPathWithoutShare_ReturnsServerLength(string path)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.Equal(8, length);
    }

    [Theory]
    [InlineData(@"\\server", 8)] // prefix(2) + server(6)
    [InlineData(@"\\server\", 9)]
    [InlineData(@"\\?\UNC\server", 14)] // prefix(4) + UNC\(4) + server(6)
    public void GetRootLength_IncompleteUnc_IncludesServer(string path, int expectedLength)
    {
        // Even without a share name, the server part is considered the root
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.Equal(expectedLength, length);
    }

    [Fact]
    public void GetRootLength_IPv6_UncPath_HandledCorrectly()
    {
        // IPv6 addresses contain colons, which can be confused with drive letters.
        // Format: \\2001:db8::1\share
        // The parser must treat "2001:db8::1" as the server, not confusing ":d" as a drive.

        var ipv6Unc = @"\\2001:db8::1\share\folder";
        var rootLength = PathHelper.GetRootLength(ipv6Unc, PathOptions.Windows, out _);

        // Root should be \\2001:db8::1\share\ (including trailing separator)
        // Length: 2 (\\) + 11 (2001:db8::1) + 1 (\) + 5 (share) + 1 (\) = 20
        var rootPart = ipv6Unc.Substring(0, rootLength);

        Assert.StartsWith(@"\\2001:db8::1\share", rootPart);
        Assert.Equal(20, rootLength);
    }

    [Theory]
    [InlineData(@"\\")]
    [InlineData(@"//")]
    public void GetRootLength_MalformedUncPath_Throws(string path)
    {
        var ex = Assert.Throws<ArgumentException>(() => PathHelper.GetRootLength(path, PathOptions.Windows, out _));
        Assert.Contains("Malformed UNC path", ex.Message);
    }

    #endregion

    #region GetRootLength - Drive Letter Paths

    [Theory]
    [InlineData(@"C:\")]
    [InlineData(@"C:/")]
    [InlineData(@"D:\path\to\file")]
    [InlineData(@"Z:\")]
    [InlineData(@"A:\directory")]
    public void GetRootLength_DriveLetterAbsolute_ReturnsThree(string path)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.Equal(3, length);
    }

    [Theory]
    [InlineData(@"C:")]
    [InlineData(@"D:relative\path")]
    [InlineData(@"Z:file.txt")]
    public void GetRootLength_DriveLetterRelative_ReturnsTwo(string path)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.Equal(2, length);
    }

    [Theory]
    [InlineData("c:\\")]
    [InlineData("d:/")]
    [InlineData("e:")]
    [InlineData("z:\\path")]
    public void GetRootLength_DriveLetterLowercase_ReturnsCorrectLength(string path)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        var expected = path.Length >= 3 && (path[2] == '\\' || path[2] == '/') ? 3 : 2;
        Assert.Equal(expected, length);
    }

    #endregion

    #region GetRootLength - Root-Relative Paths

    [Theory]
    [InlineData(@"\")]
    [InlineData(@"/")]
    [InlineData(@"\Windows")]
    [InlineData(@"/usr/bin")]
    [InlineData(@"\path\to\file")]
    public void GetRootLength_RootRelativePath_ReturnsOne(string path)
    {
        // Root-relative paths (\path or /path) are cross-platform
        var length = PathHelper.GetRootLength(path, PathOptions.Default, out _);
        Assert.Equal(1, length);
    }

    #endregion

    #region GetRootLength - Relative Paths

    [Theory]
    [InlineData("relative")]
    [InlineData("relative\\path")]
    [InlineData("relative/path")]
    [InlineData("..\\parent")]
    [InlineData("../parent")]
    [InlineData(".\\current")]
    [InlineData("./current")]
    [InlineData("file.txt")]
    public void GetRootLength_RelativePath_ReturnsZero(string path)
    {
        // Relative paths are cross-platform
        var length = PathHelper.GetRootLength(path, PathOptions.Default, out _);
        Assert.Equal(0, length);
    }

    #endregion

    #region GetRootLength - Edge Cases

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void GetRootLength_EmptyOrNullPath_ReturnsZero(string? path)
    {
        var length = PathHelper.GetRootLength(path ?? string.Empty, PathOptions.Default, out _);
        Assert.Equal(0, length);
    }

    [Theory]
    [InlineData(@"\\?\UNC\server\share\folder1\folder2\file.txt")]
    [InlineData(@"\\.\C:\Program Files\MyApp\data.bin")]
    public void GetRootLength_VeryLongPath_ReturnsCorrectRootLength(string path)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.True(length > 0);
        Assert.True(length < path.Length);
    }

    [Theory]
    [InlineData(@"\\?\UNC\192.168.1.100\share$\path")]
    [InlineData(@"\\.\C:\Users\My-Name\Documents")]
    [InlineData(@"\\server-01\share_name\folder")]
    public void GetRootLength_SpecialCharactersInPath_ReturnsCorrectLength(string path)
    {
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.True(length > 0);
    }

    #endregion

    #region GetRootLength - Mixed Separators

    [Theory]
    [InlineData(@"\\server\share")]
    [InlineData(@"//server/share")]
    [InlineData(@"C:/path\to\file")]
    [InlineData(@"C:\path/to\file")]
    public void GetRootLength_MixedSeparators_ReturnsCorrectLength(string path)
    {
        // Windows-specific paths with mixed separators
        var length = PathHelper.GetRootLength(path, PathOptions.Windows, out _);
        Assert.True(length > 0);
    }

    #endregion
}
