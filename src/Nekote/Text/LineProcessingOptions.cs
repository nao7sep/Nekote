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
    /// <summary>
    /// Gets how leading whitespace at the beginning of each line should be handled.
    /// </summary>
    /// <remarks>
    /// This property is required.
    /// </remarks>
    public required LeadingWhitespaceHandling LeadingWhitespaceHandling { get; init; }

    /// <summary>
    /// Gets how consecutive whitespace characters within each line should be handled.
    /// </summary>
    /// <remarks>
    /// When set to <see cref="InnerWhitespaceHandling.Collapse"/>, the replacement character or string
    /// is specified by <see cref="InnerWhitespaceReplacement"/>. This property is required.
    /// </remarks>
    public required InnerWhitespaceHandling InnerWhitespaceHandling { get; init; }

    /// <summary>
    /// Gets the replacement character or string to use when collapsing consecutive inner whitespace.
    /// </summary>
    /// <remarks>
    /// This property is only used when <see cref="InnerWhitespaceHandling"/> is set to <see cref="InnerWhitespaceHandling.Collapse"/>.
    /// This property is required.
    /// </remarks>
    public required CharOrString InnerWhitespaceReplacement { get; init; }

    /// <summary>
    /// Gets how trailing whitespace at the end of each line should be handled.
    /// </summary>
    /// <remarks>
    /// This property is required.
    /// </remarks>
    public required TrailingWhitespaceHandling TrailingWhitespaceHandling { get; init; }

    /// <summary>
    /// Gets how leading blank lines before the first visible line should be handled.
    /// </summary>
    /// <remarks>
    /// A blank line is one that is null or contains only whitespace characters.
    /// This property is required.
    /// </remarks>
    public required LeadingBlankLineHandling LeadingBlankLineHandling { get; init; }

    /// <summary>
    /// Gets how consecutive blank lines between visible content should be handled.
    /// </summary>
    /// <remarks>
    /// A blank line is one that is null or contains only whitespace characters.
    /// This property is required.
    /// </remarks>
    public required InnerBlankLineHandling InnerBlankLineHandling { get; init; }

    /// <summary>
    /// Gets how trailing blank lines after the last visible line should be handled.
    /// </summary>
    /// <remarks>
    /// A blank line is one that is null or contains only whitespace characters.
    /// This property is required.
    /// </remarks>
    public required TrailingBlankLineHandling TrailingBlankLineHandling { get; init; }

    /// <summary>
    /// Gets the line separator string to use when joining lines back into a single text.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Common values include <c>"\n"</c> (Unix), <c>"\r\n"</c> (Windows), or <c>Environment.NewLine</c> (platform-specific).
    /// This property is required.
    /// </para>
    /// <para>
    /// Note: This property is a <c>string</c> rather than <see cref="CharOrString"/> because line separators
    /// must support multi-character sequences like <c>"\r\n"</c>. In contrast, the replacement properties
    /// (<see cref="InnerWhitespaceReplacement"/>, <see cref="NewlineReplacement"/>) use <see cref="CharOrString"/>
    /// with single-character defaults for performance, as they may be applied millions of times
    /// when processing large text files (e.g., server access logs, database dumps, CSV datasets, system logs).
    /// </para>
    /// </remarks>
    public required string NewLine { get; init; }

    /// <summary>
    /// Gets how newline sequences should be handled when flattening multi-line text into a single line.
    /// </summary>
    /// <remarks>
    /// This property is required.
    /// </remarks>
    public required NewlineHandling NewlineHandling { get; init; }

    /// <summary>
    /// Gets the replacement character or string to use when replacing or collapsing newlines.
    /// </summary>
    /// <remarks>
    /// This property is only used when <see cref="NewlineHandling"/> is set to <see cref="NewlineHandling.ReplaceEach"/>
    /// or <see cref="NewlineHandling.CollapseConsecutive"/>. This property is required.
    /// </remarks>
    public required CharOrString NewlineReplacement { get; init; }

    /// <summary>
    /// Default line processing options: preserve line whitespace structure, normalize blank lines, collapse newlines.
    /// </summary>
    /// <remarks>
    /// This configuration is suitable for most text processing scenarios:
    /// removes trailing whitespace per line, removes leading/trailing blank lines,
    /// collapses consecutive blank lines to single separators, and collapses consecutive
    /// newlines when flattening to single-line (useful for preview text generation).
    /// </remarks>
    public static readonly LineProcessingOptions Default = new()
    {
        LeadingWhitespaceHandling = LeadingWhitespaceHandling.Preserve,
        InnerWhitespaceHandling = InnerWhitespaceHandling.Preserve,
        InnerWhitespaceReplacement = ' ',
        TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
        LeadingBlankLineHandling = LeadingBlankLineHandling.Remove,
        InnerBlankLineHandling = InnerBlankLineHandling.Collapse,
        TrailingBlankLineHandling = TrailingBlankLineHandling.Remove,
        NewLine = Environment.NewLine,
        NewlineHandling = NewlineHandling.CollapseConsecutive,
        NewlineReplacement = ' '
    };

    /// <summary>
    /// Minimal line processing options: aggressive whitespace and blank line removal.
    /// </summary>
    /// <remarks>
    /// This configuration removes all leading/inner/trailing whitespace, removes all blank lines,
    /// and removes all newlines when flattening to single-line. Primarily useful
    /// for testing and scenarios where minimal representation with no whitespace is desired.
    /// </remarks>
    public static readonly LineProcessingOptions Minimal = new()
    {
        LeadingWhitespaceHandling = LeadingWhitespaceHandling.Remove,
        InnerWhitespaceHandling = InnerWhitespaceHandling.Remove,
        InnerWhitespaceReplacement = ' ',
        TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
        LeadingBlankLineHandling = LeadingBlankLineHandling.Remove,
        InnerBlankLineHandling = InnerBlankLineHandling.Remove,
        TrailingBlankLineHandling = TrailingBlankLineHandling.Remove,
        NewLine = Environment.NewLine,
        NewlineHandling = NewlineHandling.Remove,
        NewlineReplacement = ' '
    };
}
