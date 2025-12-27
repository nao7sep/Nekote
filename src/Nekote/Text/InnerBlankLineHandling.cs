namespace Nekote.Text;

/// <summary>
/// Specifies how consecutive blank lines between visible content should be handled.
/// </summary>
public enum InnerBlankLineHandling
{
    /// <summary>
    /// Preserve all consecutive blank lines as-is.
    /// </summary>
    Preserve,

    /// <summary>
    /// Collapse consecutive blank lines into a single blank line.
    /// </summary>
    Collapse,

    /// <summary>
    /// Remove all blank lines between visible content.
    /// </summary>
    Remove
}
