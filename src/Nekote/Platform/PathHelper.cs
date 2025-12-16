namespace Nekote.Platform;

/// <summary>
/// Provides path manipulation utilities not available in <see cref="System.IO.Path"/>.
/// </summary>
/// <remarks>
/// This class only includes functionality that .NET's <see cref="System.IO.Path"/> class does not provide.
/// For standard path operations (Combine, GetFileName, GetExtension, etc.), use <see cref="System.IO.Path"/> directly.
/// </remarks>
public static class PathHelper
{
    /// <summary>
    /// Converts all path separators in the specified path to Unix-style forward slashes (<c>/</c>).
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The path with all backslashes replaced by forward slashes.</returns>
    public static string ToUnixPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return path.Replace(PathSeparators.Windows, PathSeparators.Unix);
    }

    /// <summary>
    /// Converts all path separators in the specified path to Windows-style backslashes (<c>\</c>).
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The path with all forward slashes replaced by backslashes.</returns>
    public static string ToWindowsPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return path.Replace(PathSeparators.Unix, PathSeparators.Windows);
    }

    /// <summary>
    /// Converts all path separators in the specified path to the native separator for the current platform.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>
    /// On Windows, returns the path with forward slashes replaced by backslashes.
    /// On Unix-like systems, returns the path with backslashes replaced by forward slashes.
    /// </returns>
    public static string ToNativePath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return PathSeparators.Native == PathSeparators.Windows
            ? ToWindowsPath(path)
            : ToUnixPath(path);
    }
}
