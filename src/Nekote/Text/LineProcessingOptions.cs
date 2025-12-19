namespace Nekote.Text;

/// <summary>
/// Options for line processing operations.
/// </summary>
/// <remarks>
/// All properties are required and must be set explicitly to ensure intentional configuration.
/// Use predefined presets like <see cref="Default"/> for common scenarios.
/// </remarks>
public sealed record LineProcessingOptions
{
    /// <summary>
    /// How to handle leading whitespace at the start of each line.
    /// </summary>
    public required LineStartWhitespaceMode LineStart { get; init; }

    /// <summary>
    /// How to handle whitespace within line content.
    /// </summary>
    public required InlineWhitespaceMode InlineWhitespace { get; init; }

    /// <summary>
    /// How to handle trailing whitespace at the end of each line.
    /// </summary>
    public required LineEndWhitespaceMode LineEnd { get; init; }

    /// <summary>
    /// How to handle blank lines at the start of text.
    /// </summary>
    public required LeadingBlankLinesMode LeadingBlanks { get; init; }

    /// <summary>
    /// How to handle consecutive blank lines within text.
    /// </summary>
    public required ConsecutiveBlankLinesMode ConsecutiveBlanks { get; init; }

    /// <summary>
    /// How to handle blank lines at the end of text.
    /// </summary>
    public required TrailingBlankLinesMode TrailingBlanks { get; init; }

    /// <summary>
    /// Predefined: Preserve indentation, trim trailing whitespace, clean blank lines.
    /// </summary>
    /// <remarks>
    /// This is a balanced default for most text processing needs:
    /// - Preserves line indentation (leading whitespace)
    /// - Preserves inline whitespace (e.g., multiple spaces between words)
    /// - Trims trailing whitespace from lines
    /// - Omits leading blank lines
    /// - Collapses consecutive blank lines to single blank line
    /// - Omits trailing blank lines
    /// </remarks>
    public static readonly LineProcessingOptions Default = new()
    {
        LineStart = LineStartWhitespaceMode.Preserve,
        InlineWhitespace = InlineWhitespaceMode.Preserve,
        LineEnd = LineEndWhitespaceMode.Trim,
        LeadingBlanks = LeadingBlankLinesMode.Omit,
        ConsecutiveBlanks = ConsecutiveBlankLinesMode.Collapse,
        TrailingBlanks = TrailingBlankLinesMode.Omit
    };

    /// <summary>
    /// Predefined: Collapse all whitespace like HTML text rendering (useful for canonical comparison in tests).
    /// </summary>
    /// <remarks>
    /// Mimics how browsers render HTML text where whitespace is collapsed:
    /// - Trims leading whitespace from each line
    /// - Collapses inline whitespace to single spaces
    /// - Trims trailing whitespace from each line
    /// - Omits leading blank lines
    /// - Collapses consecutive blank lines to single blank line
    /// - Omits trailing blank lines
    /// Ideal for creating canonical text forms for comparison in unit tests.
    /// </remarks>
    public static readonly LineProcessingOptions Collapsed = new()
    {
        LineStart = LineStartWhitespaceMode.Trim,
        InlineWhitespace = InlineWhitespaceMode.Collapse,
        LineEnd = LineEndWhitespaceMode.Trim,
        LeadingBlanks = LeadingBlankLinesMode.Omit,
        ConsecutiveBlanks = ConsecutiveBlankLinesMode.Collapse,
        TrailingBlanks = TrailingBlankLinesMode.Omit
    };
}
