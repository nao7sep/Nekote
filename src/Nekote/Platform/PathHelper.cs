namespace Nekote.Platform;

/// <summary>
/// Provides path manipulation utilities not available in <see cref="System.IO.Path"/>.
/// </summary>
/// <remarks>
/// This class only includes functionality that .NET's <see cref="System.IO.Path"/> class does not provide.
/// For standard path operations (Combine, GetFileName, GetExtension, etc.), use <see cref="System.IO.Path"/> directly.
/// 
/// The class is organized into partial files for AI-editing efficiency:
/// - <c>PathHelper.RootParsing.cs</c> - Root detection and parsing (device, extended, UNC, drive, simple roots)
/// - <c>PathHelper.Combining.cs</c> - Path combining operations (complete feature following external specs)
/// - <c>PathHelper.Normalization.cs</c> - All normalization operations: structure, Unicode, separators, trailing (complete feature following external specs)
/// - <c>PathHelper.cs</c> - Utility methods and helpers (drive letter validation, separator checking)
/// 
/// Subcategories are created only for "complete features" that follow external specifications and are large enough to deserve their own file.
/// Anything else stays in the main file.
/// </remarks>
public static partial class PathHelper
{
    #region Path Analysis

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
    internal static bool IsSeparator(char c)
    {
        return c == '/' || c == '\\';
    }

    #endregion
}
