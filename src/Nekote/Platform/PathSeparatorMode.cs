namespace Nekote.Platform;

/// <summary>
/// Specifies how path separators should be normalized.
/// </summary>
public enum PathSeparatorMode
{
    /// <summary>
    /// Preserve existing separators without modification (mixed separators allowed).
    /// </summary>
    Preserve,

    /// <summary>
    /// Convert all separators to the platform-native separator (backslash on Windows, forward slash on Unix).
    /// </summary>
    Native,

    /// <summary>
    /// Convert all separators to Unix-style forward slashes (<c>/</c>).
    /// </summary>
    Unix,

    /// <summary>
    /// Convert all separators to Windows-style backslashes (<c>\</c>).
    /// </summary>
    Windows
}
