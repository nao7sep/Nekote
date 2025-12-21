namespace Nekote.Text;

/// <summary>
/// Specifies how leading blank lines before the first visible line should be handled.
/// </summary>
public enum LeadingBlankLineHandling
{
    /// <summary>
    /// Preserve all leading blank lines as-is.
    /// </summary>
    Preserve,

    /// <summary>
    /// Remove all leading blank lines.
    /// </summary>
    Remove
}
