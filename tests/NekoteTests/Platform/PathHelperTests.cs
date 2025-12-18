using Nekote.Platform;

namespace Nekote.Tests.Platform;

/// <summary>
/// Tests for PathHelper utility methods (non-partial main file).
/// </summary>
public partial class PathHelperTests
{
    #region IsValidDriveChar

    [Theory]
    [InlineData('A', true)]
    [InlineData('Z', true)]
    [InlineData('a', true)]
    [InlineData('z', true)]
    [InlineData('C', true)]
    [InlineData('D', true)]
    [InlineData('m', true)]
    public void IsValidDriveChar_ValidLetters_ReturnsTrue(char c, bool expected)
    {
        var method = typeof(PathHelper).GetMethod("IsValidDriveChar",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = (bool)method!.Invoke(null, new object[] { c })!;
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData('0')]
    [InlineData('9')]
    [InlineData('@')]
    [InlineData('[')]
    [InlineData('`')]
    [InlineData('{')]
    [InlineData('-')]
    [InlineData('_')]
    [InlineData(' ')]
    [InlineData(':')]
    [InlineData('\\')]
    [InlineData('/')]
    public void IsValidDriveChar_InvalidCharacters_ReturnsFalse(char c)
    {
        var method = typeof(PathHelper).GetMethod("IsValidDriveChar",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = (bool)method!.Invoke(null, new object[] { c })!;
        Assert.False(result);
    }

    #endregion

    #region IsSeparator

    [Theory]
    [InlineData('/', true)]
    [InlineData('\\', true)]
    public void IsSeparator_Separators_ReturnsTrue(char c, bool expected)
    {
        var method = typeof(PathHelper).GetMethod("IsSeparator",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = (bool)method!.Invoke(null, new object[] { c })!;
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData('a')]
    [InlineData('Z')]
    [InlineData('0')]
    [InlineData(':')]
    [InlineData('.')]
    [InlineData(' ')]
    [InlineData('-')]
    public void IsSeparator_NonSeparators_ReturnsFalse(char c)
    {
        var method = typeof(PathHelper).GetMethod("IsSeparator",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = (bool)method!.Invoke(null, new object[] { c })!;
        Assert.False(result);
    }

    #endregion

    #region Integration Tests - Cross-Feature

    [Fact]
    public void PathHelper_CompleteWorkflow_CombineAndNormalize()
    {
        // Combine paths with structure that needs normalization
        var result = PathHelper.Combine(
            PathOptions.Unix,
            "/home/user",
            "documents",
            "..",
            "downloads",
            "file.txt"
        );

        // Should combine and normalize
        Assert.Equal("/home/user/downloads/file.txt", result);
    }

    [Fact]
    public void PathHelper_WindowsWorkflow_AllFeatures()
    {
        var result = PathHelper.Combine(
            PathOptions.Windows,
            @"C:\Users",
            "Public",
            "Documents",
            ".",
            "..",
            "Pictures",
            "photo.jpg"
        );

        // Should use backslashes and normalize dots
        Assert.Contains("\\", result);
        Assert.DoesNotContain("/", result);
        Assert.Contains("Public", result);
        Assert.Contains("Pictures", result);
        Assert.DoesNotContain("..", result);
    }

    [Fact]
    public void PathHelper_UncWorkflow_PreservesUncRoot()
    {
        var result = PathHelper.Combine(
            PathOptions.Windows,
            @"\\server\share",
            "folder1",
            "folder2",
            "..",
            "file.txt"
        );

        // Should preserve UNC root and normalize structure
        Assert.StartsWith(@"\\server\share", result);
        Assert.Contains("folder1", result);
        Assert.Contains("file.txt", result);
        Assert.DoesNotContain("folder2", result); // Removed by .. normalization
    }

    [Fact]
    public void PathHelper_ExtendedLengthPath_PreservesPrefix()
    {
        var result = PathHelper.Combine(
            PathOptions.Windows,
            @"\\?\C:\very\long\path",
            "more",
            "segments"
        );

        Assert.StartsWith(@"\\?\C:", result);
        Assert.Contains("more", result);
        Assert.Contains("segments", result);
    }

    [Fact]
    public void PathHelper_UnicodeWithNormalization_Works()
    {
        // Use decomposed Unicode
        var decomposed = "café"; // May be decomposed depending on how entered
        var result = PathHelper.Combine(
            PathOptions.Unix with { NormalizeUnicode = true },
            decomposed,
            "menu.txt"
        );

        Assert.Contains("café", result);
        Assert.Contains("menu.txt", result);
    }

    #endregion

    #region Edge Cases - Empty and Null

    [Fact]
    public void PathHelper_AllMethodsHandleNull_Appropriately()
    {
        // Methods that should throw on null
        Assert.Throws<ArgumentNullException>(() => PathHelper.NormalizeStructure(null!));
        Assert.Throws<ArgumentNullException>(() => PathHelper.ToUnixPath(null!));
        Assert.Throws<ArgumentNullException>(() => PathHelper.ToWindowsPath(null!));
        Assert.Throws<ArgumentNullException>(() => PathHelper.ToNativePath(null!));
        Assert.Throws<ArgumentNullException>(() => PathHelper.EnsureTrailingSeparator(null!));
        Assert.Throws<ArgumentNullException>(() => PathHelper.RemoveTrailingSeparator(null!));
        Assert.Throws<ArgumentNullException>(() => PathHelper.NormalizeUnicode(null!));
    }

    [Fact]
    public void PathHelper_CombineWithAllNull_HandlesGracefully()
    {
        // Should throw since RequireAtLeastOneSegment is true by default
        Assert.Throws<ArgumentException>(() =>
            PathHelper.Combine(PathOptions.Default, null, null, null));
    }

    #endregion

    #region Behavior Consistency Tests

    [Fact]
    public void PathHelper_SeparatorNormalization_IsConsistent()
    {
        var mixedPath = @"base/dir\file";

        var unixResult = PathHelper.ToUnixPath(mixedPath);
        var windowsResult = PathHelper.ToWindowsPath(mixedPath);

        Assert.DoesNotContain("\\", unixResult);
        Assert.DoesNotContain("/", windowsResult);

        // Converting back should be reversible (content-wise)
        var backToUnix = PathHelper.ToUnixPath(windowsResult);
        Assert.Equal(unixResult, backToUnix);
    }

    [Fact]
    public void PathHelper_StructureNormalization_IsIdempotent()
    {
        var path = @"dir1/./dir2/../dir3//file";

        var firstPass = PathHelper.NormalizeStructure(path);
        var secondPass = PathHelper.NormalizeStructure(firstPass);

        Assert.Equal(firstPass, secondPass);
    }

    [Fact]
    public void PathHelper_UnicodeNormalization_IsIdempotent()
    {
        var decomposed = "caf\u0065\u0301"; // café in NFD

        var firstPass = PathHelper.NormalizeUnicode(decomposed);
        var secondPass = PathHelper.NormalizeUnicode(firstPass);

        Assert.Equal(firstPass, secondPass);
    }

    #endregion

    #region Performance and Stress Tests

    [Fact]
    public void PathHelper_VeryDeepPath_HandlesCorrectly()
    {
        var segments = new List<string> { "C:\\root" };
        for (int i = 0; i < 100; i++)
        {
            segments.Add($"level{i}");
        }

        var result = PathHelper.Combine(PathOptions.Windows, segments.ToArray());

        Assert.StartsWith("C:", result);
        Assert.Contains("level0", result);
        Assert.Contains("level99", result);
    }

    [Fact]
    public void PathHelper_ManyDotsInPath_NormalizesCorrectly()
    {
        var segments = new[] { "/start" }.Concat(Enumerable.Repeat("..", 50)).Concat(new[] { "end" }).ToArray();

        var result = PathHelper.NormalizeStructure(string.Join("/", segments));

        // /start/../../../... clamps at root /, then adds end = /end
        Assert.DoesNotContain("..", result);  // All .. resolved (clamped at root)
        Assert.Contains("end", result);
        Assert.Equal("/end", result);
    }

    [Fact]
    public void PathHelper_LongPathNames_HandlesCorrectly()
    {
        var longName = new string('x', 255); // Max filename length on many systems
        var result = PathHelper.Combine(PathOptions.Unix, "/base", longName, "file.txt");

        Assert.Contains(longName, result);
        Assert.Contains("file.txt", result);
    }

    #endregion

    #region Platform-Specific Behavior

    [Fact]
    public void PathHelper_NativeMode_MatchesPlatform()
    {
        var result = PathHelper.CombineNative("base", "dir", "file.txt");

        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows))
        {
            Assert.Contains("\\", result);
        }
        else
        {
            Assert.Contains("/", result);
        }
    }

    #endregion

    #region Malformed Path Handling

    [Theory]
    [InlineData(@"\\?\")]
    [InlineData(@"\\.\")]
    [InlineData(@"\??\")]
    public void PathHelper_MalformedDevicePath_ThrowsAppropriately(string malformedPath)
    {
        // Note: Reflection wraps exceptions in TargetInvocationException
        var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() => PathHelper_GetRootLength(malformedPath));
        Assert.IsType<ArgumentException>(ex.InnerException);
    }

    [Theory]
    [InlineData(@"\\")]
    [InlineData(@"//")]
    public void PathHelper_MalformedUncPath_ThrowsAppropriately(string malformedPath)
    {
        // Note: Reflection wraps exceptions in TargetInvocationException
        var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() => PathHelper_GetRootLength(malformedPath));
        Assert.IsType<ArgumentException>(ex.InnerException);
    }

    #endregion

    #region Special Character Handling

    [Theory]
    [InlineData("path with spaces/file.txt")]
    [InlineData("path-with-dashes/file.txt")]
    [InlineData("path_with_underscores/file.txt")]
    [InlineData("path.with.dots/file.txt")]
    [InlineData("path(with)parens/file.txt")]
    [InlineData("path[with]brackets/file.txt")]
    public void PathHelper_SpecialCharacters_PreservesThem(string path)
    {
        var result = PathHelper.NormalizeStructure(path);
        // Should preserve special characters, only normalize structure
        Assert.Contains(path.Split('/')[0], result);
    }

    [Theory]
    [InlineData("文档/ファイル/файл.txt")]
    [InlineData("한글/中文/العربية")]
    [InlineData("emoji😀/test/file.txt")]
    public void PathHelper_InternationalCharacters_PreservesThem(string path)
    {
        var result = PathHelper.NormalizeStructure(path);
        // Should preserve international characters
        Assert.NotEmpty(result);
    }

    #endregion
}
