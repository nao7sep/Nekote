namespace Nekote.Text;

/// <summary>
/// Specifies how consecutive blank lines between visible content should be handled.
/// </summary>
public enum InnerBlankLineHandling
{
    /// <summary>Preserve consecutive blank lines as-is.</summary>
    Preserve,

    /// <summary>Collapse consecutive blank lines to a single one.</summary>
    Collapse,

    /// <summary>Remove blank lines between content.</summary>
    Remove
}
