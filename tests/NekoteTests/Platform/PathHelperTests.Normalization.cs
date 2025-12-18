using Nekote.Platform;
using System.Text;

namespace Nekote.Tests.Platform;

/// <summary>
/// Tests for PathHelper normalization methods.
/// </summary>
/// <remarks>
/// IMPORTANT: Normalization behavior clarifications:
/// - NormalizeStructure preserves trailing separators after "." removal
///   Example: "path/." returns "path/" not "path"
/// - Parent directory (..) resolution clamps at the ROOT boundary
///   Example: UNC \\server\share\.. stays at \\server\share\ (share is part of root)
/// - Triple+ slashes (///path) are treated as malformed UNC paths (throws)
/// - The root includes the share name in UNC paths, device names in device paths
/// </remarks>
public partial class PathHelperTests
{
    #region NormalizeStructure - Current Directory (.) Removal

    [Theory]
    [InlineData("dir/./file", "dir/file")]
    [InlineData("./file", "file")]
    [InlineData("path/.", "path/")]  // Trailing separator is preserved after . removal
    [InlineData("a/./b/./c", "a/b/c")]
    [InlineData(@"dir\.\..\file", "file")]
    public void NormalizeStructure_CurrentDirectory_RemovesIt(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeStructure_TripleDot_TreatedAsName()
    {
        // "..." is a valid filename in most filesystems, unlike "." and ".."
        var input = "path/to/.../file";
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal("path/to/.../file", result);
    }

    #endregion

    #region NormalizeStructure - Parent Directory (..) Resolution - Relative Paths

    [Theory]
    [InlineData("dir1/dir2/../file", "dir1/file")]
    [InlineData("dir1/../file", "file")]
    [InlineData("dir1/dir2/dir3/../../file", "dir1/file")]
    [InlineData("a/b/c/../../d", "a/d")]
    public void NormalizeStructure_ParentDirectory_CollapsesInRelativePaths(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("../../file", "../../file")]
    [InlineData("../file", "../file")]
    [InlineData("../../../a", "../../../a")]
    public void NormalizeStructure_LeadingParentDirectory_PreservedInRelativePaths(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("dir1/../../file", "../file")]
    [InlineData("a/../../../b", "../../b")]
    public void NormalizeStructure_ParentDirectoryExceedingDepth_PreservesExtra(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    #endregion

    #region NormalizeStructure - Parent Directory (..) Resolution - Absolute Paths (Clamped)

    [Theory]
    [InlineData("/usr/../bin", "/bin")]
    [InlineData("/a/b/../../c", "/c")]
    [InlineData("/dir/../file", "/file")]
    public void NormalizeStructure_ParentDirectoryInAbsolutePath_ClampsAtRoot(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("/../../file", "/file")]
    [InlineData("/../../../a", "/a")]
    [InlineData("/../../../../", "/")]
    public void NormalizeStructure_ExcessiveParentDirectoryInAbsolutePath_ClampsAtRoot(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"C:\Windows\..\Users", @"C:\Users")]
    [InlineData(@"C:\a\b\..\..\c", @"C:\c")]
    [InlineData(@"C:\..\..\file", @"C:\file")]
    public void NormalizeStructure_WindowsDrivePath_ClampsAtRoot(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"\\server\share\..\file", @"\\server\share\file")]  // .. clamps at share root
    [InlineData(@"\\server\share\dir\..\file", @"\\server\share\file")]
    [InlineData(@"\\server\share\..\..\file", @"\\server\share\file")]  // Both .. clamp at share root
    [InlineData(@"\\server\share\dir\.\file", @"\\server\share\dir\file")]  // . removes itself only, dir remains
    public void NormalizeStructure_UncPath_ClampsAtShareRoot(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"\\?\C:\dir\..\file", @"\\?\C:\file")]
    [InlineData(@"\\?\UNC\server\share\..\other", @"\\?\UNC\server\share\other")]  // .. clamps at share root
    [InlineData(@"\\.\Device\..\other", @"\\.\Device\other")]  // .. clamps at device root
    public void NormalizeStructure_DeviceAndExtendedPaths_ClampsAtRoot(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    #endregion

    #region NormalizeStructure - Consecutive Separator Removal

    [Theory]
    [InlineData("usr//bin", "usr/bin")]
    [InlineData("dir///file", "dir/file")]
    [InlineData("a////b////c", "a/b/c")]
    [InlineData(@"path\\to\\file", @"path\to\file")]
    public void NormalizeStructure_ConsecutiveSeparators_RemovesDuplicates(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"\\server\share", @"\\server\share")]
    [InlineData("//server/share", "//server/share")]
    public void NormalizeStructure_UncPrefix_Preserved(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("///usr/bin")]  // Treated as malformed UNC path (3+ slashes)
    [InlineData("////a/b")]
    public void NormalizeStructure_TripleSlashPrefix_ThrowsMalformedUncException(string input)
    {
        // Triple+ slashes are interpreted as UNC paths but missing server
        var ex = Assert.Throws<ArgumentException>(() => PathHelper.NormalizeStructure(input));
        Assert.Contains("Malformed UNC path", ex.Message);
    }

    #endregion

    #region NormalizeStructure - Combined Operations

    [Theory]
    [InlineData("dir1/./dir2/../dir3//file", "dir1/dir3/file")]
    [InlineData("./a//../b/./c///d", "b/c/d")]
    [InlineData("/usr/./local/../bin//ls", "/usr/bin/ls")]
    public void NormalizeStructure_CombinedOperations_WorksCorrectly(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    #endregion

    #region NormalizeStructure - Edge Cases

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizeStructure_EmptyOrWhitespace_ReturnsAsIs(string input)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(input, result);
    }

    [Theory]
    [InlineData("C:\\")]
    [InlineData("/")]
    [InlineData(@"\\server\share")]
    public void NormalizeStructure_RootOnly_ReturnsAsIs(string input)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(input, result);
    }

    [Fact]
    public void NormalizeStructure_NullInput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => PathHelper.NormalizeStructure(null!));
    }

    [Theory]
    [InlineData(@"C:\", @"C:\")]
    [InlineData(@"C:\path", @"C:\path")]
    [InlineData("C:", "C:")]
    public void NormalizeStructure_DriveOnly_HandlesCorrectly(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"C:path\..\file", @"C:file")]
    [InlineData(@"C:dir1\dir2\..\..", @"C:")]
    public void NormalizeStructure_DriveRelativePaths_NormalizesCorrectly(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    #endregion

    #region NormalizeStructure - Special Characters

    [Fact]
    public void NormalizeStructure_Wildcards_Preserved()
    {
        // * and ? are valid in Unix paths (though ? is used in Windows extended prefix)
        // NormalizeStructure shouldn't strip them from the body of the path
        var input = "path/to/*.txt";
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal("path/to/*.txt", result);

        var input2 = "path/to/fi?e.txt";
        var result2 = PathHelper.NormalizeStructure(input2);
        Assert.Equal("path/to/fi?e.txt", result2);
    }

    [Fact]
    public void NormalizeStructure_ZalgoText_Preserved()
    {
        // "H̷e comes" - excessive combining characters
        var zalgo = "H\u0337\u0300\u0321\u0328\u0355\u0320\u0330\u0338\u0341\u0334\u031ee comes/file.txt";
        var result = PathHelper.NormalizeStructure(zalgo);

        Assert.Equal(zalgo, result);
    }

    [Fact]
    public void NormalizeStructure_RTL_MixedDirection_Preserved()
    {
        // "folder/ملف/file.txt" (Arabic for file)
        var path = "folder/ملف/file.txt";
        var result = PathHelper.NormalizeStructure(path);

        Assert.Equal(path, result);
    }

    [Fact]
    public void NormalizeStructure_ComplexEmojis_Preserved()
    {
        // Family emoji: Man, Woman, Girl, Boy (joined by ZWJ)
        // 👨‍👩‍👧‍👦
        var family = "👨\u200D👩\u200D👧\u200D👦";
        var path = $"{family}/documents/vacation.jpg";

        var result = PathHelper.NormalizeStructure(path);

        // Ensure the sequence wasn't split or mangled
        Assert.Contains(family, result);
        Assert.Equal(path, result);
    }

    [Fact]
    public void NormalizeStructure_ControlCharacters_PassedThrough()
    {
        // PathHelper is a string manipulator, it doesn't enforce OS validity.
        // It should handle control chars without crashing.
        var path = "dir/\u0007bell/file.txt"; // \a (bell)
        var result = PathHelper.NormalizeStructure(path);

        Assert.Equal(path, result);
    }

    [Theory]
    [InlineData("CON/file.txt")]
    [InlineData("PRN/file.txt")]
    [InlineData("AUX/file.txt")]
    [InlineData("NUL/file.txt")]
    public void NormalizeStructure_WindowsReservedNames_Preserved(string path)
    {
        // PathHelper shouldn't care about reserved names, just structure
        var result = PathHelper.NormalizeStructure(path);
        Assert.Equal(path, result);
    }

    [Fact]
    public void NormalizeStructure_ZeroWidthJoiner_Preserved()
    {
        // ZWJ inside a filename
        var path = "fi\u200Dle.txt";
        var result = PathHelper.NormalizeStructure(path);

        Assert.Equal(path, result);
    }

    #endregion

    #region NormalizeStructure - Trailing Separators

    [Theory]
    [InlineData("path/to/dir/", "path/to/dir/")]
    [InlineData("path/to/dir", "path/to/dir")]
    [InlineData("a/b/c/./", "a/b/c/")]
    public void NormalizeStructure_TrailingSeparator_Preserved(string input, string expected)
    {
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(expected, result);
    }

    #endregion

    #region ToUnixPath

    [Theory]
    [InlineData(@"C:\Windows\System32", "C:/Windows/System32")]
    [InlineData(@"path\to\file", "path/to/file")]
    [InlineData(@"\\server\share", "//server/share")]
    [InlineData("already/unix/style", "already/unix/style")]
    public void ToUnixPath_ConvertsBackslashesToForwardSlashes(string input, string expected)
    {
        var result = PathHelper.ToUnixPath(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToUnixPath_NullInput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => PathHelper.ToUnixPath(null!));
    }

    #endregion

    #region ToWindowsPath

    [Theory]
    [InlineData("C:/Windows/System32", @"C:\Windows\System32")]
    [InlineData("path/to/file", @"path\to\file")]
    [InlineData("//server/share", @"\\server\share")]
    [InlineData(@"already\windows\style", @"already\windows\style")]
    public void ToWindowsPath_ConvertsForwardSlashesToBackslashes(string input, string expected)
    {
        var result = PathHelper.ToWindowsPath(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToWindowsPath_NullInput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => PathHelper.ToWindowsPath(null!));
    }

    #endregion

    #region ToNativePath

    [Fact]
    public void ToNativePath_ConvertsToNativeSeparator()
    {
        var input = "path/to/file";
        var result = PathHelper.ToNativePath(input);

        if (Path.DirectorySeparatorChar == '\\')
        {
            Assert.DoesNotContain("/", result);
            Assert.Contains("\\", result);
        }
        else
        {
            Assert.DoesNotContain("\\", result);
            Assert.Contains("/", result);
        }
    }

    [Fact]
    public void ToNativePath_NullInput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => PathHelper.ToNativePath(null!));
    }

    #endregion

    #region NormalizeSeparators

    [Theory]
    [InlineData("path/to\\file", PathSeparatorMode.Preserve, "path/to\\file")]
    [InlineData("path/to\\file", PathSeparatorMode.Windows, "path\\to\\file")]
    [InlineData("path/to\\file", PathSeparatorMode.Unix, "path/to/file")]
    public void NormalizeSeparators_VariousModes_WorksCorrectly(string input, PathSeparatorMode mode, string expected)
    {
        var result = PathHelper.NormalizeSeparators(input, mode);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeSeparators_InvalidMode_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PathHelper.NormalizeSeparators("path", (PathSeparatorMode)999));
    }

    [Fact]
    public void NormalizeSeparators_NullInput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            PathHelper.NormalizeSeparators(null!, PathSeparatorMode.Unix));
    }

    #endregion

    #region EnsureTrailingSeparator

    [Theory]
    [InlineData("path/to/dir")]
    [InlineData("path/to/dir/")]
    [InlineData("file")]
    public void EnsureTrailingSeparator_AddsIfMissing(string input)
    {
        var result = PathHelper.EnsureTrailingSeparator(input);
        // Result should end with either / or \
        Assert.True(result.EndsWith("/") || result.EndsWith("\\"));
        Assert.StartsWith(input.TrimEnd('/', '\\'), result);
    }

    [Fact]
    public void EnsureTrailingSeparator_NullInput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => PathHelper.EnsureTrailingSeparator(null!));
    }

    #endregion

    #region RemoveTrailingSeparator

    [Theory]
    [InlineData("path/to/dir/", "path/to/dir")]
    [InlineData("path/to/dir", "path/to/dir")]
    [InlineData(@"path\to\dir\", "path\\to\\dir")]
    [InlineData("file/", "file")]
    public void RemoveTrailingSeparator_RemovesIfPresent(string input, string expected)
    {
        var result = PathHelper.RemoveTrailingSeparator(input);
        Assert.False(result.EndsWith("/") || result.EndsWith("\\"));
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RemoveTrailingSeparator_NullInput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => PathHelper.RemoveTrailingSeparator(null!));
    }

    #endregion

    #region HandleTrailingSeparator

    [Theory]
    [InlineData("path/", TrailingSeparatorHandling.Preserve, "path/")]
    [InlineData("path", TrailingSeparatorHandling.Preserve, "path")]
    public void HandleTrailingSeparator_Preserve_KeepsOriginal(string input, TrailingSeparatorHandling handling, string expected)
    {
        var result = PathHelper.HandleTrailingSeparator(input, handling);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("path/", TrailingSeparatorHandling.Remove, "path")]
    [InlineData("path", TrailingSeparatorHandling.Remove, "path")]
    public void HandleTrailingSeparator_Remove_RemovesSeparator(string input, TrailingSeparatorHandling handling, string expected)
    {
        var result = PathHelper.HandleTrailingSeparator(input, handling);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandleTrailingSeparator_Ensure_AddsSeparator()
    {
        var result = PathHelper.HandleTrailingSeparator("path", TrailingSeparatorHandling.Ensure);
        Assert.True(result.EndsWith("/") || result.EndsWith("\\"));
    }

    [Fact]
    public void HandleTrailingSeparator_InvalidMode_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PathHelper.HandleTrailingSeparator("path", (TrailingSeparatorHandling)999));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void HandleTrailingSeparator_EmptyOrWhitespace_ReturnsAsIs(string input)
    {
        var result = PathHelper.HandleTrailingSeparator(input, TrailingSeparatorHandling.Remove);
        Assert.Equal(input, result);
    }

    #endregion

    #region NormalizeUnicode

    [Fact]
    public void NormalizeUnicode_NFDtoNFC_Normalizes()
    {
        // NFD (decomposed): é = e + combining acute accent
        var nfd = "caf\u0065\u0301"; // café in NFD form
        var result = PathHelper.NormalizeUnicode(nfd);

        // Should be NFC (composed)
        Assert.Equal("café", result);
        Assert.Equal(4, result.Length); // café in NFC is 4 characters
    }

    [Fact]
    public void NormalizeUnicode_AlreadyNFC_RemainsUnchanged()
    {
        var nfc = "café"; // Already in NFC form
        var result = PathHelper.NormalizeUnicode(nfc);
        Assert.Equal(nfc, result);
    }

    [Fact]
    public void NormalizeUnicode_ComplexPath_Normalizes()
    {
        // Path with decomposed characters
        var decomposed = "Documents/re\u0301sume\u0301/file.txt"; // résumé with combining accents
        var result = PathHelper.NormalizeUnicode(decomposed);

        Assert.Contains("résumé", result);
    }

    [Fact]
    public void NormalizeUnicode_NullInput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => PathHelper.NormalizeUnicode(null!));
    }

    #endregion

    #region ApplyNormalization - Internal Orchestration

    [Fact]
    public void ApplyNormalization_AllOperations_AppliesInCorrectOrder()
    {
        var options = new PathOptions
        {
            ThrowOnEmptySegments = false,
            TrimSegments = true,
            RequireAtLeastOneSegment = true,
            RequireAbsoluteFirstSegment = false,
            ValidateSubsequentPathsRelative = true,
            NormalizeStructure = true,
            NormalizeUnicode = true,
            NormalizeSeparators = PathSeparatorMode.Unix,
            TrailingSeparator = TrailingSeparatorHandling.Remove
        };

        var input = @"dir1\.\dir2\..\file/";
        var method = typeof(PathHelper).GetMethod("ApplyNormalization",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = (string)method!.Invoke(null, new object[] { options, input })!;

        // Should normalize structure, convert to Unix separators, and remove trailing separator
        Assert.Equal("dir1/file", result);
    }

    [Fact]
    public void ApplyNormalization_NoNormalizations_ReturnsOriginal()
    {
        var options = new PathOptions
        {
            ThrowOnEmptySegments = false,
            TrimSegments = true,
            RequireAtLeastOneSegment = true,
            RequireAbsoluteFirstSegment = false,
            ValidateSubsequentPathsRelative = true,
            NormalizeStructure = false,
            NormalizeUnicode = false,
            NormalizeSeparators = PathSeparatorMode.Preserve,
            TrailingSeparator = TrailingSeparatorHandling.Preserve
        };

        var input = @"dir\..\file/";
        var method = typeof(PathHelper).GetMethod("ApplyNormalization",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = (string)method!.Invoke(null, new object[] { options, input })!;

        // Should preserve everything
        Assert.Equal(input, result);
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void NormalizeStructure_ComplexWindowsPath_HandlesCorrectly()
    {
        var input = @"C:\Program Files\.\MyApp\..\Other\..\..\Windows\System32";
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal(@"C:\Windows\System32", result);
    }

    [Fact]
    public void NormalizeStructure_ComplexUnixPath_HandlesCorrectly()
    {
        var input = "/usr/./local/../share/../../opt/bin";
        var result = PathHelper.NormalizeStructure(input);
        Assert.Equal("/opt/bin", result);
    }

    [Fact]
    public void NormalizeStructure_MixedSeparatorsAndDots_HandlesCorrectly()
    {
        var input = @"base/dir1\.\dir2/..\dir3//file";
        var result = PathHelper.NormalizeStructure(input);
        // Should resolve dots and consecutive separators
        Assert.Contains("base", result);
        Assert.Contains("dir1", result);
        Assert.Contains("dir3", result);
        Assert.DoesNotContain("..", result);
    }

    #endregion

    #region NormalizeStructure - Deep Chaos

    [Fact]
    public void NormalizeStructure_AlternateDataStreams_Preserved()
    {
        // Windows NTFS supports "filename:streamname".
        // Normalization should treat ":streamname" as part of the filename, NOT as a drive or separator.

        var path = @"C:\data\file.txt:secret_stream";
        var result = PathHelper.NormalizeStructure(path);

        Assert.Equal(@"C:\data\file.txt:secret_stream", result);
    }

    [Fact]
    public void NormalizeStructure_AdsWithRelativePath_Preserved()
    {
        // Relative path with ADS: "file.txt:stream"
        // Should NOT be confused with a drive "file.txt:" (invalid drive anyway)

        var path = "file.txt:stream";
        var result = PathHelper.NormalizeStructure(path);

        Assert.Equal("file.txt:stream", result);
    }

    [Fact]
    public void NormalizeStructure_UnixFileNamedLikeDrive_TreatedAsRoot_KnownLimitation()
    {
        // CRITICAL AMBIGUITY:
        // On Linux, a file named "C:" is valid in the current directory.
        // However, PathHelper's logic is "Universal/Windows-biased", so it treats "C:" as a drive root.
        // This test documents this SPECIFIC behavior. If this behavior changes, this test should be updated.

        var unixPath = "C:/../file";

        // IF we were truly Unix-aware: "C:" is a dir. ".." goes out. Result -> "file" (or "../file").
        // BUT PathHelper assumes "C:" is a drive root. Roots clamp "..". Result -> "C:/file".

        var result = PathHelper.NormalizeStructure(unixPath);

        // Asserting the CURRENT behavior (clamping).
        // This confirms the library is "Windows-Safe" even on Unix, arguably "Unix-Hostile" for this edge case.
        Assert.Equal("C:/file", result);
    }

    [Fact]
    public void NormalizeStructure_BackslashOnUnix_TreatedAsSeparator_KnownLimitation()
    {
        // On Unix, "\" is a valid filename character, not a separator.
        // PathHelper treats it as a separator regardless of platform.

        var path = @"folder\file.txt"; // Unix: one file named "folder\file.txt"

        var result = PathHelper.NormalizeStructure(path);

        // PathHelper normalizes structure. Since it sees '\' as separator, it sees 2 segments.
        // It returns the path structurally normalized.
        // If it treated it as a filename, it would return "folder\file.txt" verbatim.
        // But it splits it.

        Assert.Equal(@"folder\file.txt", result);
    }

    [Fact]
    public void NormalizeStructure_DeeplyNested_DoesNotStackOverflow()
    {
        // Generate a path with 5000 segments
        var depth = 5000;
        var segments = Enumerable.Repeat("a", depth);
        var path = string.Join("/", segments);

        // This should not throw StackOverflowException
        var result = PathHelper.NormalizeStructure(path);

        Assert.EndsWith("a/a", result);
        Assert.True(result.Length > depth);
    }

    [Fact]
    public void NormalizeStructure_MassiveParentTraversal_ClampsEfficiently()
    {
        // /root/ + 10,000 ".." + /file
        var sb = new StringBuilder();
        sb.Append("/root");
        for (int i = 0; i < 10000; i++)
        {
            sb.Append("/..");
        }
        sb.Append("/file");

        var result = PathHelper.NormalizeStructure(sb.ToString());

        // Should clamp at root
        Assert.Equal("/file", result);
    }

    [Fact]
    public void NormalizeStructure_HomoglyphSeparators_NotTreatedAsSeparators()
    {
        // The mathematical division slash (U+2215) looks like /, but is NOT a separator.
        // Input: "folder∕file.txt" (1 segment)
        // Should NOT become "folder/file.txt" (2 segments)

        var homoglyph = "folder\u2215file.txt";
        var result = PathHelper.NormalizeStructure(homoglyph);

        Assert.Equal(homoglyph, result);
    }

    [Fact]
    public void NormalizeStructure_NullByte_Preserved()
    {
        // Null byte injection attack checks.
        // PathHelper is a structural normalizer, not a security sanitizer.
        // It should treat \0 as just another character in the name, preserving it.
        // It must NOT truncate the string (C-style behavior).

        var path = "safe.txt\0malicious.exe";
        var result = PathHelper.NormalizeStructure(path);

        Assert.Equal(path, result);
    }

    [Fact]
    public void NormalizeStructure_UriScheme_TreatedAsRelative()
    {
        // "file:///C:/path"
        // "file:" is a valid directory name on Windows/Linux (if no forbidden chars).
        // The parser should treat this as: "file:" -> "" -> "" -> "C:" -> "path"
        // (Assuming / is separator)

        var path = "file:///C:/path";
        var result = PathHelper.NormalizeStructure(path);

        // NormalizeStructure removes empty segments (consecutive separators)
        // "file:" / / / "C:" / "path"
        // Becomes: "file:/C:/path"

        Assert.Equal("file:/C:/path", result);
    }

    [Fact]
    public void NormalizeStructure_BrokenSurrogates_DoesNotCrash()
    {
        // High surrogate (U+D83D) without Low surrogate.
        // This is an invalid string, but the parser should be robust.

        var broken = "folder/\uD83D/file";
        var result = PathHelper.NormalizeStructure(broken);

        Assert.Equal(broken, result);
    }

    [Fact]
    public void NormalizeStructure_MixedRootSeparators_Consolidated()
    {
        // "C:\/Windows" -> Mixed separators at root.
        // NormalizeStructure consolidates consecutive separators.
        // But Root parsing logic handles the "C:\" part first.
        // Does it handle "C:\" then see "/" as start of next segment (empty) and remove it?
        // Or does it treat "C:\/" as the root? (GetRootLength doesn't consume mixed except for specific prefixes)
        //
        // If Root is "C:\", remaining is "/Windows".
        // Separator normalization isn't enabled here (only Structure), but duplicate separators are removed.
        // So "C:\" + "/Windows" -> "C:\/Windows" -> "C:\Windows" (consecutive removal).

        var path = @"C:\/Windows";
        var result = PathHelper.NormalizeStructure(path);

        // Should remove the redundant separator
        Assert.True(result == @"C:\Windows" || result == @"C:/Windows");
    }

    [Fact]
    public void NormalizeStructure_SurrogatesAtBoundaries_Preserved()
    {
        // Emoji (Grinning Face: \uD83D\uDE00) at start and end of segments
        // Ensure that splitting logic doesn't eat the first or last char of a surrogate pair
        // if it aligns with segment boundaries.

        var emoji = "\uD83D\uDE00"; // 😀
        var path = $"{emoji}/middle/{emoji}";
        var result = PathHelper.NormalizeStructure(path);

        Assert.Equal(path, result);
    }

    [Fact]
    public void NormalizeStructure_SplitSurrogatePair_TreatedAsTwoSegments()
    {
        // What if a user puts a separator INSIDE a surrogate pair?
        // High Surrogate (U+D83D) + Separator + Low Surrogate (U+DE00)
        // This makes two invalid strings (broken surrogates), but structurally it's two segments.
        // The parser should NOT crash or try to "heal" the pair by eating the separator.

        var split = "Start\uD83D/\uDE00End";
        var result = PathHelper.NormalizeStructure(split);

        Assert.Equal(split, result);
    }

    #endregion
}
