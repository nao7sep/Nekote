namespace Nekote.Platform;

/// <summary>
/// Specifies how trailing path separators should be handled.
/// </summary>
public enum TrailingSeparatorHandling
{
    /// <summary>
    /// Preserve the trailing separator as-is (present or absent).
    /// </summary>
    Preserve,

    /// <summary>
    /// Remove the trailing separator if present.
    /// </summary>
    Remove,

    /// <summary>
    /// Ensure a trailing separator is present, adding it if missing.
    /// </summary>
    Ensure
}
