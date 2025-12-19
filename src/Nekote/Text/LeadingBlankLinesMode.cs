namespace Nekote.Text;

/// <summary>
/// Defines how blank lines at the start of text are handled.
/// </summary>
/// <remarks>
/// A blank line is defined as a line containing only whitespace characters or completely empty.
/// This is equivalent to a line where <see cref="string.IsNullOrWhiteSpace"/> returns true.
/// Conversely, a visible or meaningful line contains at least one non-whitespace character.
/// </remarks>
public enum LeadingBlankLinesMode
{
    /// <summary>
    /// Keep blank lines at the start of text.
    /// </summary>
    Preserve,

    /// <summary>
    /// Remove blank lines from the start of text.
    /// </summary>
    /// <remarks>
    /// All leading blank lines before the first visible line will be omitted from the output.
    /// </remarks>
    Omit
}
