using System;

namespace Nekote.Text;

/// <summary>
/// Configuration options for line-level text processing operations.
/// </summary>
/// <remarks>
/// Controls how individual lines and sequences of lines are processed, including whitespace handling,
/// line separator behavior, and blank line management. All properties are required. Use predefined instances
/// for common scenarios or use <c>with</c> expressions to customize specific properties.
/// </remarks>
public sealed record LineProcessingOptions
{
    public required LeadingWhitespaceHandling LeadingWhitespaceHandling { get; init; }

    /// <summary>
    /// How consecutive whitespace within each line should be handled.
    /// </summary>
    /// <remarks>
    /// When set to <see cref="InnerWhitespaceHandling.Collapse"/>, the replacement string
    /// is specified by <see cref="InnerWhitespaceReplacement"/>.
    /// </remarks>
    public required InnerWhitespaceHandling InnerWhitespaceHandling { get; init; }

    /// <summary>
    /// Replacement string when collapsing consecutive inner whitespace.
    /// </summary>
    /// <remarks>
    /// Only used when <see cref="InnerWhitespaceHandling"/> is <see cref="InnerWhitespaceHandling.Collapse"/>.
    /// Common values: <c>" "</c> (single space) or <c>""</c> (empty string).
    /// </remarks>
    public required string InnerWhitespaceReplacement { get; init; }

    public required TrailingWhitespaceHandling TrailingWhitespaceHandling { get; init; }

    public required LeadingBlankLineHandling LeadingBlankLineHandling { get; init; }

    public required InnerBlankLineHandling InnerBlankLineHandling { get; init; }

    public required TrailingBlankLineHandling TrailingBlankLineHandling { get; init; }

    /// <summary>
    /// Line separator to use when joining lines into text.
    /// </summary>
    /// <remarks>
    /// Common values: <c>"\n"</c> (Unix), <c>"\r\n"</c> (Windows), <c>" "</c> (space for joining lines),
    /// or <c>""</c> (empty for direct concatenation).
    /// </remarks>
    public required string NewLine { get; init; }

    /// <summary>Default options: preserve whitespace, normalize blank lines.</summary>
    public static readonly LineProcessingOptions Default = new()
    {
        LeadingWhitespaceHandling = LeadingWhitespaceHandling.Preserve,
        InnerWhitespaceHandling = InnerWhitespaceHandling.Preserve,
        InnerWhitespaceReplacement = " ",
        TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
        LeadingBlankLineHandling = LeadingBlankLineHandling.Remove,
        InnerBlankLineHandling = InnerBlankLineHandling.Collapse,
        TrailingBlankLineHandling = TrailingBlankLineHandling.Remove,
        NewLine = Environment.NewLine
    };

    /// <summary>Single-line options: removes whitespace/blank lines, joins with spaces.</summary>
    public static readonly LineProcessingOptions SingleLine = new()
    {
        LeadingWhitespaceHandling = LeadingWhitespaceHandling.Remove,
        InnerWhitespaceHandling = InnerWhitespaceHandling.Collapse,
        InnerWhitespaceReplacement = " ",
        TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
        LeadingBlankLineHandling = LeadingBlankLineHandling.Remove,
        InnerBlankLineHandling = InnerBlankLineHandling.Remove,
        TrailingBlankLineHandling = TrailingBlankLineHandling.Remove,
        NewLine = " "
    };

    /// <summary>Minimal options: aggressive whitespace/blank line removal.</summary>
    public static readonly LineProcessingOptions Minimal = new()
    {
        LeadingWhitespaceHandling = LeadingWhitespaceHandling.Remove,
        InnerWhitespaceHandling = InnerWhitespaceHandling.Remove,
        InnerWhitespaceReplacement = string.Empty,
        TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
        LeadingBlankLineHandling = LeadingBlankLineHandling.Remove,
        InnerBlankLineHandling = InnerBlankLineHandling.Remove,
        TrailingBlankLineHandling = TrailingBlankLineHandling.Remove,
        NewLine = string.Empty
    };
}
