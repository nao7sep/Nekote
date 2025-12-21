namespace Nekote.Text;

/// <summary>
/// Specifies how newline sequences should be handled when processing text.
/// </summary>
public enum NewlineHandling
{
    /// <summary>
    /// Preserve all newline sequences as-is.
    /// </summary>
    Preserve,

    /// <summary>
    /// Replace each newline sequence (\r\n, \r, or \n) with a replacement character or string.
    /// </summary>
    ReplaceEach,

    /// <summary>
    /// Collapse consecutive newline characters into a single replacement character or string.
    /// </summary>
    CollapseConsecutive,

    /// <summary>
    /// Remove all newline sequences entirely.
    /// </summary>
    Remove
}
