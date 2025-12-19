namespace Nekote.Text;

/// <summary>
/// Defines how whitespace within line content is handled.
/// </summary>
public enum InlineWhitespaceMode
{
    /// <summary>
    /// Keep all whitespace as-is.
    /// </summary>
    Preserve,

    /// <summary>
    /// Collapse consecutive whitespace characters to a single ASCII space (U+0020).
    /// </summary>
    /// <remarks>
    /// This is equivalent to the regex pattern \s+ being replaced with a single space.
    /// Implementation uses <see cref="char.IsWhiteSpace"/> to detect any whitespace character
    /// (spaces, tabs, line breaks, etc.) and replaces sequences with one ASCII space.
    /// Even a single whitespace character will be normalized to ASCII space.
    /// <example>"hello    world" → "hello world"</example>
    /// <example>"hello\t\tworld" → "hello world"</example>
    /// </remarks>
    Collapse,

    /// <summary>
    /// Remove all whitespace characters from line content.
    /// </summary>
    /// <remarks>
    /// This strips all whitespace detected by <see cref="char.IsWhiteSpace"/>, including
    /// ASCII spaces, tabs, and Unicode whitespace characters like fullwidth spaces (U+3000).
    /// <para>
    /// <strong>Use case:</strong> In Japanese text, fullwidth and halfwidth spaces are sometimes
    /// used purely for visual spacing in titles or headers (e.g., "日　本　語" with fullwidth
    /// spaces U+3000 between characters). Stripping these produces the actual content without
    /// decorative spacing: "日本語". This is rarely needed in Western languages where removing
    /// spaces would break word boundaries.
    /// </para>
    /// <example>"hello    world" → "helloworld"</example>
    /// <example>"日　本　語" → "日本語" (removes fullwidth spaces U+3000)</example>
    /// </remarks>
    Strip
}
