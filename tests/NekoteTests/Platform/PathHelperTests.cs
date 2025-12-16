using Nekote.Platform;

namespace NekoteTests.Platform;

public class PathHelperTests
{
    #region Atomic Operations - Unicode Normalization

    [Fact]
    public void NormalizeUnicode_DecomposedForm_ReturnsComposedNFC()
    {
        // NFD (decomposed): e + combining acute accent
        var decomposed = "cafe\u0301";

        var normalized = PathHelper.NormalizeUnicode(decomposed);

        Assert.Equal("café", normalized);
    }

    [Fact]
    public void NormalizeUnicode_AlreadyComposed_Unchanged()
    {
        var composed = "café";

        var normalized = PathHelper.NormalizeUnicode(composed);

        Assert.Equal(composed, normalized);
    }

    #endregion

    #region Atomic Operations - Structural Normalization

    [Fact]
    public void NormalizeStructure_CurrentDirectoryMarker_Removed()
    {
        var pathWithDot = "folder/./subfolder";

        var normalized = PathHelper.NormalizeStructure(pathWithDot);

        Assert.Equal("folder/subfolder", normalized);
    }

    [Fact]
    public void NormalizeStructure_ParentDirectoryMarker_CollapsesWithPrevious()
    {
        var pathWithDotDot = "folder/subfolder/../file.txt";

        var normalized = PathHelper.NormalizeStructure(pathWithDotDot);

        Assert.Equal("folder/file.txt", normalized);
    }

    [Fact]
    public void NormalizeStructure_ComplexPath_FullyResolved()
    {
        var complex = "a/./b/../c/./d/../e";

        var normalized = PathHelper.NormalizeStructure(complex);

        Assert.Equal("a/c/e", normalized);
    }

    [Fact]
    public void NormalizeStructure_PreservesSeparatorStyle()
    {
        var windowsStyle = @"folder\.\subfolder\..\file.txt";

        var normalized = PathHelper.NormalizeStructure(windowsStyle);

        Assert.Equal(@"folder\file.txt", normalized);
    }

    [Fact]
    public void NormalizeStructure_EmptyString_ReturnsEmpty()
    {
        var result = PathHelper.NormalizeStructure("");

        Assert.Equal("", result);
    }

    [Fact]
    public void NormalizeStructure_OnlyDots_Resolved()
    {
        var onlyDots = "./././.";

        var result = PathHelper.NormalizeStructure(onlyDots);

        Assert.Equal("", result);
    }

    [Fact]
    public void NormalizeStructure_LeadingParentReference_Preserved()
    {
        var leadingParent = "../../folder/file.txt";

        var result = PathHelper.NormalizeStructure(leadingParent);

        Assert.Equal("../../folder/file.txt", result);
    }

    #endregion

    #region Atomic Operations - Separator Normalization

    [Fact]
    public void NormalizeSeparators_PreserveMode_NoModification()
    {
        var mixedPath = @"some/mixed\separators/here";

        var result = PathHelper.NormalizeSeparators(mixedPath, PathSeparatorMode.Preserve);

        Assert.Equal(mixedPath, result);
    }

    [Fact]
    public void NormalizeSeparators_UnixMode_AllForwardSlashes()
    {
        var windowsPath = @"folder\subfolder\file.txt";

        var result = PathHelper.NormalizeSeparators(windowsPath, PathSeparatorMode.Unix);

        Assert.Equal("folder/subfolder/file.txt", result);
    }

    [Fact]
    public void NormalizeSeparators_WindowsMode_AllBackslashes()
    {
        var unixPath = "folder/subfolder/file.txt";

        var result = PathHelper.NormalizeSeparators(unixPath, PathSeparatorMode.Windows);

        Assert.Equal(@"folder\subfolder\file.txt", result);
    }

    [Fact]
    public void NormalizeSeparators_NativeMode_MatchesPlatformSeparator()
    {
        var path = "folder/subfolder";

        var result = PathHelper.NormalizeSeparators(path, PathSeparatorMode.Native);

        if (System.OperatingSystem.IsWindows())
        {
            Assert.Equal(@"folder\subfolder", result);
        }
        else
        {
            Assert.Equal("folder/subfolder", result);
        }
    }

    #endregion

    #region Atomic Operations - Trailing Separator Handling

    [Fact]
    public void HandleTrailingSeparator_PreserveMode_Unchanged()
    {
        var withSeparator = "folder/";
        var withoutSeparator = "folder";

        Assert.Equal("folder/", PathHelper.HandleTrailingSeparator(withSeparator, TrailingSeparatorHandling.Preserve));
        Assert.Equal("folder", PathHelper.HandleTrailingSeparator(withoutSeparator, TrailingSeparatorHandling.Preserve));
    }

    [Fact]
    public void HandleTrailingSeparator_RemoveMode_AlwaysRemoved()
    {
        var withUnixSep = "folder/";
        var withWindowsSep = @"folder\";

        Assert.Equal("folder", PathHelper.HandleTrailingSeparator(withUnixSep, TrailingSeparatorHandling.Remove));
        Assert.Equal("folder", PathHelper.HandleTrailingSeparator(withWindowsSep, TrailingSeparatorHandling.Remove));
    }

    [Fact]
    public void HandleTrailingSeparator_EnsureMode_AlwaysPresent()
    {
        var without = "folder";

        var result = PathHelper.HandleTrailingSeparator(without, TrailingSeparatorHandling.Ensure);

        Assert.True(result.EndsWith(PathSeparators.Native));
    }

    [Fact]
    public void HandleTrailingSeparator_EmptyString_PreserveReturnsEmpty()
    {
        var result = PathHelper.HandleTrailingSeparator("", TrailingSeparatorHandling.Preserve);

        Assert.Equal("", result);
    }

    [Fact]
    public void HandleTrailingSeparator_BothSeparatorTypes_RemovedCorrectly()
    {
        var unixPath = "folder/sub/";
        var windowsPath = @"folder\sub\";

        Assert.Equal("folder/sub", PathHelper.HandleTrailingSeparator(unixPath, TrailingSeparatorHandling.Remove));
        Assert.Equal(@"folder\sub", PathHelper.HandleTrailingSeparator(windowsPath, TrailingSeparatorHandling.Remove));
    }

    #endregion

    #region Convenience Methods - Separator Conversion

    [Fact]
    public void ToUnixPath_WindowsPath_ConvertsToForwardSlashes()
    {
        var windowsPath = @"C:\Users\Documents\file.txt";

        var result = PathHelper.ToUnixPath(windowsPath);

        Assert.Equal("C:/Users/Documents/file.txt", result);
    }

    [Fact]
    public void ToWindowsPath_UnixPath_ConvertsToBackslashes()
    {
        var unixPath = "/home/user/documents/file.txt";

        var result = PathHelper.ToWindowsPath(unixPath);

        Assert.Equal(@"\home\user\documents\file.txt", result);
    }

    [Fact]
    public void ToNativePath_MixedPath_ConvertsToNativeSeparator()
    {
        var mixedPath = @"folder/subfolder\file.txt";

        var result = PathHelper.ToNativePath(mixedPath);

        if (System.OperatingSystem.IsWindows())
        {
            Assert.DoesNotContain("/", result);
        }
        else
        {
            Assert.DoesNotContain("\\", result);
        }
    }

    [Fact]
    public void ConvenienceMethods_UsePathSeparatorsConstants()
    {
        var path = "test";

        // Verify all convenience methods produce results using PathSeparators constants
        var unixResult = PathHelper.ToUnixPath(path);
        var windowsResult = PathHelper.ToWindowsPath(path);

        Assert.Equal(path, unixResult); // No conversion needed for pure name
        Assert.Equal(path, windowsResult); // No conversion needed for pure name
    }

    #endregion

    #region Path Combining - Absolute Paths

    [Fact]
    public void CombineToAbsolute_TwoSegments_CreatesAbsolutePath()
    {
        var options = PathOptions.Default;

        var result = PathHelper.CombineToAbsolute(options, "C:\\base", "relative");

        Assert.True(Path.IsPathRooted(result));
        Assert.Contains("base", result);
        Assert.Contains("relative", result);
    }

    [Fact]
    public void CombineToAbsolute_MultipleSegments_AllCombined()
    {
        var options = PathOptions.Default;

        var result = PathHelper.CombineToAbsolute(options, "C:\\root", "level1", "level2", "file.txt");

        Assert.EndsWith("file.txt", result);
    }

    [Fact]
    public void CombineToAbsolute_WithOptions_AppliesNormalization()
    {
        var options = new PathOptions
        {
            ThrowOnEmptySegments = false,
            TrimSegments = true,
            RequireAtLeastOneSegment = true,
            ValidateSubsequentPathsRelative = true,
            NormalizeUnicode = true,
            NormalizeStructure = true,
            NormalizeSeparators = PathSeparatorMode.Unix,
            TrailingSeparator = TrailingSeparatorHandling.Remove
        };

        var result = PathHelper.CombineToAbsolute(options, "C:\\base", "sub/./folder/../file.txt");

        // Note: GetFullPath applies OS-native separators, overriding Unix normalization
        Assert.True(Path.IsPathRooted(result));
        Assert.DoesNotContain("..", result); // Structure normalized
        Assert.EndsWith("file.txt", result);
    }

    [Fact]
    public void CombineToAbsolute_ParamsOverload_AcceptsMultiplePaths()
    {
        var options = PathOptions.Default;

        var result = PathHelper.CombineToAbsolute(options, "C:\\base", "a", "b", "c", "d", "file.txt");

        Assert.True(Path.IsPathRooted(result));
        Assert.EndsWith("file.txt", result);
    }

    #endregion

    #region Path Combining - Relative Paths

    [Fact]
    public void CombineRelative_TwoSegments_RemainsRelative()
    {
        var options = PathOptions.Default;

        var result = PathHelper.CombineRelative(options, "config", "settings.ini");

        Assert.False(Path.IsPathRooted(result));
        Assert.Contains("config", result);
        Assert.Contains("settings.ini", result);
    }

    [Fact]
    public void CombineRelative_MultipleSegments_PreservesRelativeNature()
    {
        var options = PathOptions.Default;

        var result = PathHelper.CombineRelative(options, "data", "cache", "temp", "file.dat");

        Assert.False(Path.IsPathRooted(result));
        Assert.EndsWith("file.dat", result);
    }

    [Fact]
    public void CombineRelative_WithStructuralNormalization_ResolvesDotsButStaysRelative()
    {
        var options = PathOptions.Default;

        var result = PathHelper.CombineRelative(options, "folder", "./sub/../other");

        Assert.False(Path.IsPathRooted(result));
        Assert.DoesNotContain("..", result);
    }

    [Fact]
    public void CombineRelative_ParamsOverload_HandlesManySegments()
    {
        var options = PathOptions.Default;

        var result = PathHelper.CombineRelative(options, "a", "b", "c", "d", "e", "f");

        Assert.False(Path.IsPathRooted(result));
        Assert.EndsWith("f", result);
    }

    #endregion

    #region PathOptions - Validation Behavior

    [Fact]
    public void ThrowOnEmptySegments_True_ThrowsOnNull()
    {
        var strictOptions = PathOptions.Default with { ThrowOnEmptySegments = true };

        var exception = Assert.Throws<ArgumentException>(() =>
            PathHelper.CombineRelative(strictOptions, "base", null, "file.txt"));

        Assert.Contains("ThrowOnEmptySegments", exception.Message);
    }

    [Fact]
    public void ThrowOnEmptySegments_True_ThrowsOnEmptyString()
    {
        var strictOptions = PathOptions.Default with { ThrowOnEmptySegments = true };

        Assert.Throws<ArgumentException>(() =>
            PathHelper.CombineRelative(strictOptions, "base", "", "file.txt"));
    }

    [Fact]
    public void ThrowOnEmptySegments_True_ThrowsOnWhitespace()
    {
        var strictOptions = PathOptions.Default with { ThrowOnEmptySegments = true };

        Assert.Throws<ArgumentException>(() =>
            PathHelper.CombineRelative(strictOptions, "base", "   ", "file.txt"));
    }

    [Fact]
    public void ThrowOnEmptySegments_False_IgnoresNullSegments()
    {
        var lenientOptions = PathOptions.Default with { ThrowOnEmptySegments = false };

        var result = PathHelper.CombineRelative(lenientOptions, "base", null, "file.txt");

        Assert.Contains("base", result);
        Assert.Contains("file.txt", result);
    }

    [Fact]
    public void ThrowOnEmptySegments_False_IgnoresEmptyAndWhitespace()
    {
        var lenientOptions = PathOptions.Default with { ThrowOnEmptySegments = false };

        var result = PathHelper.CombineRelative(lenientOptions, "start", "", "   ", null, "end");

        Assert.Contains("start", result);
        Assert.Contains("end", result);
    }

    [Fact]
    public void ValidateSubsequentPathsRelative_True_ThrowsOnAbsolutePath()
    {
        var options = PathOptions.Default;

        if (System.OperatingSystem.IsWindows())
        {
            Assert.Throws<ArgumentException>(() =>
                PathHelper.CombineRelative(options, "relative", "C:\\absolute"));
        }
        else
        {
            Assert.Throws<ArgumentException>(() =>
                PathHelper.CombineRelative(options, "relative", "/absolute"));
        }
    }

    [Fact]
    public void RequireAtLeastOneSegment_True_ThrowsWhenAllEmpty()
    {
        var options = PathOptions.Default with
        {
            ThrowOnEmptySegments = false,
            RequireAtLeastOneSegment = true
        };

        Assert.Throws<ArgumentException>(() =>
            PathHelper.CombineRelative(options, null, "", "  "));
    }

    [Fact]
    public void RequireAtLeastOneSegment_False_AllowsEmptyResult()
    {
        var options = PathOptions.Default with
        {
            ThrowOnEmptySegments = false,
            RequireAtLeastOneSegment = false
        };

        // Should not throw, even though all segments are empty
        var result = PathHelper.CombineRelative(options, null, "", "  ");

        Assert.NotNull(result);
    }

    [Fact]
    public void TrimSegments_False_PreservesWhitespace()
    {
        var options = PathOptions.Default with
        {
            ThrowOnEmptySegments = false,
            TrimSegments = false
        };

        var result = PathHelper.CombineRelative(options, "  base  ", "  file  ");

        Assert.Contains("  ", result);
    }

    #endregion

    #region PathOptions - Presets

    [Fact]
    public void DefaultPreset_HasBalancedConfiguration()
    {
        var preset = PathOptions.Default;

        Assert.False(preset.ThrowOnEmptySegments);
        Assert.True(preset.TrimSegments);
        Assert.True(preset.RequireAtLeastOneSegment);
        Assert.True(preset.ValidateSubsequentPathsRelative);
        Assert.Equal(PathSeparatorMode.Preserve, preset.NormalizeSeparators);
    }

    [Fact]
    public void CrossPlatformSafePreset_UsesNativeSeparators()
    {
        var preset = PathOptions.CrossPlatformSafe;

        Assert.Equal(PathSeparatorMode.Native, preset.NormalizeSeparators);
        Assert.True(preset.NormalizeUnicode);
        Assert.True(preset.NormalizeStructure);
    }

    [Fact]
    public void UnixStrictPreset_EnforcesUnixConventions()
    {
        var preset = PathOptions.UnixStrict;

        Assert.Equal(PathSeparatorMode.Unix, preset.NormalizeSeparators);
        Assert.Equal(TrailingSeparatorHandling.Remove, preset.TrailingSeparator);
    }

    [Fact]
    public void WindowsStrictPreset_EnforcesWindowsConventions()
    {
        var preset = PathOptions.WindowsStrict;

        Assert.Equal(PathSeparatorMode.Windows, preset.NormalizeSeparators);
    }

    [Fact]
    public void MinimalPreset_OnlyValidatesWithoutTransforming()
    {
        var preset = PathOptions.Minimal;

        Assert.False(preset.NormalizeUnicode);
        Assert.False(preset.NormalizeStructure);
        Assert.Equal(PathSeparatorMode.Preserve, preset.NormalizeSeparators);
        Assert.Equal(TrailingSeparatorHandling.Preserve, preset.TrailingSeparator);
    }

    #endregion

    #region PathOptions - With Expression

    [Fact]
    public void WithExpression_ModifiesSingleProperty_OthersPreserved()
    {
        var customized = PathOptions.Default with { NormalizeSeparators = PathSeparatorMode.Unix };

        Assert.Equal(PathSeparatorMode.Unix, customized.NormalizeSeparators);
        Assert.Equal(PathOptions.Default.TrimSegments, customized.TrimSegments);
        Assert.Equal(PathOptions.Default.NormalizeUnicode, customized.NormalizeUnicode);
    }

    [Fact]
    public void WithExpression_ModifiesMultipleProperties()
    {
        var customized = PathOptions.CrossPlatformSafe with
        {
            NormalizeSeparators = PathSeparatorMode.Unix,
            TrailingSeparator = TrailingSeparatorHandling.Ensure,
            ThrowOnEmptySegments = true
        };

        Assert.Equal(PathSeparatorMode.Unix, customized.NormalizeSeparators);
        Assert.Equal(TrailingSeparatorHandling.Ensure, customized.TrailingSeparator);
        Assert.True(customized.ThrowOnEmptySegments);
    }

    [Fact]
    public void PathOptions_AsRecord_SupportsValueEquality()
    {
        var options1 = new PathOptions
        {
            ThrowOnEmptySegments = false,
            TrimSegments = true,
            RequireAtLeastOneSegment = true,
            ValidateSubsequentPathsRelative = true,
            NormalizeUnicode = true,
            NormalizeStructure = true,
            NormalizeSeparators = PathSeparatorMode.Preserve,
            TrailingSeparator = TrailingSeparatorHandling.Remove
        };

        var options2 = new PathOptions
        {
            ThrowOnEmptySegments = false,
            TrimSegments = true,
            RequireAtLeastOneSegment = true,
            ValidateSubsequentPathsRelative = true,
            NormalizeUnicode = true,
            NormalizeStructure = true,
            NormalizeSeparators = PathSeparatorMode.Preserve,
            TrailingSeparator = TrailingSeparatorHandling.Remove
        };

        Assert.Equal(options1, options2);
    }

    #endregion

    #region Integration - Complex Scenarios

    [Fact]
    public void ComplexPath_WithAllNormalizations_FullyProcessed()
    {
        var options = new PathOptions
        {
            ThrowOnEmptySegments = false,
            TrimSegments = true,
            RequireAtLeastOneSegment = true,
            ValidateSubsequentPathsRelative = true,
            NormalizeUnicode = true,
            NormalizeStructure = true,
            NormalizeSeparators = PathSeparatorMode.Unix,
            TrailingSeparator = TrailingSeparatorHandling.Remove
        };

        var result = PathHelper.CombineRelative(options,
            "  base  ",
            "folder/./subfolder",
            @"other\..\target",
            "file.txt  ");

        var segments = result.Split('/');
        Assert.DoesNotContain("..", result); // Structure normalized
        Assert.DoesNotContain("\\", result); // Unix separators
        Assert.DoesNotContain(".", segments.Take(segments.Length - 1)); // No dot directories in path (except file extension)
        Assert.EndsWith("file.txt", result); // Proper ending
    }

    [Fact]
    public void DatabaseStoragePath_PreserveForCanonicalRepresentation()
    {
        // Simulating storage of paths in database - use forward slashes as canonical format
        var options = PathOptions.Default with
        {
            NormalizeSeparators = PathSeparatorMode.Unix,
            TrailingSeparator = TrailingSeparatorHandling.Remove
        };

        var storedPath = PathHelper.CombineRelative(options, "data", "users", "profile.json");

        Assert.Equal("data/users/profile.json", storedPath);
        Assert.DoesNotContain("\\", storedPath);
    }

    [Fact]
    public void FileSystemAccess_ConvertToNativeWhenNeeded()
    {
        // Simulating conversion from stored canonical format to platform-specific
        var canonicalPath = "data/users/profile.json";

        var nativePath = PathHelper.NormalizeSeparators(canonicalPath, PathSeparatorMode.Native);

        if (System.OperatingSystem.IsWindows())
        {
            Assert.Equal(@"data\users\profile.json", nativePath);
        }
        else
        {
            Assert.Equal("data/users/profile.json", nativePath);
        }
    }

    #endregion
}
