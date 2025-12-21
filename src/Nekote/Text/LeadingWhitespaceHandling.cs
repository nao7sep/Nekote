namespace Nekote.Text;

/// <summary>
/// Specifies how leading whitespace at the beginning of a line should be handled.
/// </summary>
public enum LeadingWhitespaceHandling
{
    /// <summary>
    /// Preserve all leading whitespace characters as-is.
    /// </summary>
    Preserve,

    /// <summary>
    /// Remove all leading whitespace characters.
    /// </summary>
    Remove
}
