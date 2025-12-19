namespace Nekote.Text;

/// <summary>
/// Defines how consecutive blank lines within text are handled.
/// </summary>
/// <remarks>
/// A blank line is defined as a line containing only whitespace characters or completely empty.
/// This is equivalent to a line where <see cref="string.IsNullOrWhiteSpace"/> returns true.
/// Conversely, a visible or meaningful line contains at least one non-whitespace character.
/// </remarks>
public enum ConsecutiveBlankLinesMode
{
    /// <summary>
    /// Keep all blank lines unchanged.
    /// </summary>
    Preserve,

    /// <summary>
    /// Collapse multiple consecutive blank lines to a single empty line.
    /// </summary>
    /// <remarks>
    /// When multiple blank lines appear consecutively between visible content lines,
    /// they will be replaced with a single completely empty line (zero characters, not
    /// even whitespace). The content of the original blank lines is not preserved -
    /// a new empty line is used regardless of what whitespace the original blank lines contained.
    /// <example>
    /// Input:
    ///   Line 1
    ///
    ///
    ///   Line 2
    ///
    /// Output:
    ///   Line 1
    ///
    ///   Line 2
    /// </example>
    /// </remarks>
    Collapse,

    /// <summary>
    /// Remove all blank lines between visible content.
    /// </summary>
    /// <remarks>
    /// All blank lines appearing between lines with visible content will be omitted.
    /// Visible lines will appear consecutively with no blank lines between them.
    /// </remarks>
    Omit
}
