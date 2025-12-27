namespace Nekote.Platform;

/// <summary>
/// Specifies how trailing path separators should be handled.
/// </summary>
public enum TrailingSeparatorHandling
{
    /// <summary>Preserve trailing separator as-is.</summary>
    Preserve,

    /// <summary>Remove trailing separator if present.</summary>
    Remove,

    /// <summary>Ensure trailing separator is present.</summary>
    Ensure
}
