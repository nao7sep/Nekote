namespace Nekote.Text;

/// <summary>
/// Specifies how trailing whitespace at the end of a line should be handled.
/// </summary>
public enum TrailingWhitespaceHandling
{
    /// <summary>
    /// Preserve all trailing whitespace characters as-is.
    /// </summary>
    Preserve,

    /// <summary>
    /// Remove all trailing whitespace characters.
    /// </summary>
    Remove
}
