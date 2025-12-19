namespace Nekote.Text;

/// <summary>
/// Defines how leading whitespace at the start of each line is handled.
/// </summary>
public enum LineStartWhitespaceMode
{
    /// <summary>
    /// Keep leading whitespace unchanged.
    /// </summary>
    Preserve,

    /// <summary>
    /// Remove leading whitespace from each line.
    /// </summary>
    Trim
}
