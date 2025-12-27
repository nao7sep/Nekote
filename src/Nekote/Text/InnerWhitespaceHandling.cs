namespace Nekote.Text;

/// <summary>
/// Specifies how consecutive whitespace characters within a line (between leading and trailing) should be handled.
/// </summary>
public enum InnerWhitespaceHandling
{
    /// <summary>
    /// Preserve consecutive whitespace as-is.
    /// </summary>
    Preserve,

    /// <summary>
    /// Collapse consecutive whitespace to a single replacement character (see <see cref="LineProcessingOptions.InnerWhitespaceReplacement"/>).
    /// </summary>
    Collapse,

    /// <summary>
    /// Remove all inner whitespace.
    /// </summary>
    Remove
}
