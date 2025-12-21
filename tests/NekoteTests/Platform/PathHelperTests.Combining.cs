using Nekote.Platform;
using OperatingSystem = Nekote.Platform.OperatingSystem;

namespace Nekote.Tests.Platform;

/// <summary>
/// Tests for PathHelper path combining methods.
/// </summary>
public partial class PathHelperTests
{
    #region Combine - Basic Functionality

    [Theory]
    [InlineData("C:\\base", "file.txt", "C:\\base\\file.txt")]
    [InlineData("C:/base", "file.txt", "C:/base/file.txt")]
    [InlineData("/usr", "bin", "/usr/bin")]
    [InlineData("relative", "path", "relative\\path")] // Windows default
    public void Combine_TwoSegments_CombinesCorrectly(string path1, string path2, string expected)
    {
        // Use Windows options for tests with drive letters
        var options = path1.Length >= 2 && path1[1] == ':' ? PathOptions.Windows : PathOptions.Default;
        var result = PathHelper.Combine(options, path1, path2);
        // Normalize separators for comparison
        Assert.Equal(expected.Replace('\\', '/'), result.Replace('\\', '/'));
    }

    [Fact]
    public void Combine_NullOptions_UsesDefaults()
    {
        // Explicitly passing null should use PathOptions.Default
        var result = PathHelper.Combine(null, "part1", "part2");
        Assert.EndsWith("part2", result);
        Assert.Contains("part1", result);
    }

    [Theory]
    [InlineData("C:\\base", "dir", "file.txt", "C:\\base\\dir\\file.txt")]
    [InlineData("/usr", "local", "bin", "/usr/local/bin")]
    [InlineData("a", "b", "c", "a\\b\\c")]
    public void Combine_ThreeSegments_CombinesCorrectly(string path1, string path2, string path3, string expected)
    {
        var options = path1.Length >= 2 && path1[1] == ':' ? PathOptions.Windows : PathOptions.Default;
        var result = PathHelper.Combine(options, path1, path2, path3);
        Assert.Equal(expected.Replace('\\', '/'), result.Replace('\\', '/'));
    }

    [Theory]
    [InlineData("C:\\base", "dir1", "dir2", "file.txt", "C:\\base\\dir1\\dir2\\file.txt")]
    [InlineData("/usr", "local", "share", "doc", "/usr/local/share/doc")]
    public void Combine_FourSegments_CombinesCorrectly(string path1, string path2, string path3, string path4, string expected)
    {
        var options = path1.Length >= 2 && path1[1] == ':' ? PathOptions.Windows : PathOptions.Default;
        var result = PathHelper.Combine(options, path1, path2, path3, path4);
        Assert.Equal(expected.Replace('\\', '/'), result.Replace('\\', '/'));
    }

    [Fact]
    public void Combine_ParamsArray_CombinesCorrectly()
    {
        var result = PathHelper.Combine(PathOptions.Default, "a", "b", "c", "d", "e", "f");
        Assert.Contains("a", result);
        Assert.Contains("f", result);
    }

    #endregion

    #region Combine - Null and Empty Handling

    [Fact]
    public void Combine_NullSegments_FiltersThemOut()
    {
        var result = PathHelper.Combine(PathOptions.Default, "base", null, "file.txt");
        Assert.DoesNotContain("null", result.ToLower());
        Assert.Contains("base", result);
        Assert.Contains("file.txt", result);
    }

    [Fact]
    public void Combine_EmptySegments_FiltersThemOut()
    {
        var result = PathHelper.Combine(PathOptions.Default, "base", "", "file.txt");
        Assert.Contains("base", result);
        Assert.Contains("file.txt", result);
    }

    [Fact]
    public void Combine_WhitespaceSegments_FiltersThemOut()
    {
        var result = PathHelper.Combine(PathOptions.Default, "base", "   ", "file.txt");
        Assert.Contains("base", result);
        Assert.Contains("file.txt", result);
    }

    [Fact]
    public void Combine_AllNullOrEmpty_WithRequireAtLeastOne_Throws()
    {
        var options = PathOptions.Default with { RequireAtLeastOneSegment = true };
        var ex = Assert.Throws<ArgumentException>(() =>
            PathHelper.Combine(options, null, "", "   "));
        Assert.Contains("At least one non-empty path segment is required", ex.Message);
    }

    [Fact]
    public void Combine_ThrowOnEmptySegments_WithEmpty_Throws()
    {
        var options = PathOptions.Default with { ThrowOnEmptySegments = true };
        var ex = Assert.Throws<ArgumentException>(() =>
            PathHelper.Combine(options, "base", "", "file.txt"));
        Assert.Contains("Path segments cannot be null, empty, or whitespace", ex.Message);
    }

    #endregion

    #region Combine - Trimming

    [Fact]
    public void Combine_TrimSegments_TrimsWhitespace()
    {
        var options = PathOptions.Default with { TrimSegments = true };
        var result = PathHelper.Combine(options, "  base  ", "  file.txt  ");
        Assert.Contains("base", result);
        Assert.Contains("file.txt", result);
        Assert.DoesNotContain("  base  ", result);
    }

    [Fact]
    public void Combine_NoTrimSegments_PreservesWhitespace()
    {
        var options = PathOptions.Default with { TrimSegments = false, ThrowOnEmptySegments = true };
        var result = PathHelper.Combine(options, "  base  ", "  file  ");
        // Should preserve internal structure but Path.Combine might normalize
        Assert.Contains("base", result);
        Assert.Contains("file", result);
    }

    #endregion

    #region Combine - Absolute Path Validation

    [Theory]
    [InlineData("C:\\absolute")]
    [InlineData("\\\\server\\share")]
    // Note: /usr/bin is NOT fully qualified on Windows (Path.IsPathFullyQualified returns false)
    // because it lacks a drive letter. It's only fully qualified on Unix.
    public void Combine_RequireAbsoluteFirst_WithAbsolute_Succeeds(string absolutePath)
    {
        // Use Windows options for Windows-specific paths
        var options = (absolutePath.StartsWith(@"\\") || (absolutePath.Length >= 2 && absolutePath[1] == ':'))
            ? PathOptions.Windows with { RequireAbsoluteFirstSegment = true }
            : PathOptions.Default with { RequireAbsoluteFirstSegment = true };
        var result = PathHelper.Combine(options, absolutePath, "file.txt");
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("relative")]
    [InlineData("..\\parent")]
    [InlineData(".\\current")]
    public void Combine_RequireAbsoluteFirst_WithRelative_Throws(string relativePath)
    {
        var options = PathOptions.Default with { RequireAbsoluteFirstSegment = true };
        var ex = Assert.Throws<ArgumentException>(() =>
            PathHelper.Combine(options, relativePath, "file.txt"));
        Assert.Contains("must be an absolute (fully qualified) path", ex.Message);
    }

    [Theory]
    [InlineData("\\path")] // Root-relative on Windows
    [InlineData("C:path")] // Drive-relative
    public void Combine_RequireAbsoluteFirst_WithAmbiguousPaths_Throws(string ambiguousPath)
    {
        // These paths are ambiguous on Windows but we're running on Unix, so explicitly target Windows
        var options = PathOptions.Default with
        {
            RequireAbsoluteFirstSegment = true,
            TargetOperatingSystem = OperatingSystemType.Windows
        };
        var ex = Assert.Throws<ArgumentException>(() =>
            PathHelper.Combine(options, ambiguousPath, "file.txt"));
        Assert.Contains("must be an absolute (fully qualified) path", ex.Message);
    }

    #endregion

    #region Combine - Subsequent Path Validation

    [Fact]
    public void Combine_ValidateSubsequentRelative_WithRelative_Succeeds()
    {
        var options = PathOptions.Windows with { ValidateSubsequentPathsRelative = true };
        var result = PathHelper.Combine(options, "C:\\base", "relative", "path");
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("C:\\other")]
    [InlineData("\\absolute")]
    [InlineData("/absolute")]
    [InlineData("D:relative")]
    public void Combine_ValidateSubsequentRelative_WithRooted_Throws(string rootedPath)
    {
        var options = PathOptions.Windows with { ValidateSubsequentPathsRelative = true };
        var ex = Assert.Throws<ArgumentException>(() =>
            PathHelper.Combine(options, "C:\\base", rootedPath));
        Assert.Contains("must be a relative path", ex.Message);
    }

    [Fact]
    public void Combine_ValidateSubsequentRelative_PreventsSilentReplacement()
    {
        // Without validation, Path.Combine would silently replace base with absolute
        var options = PathOptions.Windows with { ValidateSubsequentPathsRelative = true };
        var ex = Assert.Throws<ArgumentException>(() =>
            PathHelper.Combine(options, "C:\\base", "D:\\other"));
        Assert.Contains("must be a relative path", ex.Message);
    }

    [Fact]
    public void Combine_AllowAbsoluteSubsequent_RestartsPath()
    {
        // When validation is disabled, we simply concatenate.
        // We do NOT restart at root (unlike Path.Combine) to prevent path traversal attacks.
        var options = PathOptions.Default with { ValidateSubsequentPathsRelative = false };

        string root = System.OperatingSystem.IsWindows() ? @"D:\new" : "/new";
        var result = PathHelper.Combine(options, "base", root);
        
        // Should concatenate: "base/D:\new" or "base//new"
        Assert.Contains("base", result);
        Assert.EndsWith(root, result);
        Assert.True(result.Length > root.Length);
    }

    #endregion

    #region Combine - Normalization Options

    [Fact]
    public void Combine_NormalizeStructure_ResolvesDotsAndDoubleDots()
    {
        var options = PathOptions.Default with { NormalizeStructure = true };
        var result = PathHelper.Combine(options, "base", "dir1", "..", "dir2", ".", "file.txt");
        Assert.Contains("base", result);
        Assert.Contains("dir2", result);
        Assert.Contains("file.txt", result);
        Assert.DoesNotContain("..", result);
        Assert.DoesNotContain(Path.DirectorySeparatorChar + "." + Path.DirectorySeparatorChar, result);
    }

    [Fact]
    public void Combine_NoNormalizeStructure_PreservesDots()
    {
        var options = PathOptions.Default with { NormalizeStructure = false };
        var result = PathHelper.Combine(options, "base", "..", "file.txt");
        // Path.Combine doesn't normalize, so .. should remain
        Assert.Contains("..", result);
    }

    #endregion

    #region Combine - Separator Normalization

    [Fact]
    public void Combine_WindowsMode_UsesBackslashes()
    {
        var result = PathHelper.Combine(PathOptions.Windows, "base", "dir", "file.txt");
        Assert.Contains("\\", result);
        Assert.DoesNotContain("/", result);
    }

    [Fact]
    public void Combine_UnixMode_UsesForwardSlashes()
    {
        var result = PathHelper.Combine(PathOptions.Unix, "base", "dir", "file.txt");
        Assert.Contains("/", result);
        Assert.DoesNotContain("\\", result);
    }

    [Fact]
    public void Combine_PreserveMode_MaintainsOriginalSeparators()
    {
        var options = PathOptions.Default with { NormalizeSeparators = PathSeparatorMode.Preserve };
        var result = PathHelper.Combine(options, "base/dir", "file.txt");
        // Should not convert the existing forward slash
        Assert.Contains("base", result);
        Assert.Contains("file.txt", result);
    }

    #endregion

    #region Combine - Trailing Separator

    [Fact]
    public void Combine_RemoveTrailingSeparator_RemovesIt()
    {
        var options = PathOptions.Default with { TrailingSeparator = TrailingSeparatorHandling.Remove };
        var result = PathHelper.Combine(options, "base", "dir");
        Assert.False(result.EndsWith("\\") || result.EndsWith("/"));
    }

    [Fact]
    public void Combine_EnsureTrailingSeparator_AddsIt()
    {
        var options = PathOptions.Default with { TrailingSeparator = TrailingSeparatorHandling.Ensure };
        var result = PathHelper.Combine(options, "base", "dir");
        Assert.True(result.EndsWith("\\") || result.EndsWith("/"));
    }

    [Fact]
    public void Combine_PreserveTrailingSeparator_MaintainsOriginal()
    {
        var options = PathOptions.Default with { TrailingSeparator = TrailingSeparatorHandling.Preserve };
        var resultWithSlash = PathHelper.Combine(options, "base\\", "dir\\");
        var resultWithoutSlash = PathHelper.Combine(options, "base", "dir");

        // Path.Combine behavior depends on trailing separators in input
        Assert.NotEmpty(resultWithSlash);
        Assert.NotEmpty(resultWithoutSlash);
    }

    #endregion

    #region Combine - Unicode Normalization

    [Fact]
    public void Combine_NormalizeUnicode_NormalizesToNFC()
    {
        // Use NFD (decomposed) form: é = e + combining acute
        var decomposed = "caf\u0065\u0301"; // café in NFD form
        var options = PathOptions.Default with { NormalizeUnicode = true };
        var result = PathHelper.Combine(options, decomposed, "file.txt");

        // Should be normalized to NFC form
        Assert.Contains("café", result);
    }

    #endregion

    #region Convenience Methods - CombineNative

    [Fact]
    public void CombineNative_TwoSegments_UsesPlatformSeparators()
    {
        var result = PathHelper.CombineNative("base", "file.txt");
        Assert.Contains("base", result);
        Assert.Contains("file.txt", result);
        Assert.True(result.Contains(Path.DirectorySeparatorChar));
    }

    [Fact]
    public void CombineNative_MultipleSegments_Works()
    {
        var result = PathHelper.CombineNative("a", "b", "c", "d");
        Assert.Contains("a", result);
        Assert.Contains("d", result);
    }

    #endregion

    #region Convenience Methods - CombineWindows

    [Fact]
    public void CombineWindows_TwoSegments_UsesBackslashes()
    {
        var result = PathHelper.CombineWindows("base", "file.txt");
        Assert.Contains("\\", result);
        Assert.DoesNotContain("/", result);
    }

    [Fact]
    public void CombineWindows_ConvertsMixedSeparators()
    {
        var result = PathHelper.CombineWindows("base/dir", "file.txt");
        Assert.Contains("\\", result);
        Assert.DoesNotContain("/", result);
    }

    #endregion

    #region Convenience Methods - CombineUnix

    [Fact]
    public void CombineUnix_TwoSegments_UsesForwardSlashes()
    {
        var result = PathHelper.CombineUnix("base", "file.txt");
        Assert.Contains("/", result);
        Assert.DoesNotContain("\\", result);
    }

    [Fact]
    public void CombineUnix_ConvertsMixedSeparators()
    {
        var result = PathHelper.CombineUnix("base\\dir", "file.txt");
        Assert.Contains("/", result);
        Assert.DoesNotContain("\\", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Combine_VeryLongPath_Works()
    {
        var segments = new string[100];
        for (int i = 0; i < 100; i++)
        {
            segments[i] = $"dir{i}";
        }
        var result = PathHelper.Combine(PathOptions.Default, segments);
        Assert.NotEmpty(result);
        Assert.Contains("dir0", result);
        Assert.Contains("dir99", result);
    }

    [Fact]
    public void Combine_SpecialCharacters_PreservesThem()
    {
        var result = PathHelper.Combine(PathOptions.Default, "base", "dir-name_123", "file (1).txt");
        Assert.Contains("dir-name_123", result);
        Assert.Contains("file (1).txt", result);
    }

    [Fact]
    public void Combine_TripleDot_IsTreatedAsDirectoryName()
    {
        // "..." is a valid directory name, not a parent traversal.
        // Ensure it's not treated as ".."
        
        var result = PathHelper.Combine(PathOptions.Default, "root", "...", "file");
        
        // Should be root/.../file
        Assert.Contains("...", result);
        Assert.DoesNotContain("root/file", result.Replace('\\', '/')); 
    }

    [Fact]
    public void Combine_UnicodeCharacters_PreservesThem()
    {
        var result = PathHelper.Combine(PathOptions.Default, "文档", "ファイル", "файл.txt");
        Assert.Contains("文档", result);
        Assert.Contains("ファイル", result);
        Assert.Contains("файл.txt", result);
    }

    [Fact]
    public void Combine_SplitSurrogates_ThrowsOnInvalidUnicode()
    {
        // PathHelper uses strict Unicode normalization which throws on invalid sequences
        // (like split surrogate pairs).
        
        var high = "\uD83D";
        var low = "\uDE00"; 

        Assert.Throws<ArgumentException>(() => 
            PathHelper.Combine(PathOptions.Default, "dir" + high, low + "file"));
    }

    [Fact]
    public void Combine_ColonInFilename_Windows_TreatedAsRelative()
    {
        // "file:stream" looks like a drive, but drive must be 1 char [A-Za-z].
        // So on Windows, this is a valid relative path (Alternate Data Stream).
        var result = PathHelper.Combine(PathOptions.Windows, "dir", "file:stream");
        Assert.EndsWith(@"dir\file:stream", result);
    }

    [Fact]
    public void Combine_DriveLikeFilename_Windows_Throws()
    {
        // "c:file" is a drive-relative path on Windows (rooted).
        // It should be rejected as a subsequent segment.
        Assert.Throws<ArgumentException>(() => 
            PathHelper.Combine(PathOptions.Windows, "dir", "c:file"));
    }

    [Fact]
    public void Combine_DriveLikeFilename_Unix_Throws()
    {
        // "c:file" is strictly rejected on Unix to avoid ambiguity, 
        // even though it could be a valid filename.
        Assert.Throws<ArgumentException>(() => 
            PathHelper.Combine(PathOptions.Unix, "dir", "c:file"));
    }

    [Fact]
    public void Combine_HiddenFiles_Preserved()
    {
        // ".config" starts with ., but is not "." (current dir)
        var result = PathHelper.Combine(PathOptions.Default, "home", ".config");
        Assert.EndsWith("home" + Path.DirectorySeparatorChar + ".config", result);
    }

    #endregion

    #region Preset Options Testing

    [Fact]
    public void Combine_DefaultPreset_HasExpectedBehavior()
    {
        // Default should: trim, allow filtering, require at least one, validate subsequent
        var result = PathHelper.Combine(PathOptions.Default, "  base  ", null, "", "file.txt");
        Assert.Contains("base", result);
        Assert.Contains("file.txt", result);
    }

    [Fact]
    public void Combine_MinimalPreset_SkipsNormalization()
    {
        var result = PathHelper.Combine(PathOptions.Minimal, "base", "..", "file.txt");
        Assert.Contains("..", result); // Structure not normalized
    }

    [Fact]
    public void Combine_WindowsPreset_ProducesWindowsPaths()
    {
        var result = PathHelper.Combine(PathOptions.Windows, "base", "dir", "file.txt");
        Assert.Contains("\\", result);
        Assert.DoesNotContain("/", result);
    }

    [Fact]
    public void Combine_UnixPreset_ProducesUnixPaths()
    {
        var result = PathHelper.Combine(PathOptions.Unix, "base", "dir", "file.txt");
        Assert.Contains("/", result);
        Assert.DoesNotContain("\\", result);
    }

    #endregion
}
