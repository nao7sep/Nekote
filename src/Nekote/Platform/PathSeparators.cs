namespace Nekote.Platform;

/// <summary>
/// Provides directory and path separator characters used by different operating systems.
/// </summary>
public static class PathSeparators
{
    /// <summary>
    /// Windows path separator (<c>\</c>).
    /// </summary>
    public static char Windows => '\\';

    /// <summary>
    /// Unix/Linux/macOS path separator (<c>/</c>).
    /// </summary>
    public static char Unix => '/';

    /// <summary>
    /// Platform-native path separator for the current operating system.
    /// </summary>
    public static char Native => Path.DirectorySeparatorChar;
}
