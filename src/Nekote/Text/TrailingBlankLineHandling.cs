namespace Nekote.Text;

/// <summary>
/// Specifies how trailing blank lines after the last visible line should be handled.
/// </summary>
public enum TrailingBlankLineHandling
{
    /// <summary>
    /// Preserve all trailing blank lines as-is.
    /// </summary>
    Preserve,

    /// <summary>
    /// Remove all trailing blank lines.
    /// </summary>
    Remove
}
