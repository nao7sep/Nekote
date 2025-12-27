namespace Nekote.Platform;

/// <summary>
/// Specifies how path separators should be normalized.
/// </summary>
public enum PathSeparatorMode
{
    /// <summary>Preserve existing separators.</summary>
    Preserve,

    /// <summary>Convert to platform-native separator.</summary>
    Native,

    /// <summary>Convert to Unix-style forward slashes.</summary>
    Unix,

    /// <summary>Convert to Windows-style backslashes.</summary>
    Windows
}
