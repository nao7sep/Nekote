namespace Nekote.Text;

/// <summary>
/// Defines how trailing whitespace at the end of each line is handled.
/// </summary>
public enum LineEndWhitespaceMode
{
    /// <summary>
    /// Keep trailing whitespace unchanged.
    /// </summary>
    Preserve,

    /// <summary>
    /// Remove trailing whitespace from each line.
    /// </summary>
    Trim
}
