namespace Nekote.Text;

/// <summary>
/// Defines how blank lines at the end of text are handled.
/// </summary>
/// <remarks>
/// A blank line is defined as a line containing only whitespace characters or completely empty.
/// This is equivalent to a line where <see cref="string.IsNullOrWhiteSpace"/> returns true.
/// Conversely, a visible or meaningful line contains at least one non-whitespace character.
/// </remarks>
public enum TrailingBlankLinesMode
{
    /// <summary>
    /// Keep blank lines at the end of text.
    /// </summary>
    Preserve,

    /// <summary>
    /// Remove blank lines from the end of text.
    /// </summary>
    /// <remarks>
    /// All trailing blank lines after the last visible line will be omitted from the output.
    /// </remarks>
    Omit
}
