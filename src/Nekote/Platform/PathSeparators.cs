namespace Nekote.Platform;

/// <summary>
/// Provides directory and path separator characters used by different operating systems.
/// </summary>
public static class PathSeparators
{
    /// <summary>
    /// Gets the Windows path separator (<c>\</c>).
    /// </summary>
    /// <remarks>
    /// Used by Windows file systems (e.g., <c>C:\Users\Name\file.txt</c>).
    /// </remarks>
    public static char Windows => '\\';

    /// <summary>
    /// Gets the Unix/Linux/macOS path separator (<c>/</c>).
    /// </summary>
    /// <remarks>
    /// Used by Unix-like systems (e.g., <c>/home/user/file.txt</c>).
    /// Also used in URIs and URLs.
    /// </remarks>
    public static char Unix => '/';

    /// <summary>
    /// Gets the platform-native path separator for the current operating system.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="Windows"/> on Windows and <see cref="Unix"/> on Unix-like systems.
    /// Equivalent to <see cref="Path.DirectorySeparatorChar"/>.
    /// </remarks>
    public static char Native => Path.DirectorySeparatorChar;
}
