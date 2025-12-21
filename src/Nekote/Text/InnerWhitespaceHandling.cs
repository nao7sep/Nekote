namespace Nekote.Text;

/// <summary>
/// Specifies how consecutive whitespace characters within a line (between leading and trailing) should be handled.
/// </summary>
public enum InnerWhitespaceHandling
{
    /// <summary>
    /// Preserve all whitespace characters as-is, including consecutive whitespace sequences.
    /// </summary>
    Preserve,

    /// <summary>
    /// Collapse consecutive whitespace sequences to a single replacement character or string.
    /// The replacement value is specified via method parameters.
    /// </summary>
    Collapse,

    /// <summary>
    /// Remove all inner whitespace characters entirely.
    /// </summary>
    Remove
}
