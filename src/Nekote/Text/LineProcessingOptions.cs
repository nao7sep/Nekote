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
    /// Gets the line separator to use when joining lines back into a single text.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Common values include <c>"\n"</c> (Unix), <c>"\r\n"</c> (Windows), or <c>Environment.NewLine</c> (platform-specific).
    /// This property is required.
    /// </para>
    /// <para>
    /// This property is used by <see cref="LineProcessor.Process"/> to reconstruct multi-line text.
    /// </para>
    /// </remarks>
    public required CharOrString NewLine { get; init; }

    /// <summary>
    /// Default line processing options: preserve line whitespace structure, normalize blank lines.
    /// </summary>
    /// <remarks>
    /// This configuration is suitable for most text processing scenarios:
    /// removes trailing whitespace per line, removes leading/trailing blank lines,
    /// and collapses consecutive blank lines to single separators.
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
        NewLine = Environment.NewLine
    };

    /// <remarks>
    /// <para>
    /// This configuration removes all leading/trailing/inner whitespace, removes all blank lines,
    /// and sets the line separator to a single space.
    /// </para>
    /// <para>
    /// This is designed to be used with <see cref="LineProcessor.ToSingleLine(ReadOnlySpan{char}, System.Text.StringBuilder?, System.Text.StringBuilder?)"/>
    /// to produce a clean, single-line representation of the input text where logical breaks are replaced by spaces.
    /// </para>
    /// </remarks>
    public static readonly LineProcessingOptions SingleLine = new()
    {
        LeadingWhitespaceHandling = LeadingWhitespaceHandling.Remove,
        InnerWhitespaceHandling = InnerWhitespaceHandling.Collapse,
        InnerWhitespaceReplacement = ' ',
        TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
        LeadingBlankLineHandling = LeadingBlankLineHandling.Remove,
        InnerBlankLineHandling = InnerBlankLineHandling.Remove,
        TrailingBlankLineHandling = TrailingBlankLineHandling.Remove,
        NewLine = ' '
    };

    /// <summary>
    /// Minimal line processing options: aggressive whitespace and blank line removal.
    /// </summary>
    /// <remarks>
    /// This configuration removes all leading/inner/trailing whitespace and removes all blank lines.
    /// It uses an empty string for the line separator, effectively joining all content directly.
    /// Primarily useful for testing and scenarios where absolute minimal representation is desired.
    /// </remarks>
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
