using System.Net;
using System.Text;

namespace Nekote.Text;

/// <summary>
/// Provides static methods for escaping and unescaping text in various formats.
/// </summary>
public static class TextEscaper
{
    /// <summary>
    /// Escapes text according to the specified mode.
    /// </summary>
    /// <param name="text">The text to escape. Null input returns null.</param>
    /// <param name="mode">The escaping strategy to use.</param>
    /// <returns>The escaped text, or null if input is null.</returns>
    /// <exception cref="NotImplementedException">Thrown when the specified mode is not yet implemented.</exception>
    public static string? Escape(string? text, EscapeMode mode)
    {
        if (text == null)
            return null;

        if (text.Length == 0)
            return string.Empty;

        return mode switch
        {
            EscapeMode.NiniValue => EscapeNiniValue(text),
            EscapeMode.Csv => EscapeCsv(text),
            EscapeMode.Url => EscapeUrl(text),
            EscapeMode.Html => EscapeHtml(text),
            _ => throw new NotImplementedException($"Escape mode '{mode}' is not yet implemented.")
        };
    }

    /// <summary>
    /// Unescapes text according to the specified mode.
    /// </summary>
    /// <param name="escapedText">The escaped text to decode. Null input returns null.</param>
    /// <param name="mode">The escaping strategy that was used.</param>
    /// <returns>The unescaped original text, or null if input is null.</returns>
    /// <exception cref="NotImplementedException">Thrown when the specified mode is not yet implemented.</exception>
    public static string? Unescape(string? escapedText, EscapeMode mode)
    {
        if (escapedText == null)
            return null;

        if (escapedText.Length == 0)
            return string.Empty;

        return mode switch
        {
            EscapeMode.NiniValue => UnescapeNiniValue(escapedText),
            EscapeMode.Csv => UnescapeCsv(escapedText),
            EscapeMode.Url => UnescapeUrl(escapedText),
            EscapeMode.Html => UnescapeHtml(escapedText),
            _ => throw new NotImplementedException($"Unescape mode '{mode}' is not yet implemented.")
        };
    }

    private static string EscapeNiniValue(string text)
    {
        var result = new StringBuilder(text.Length + 16);

        foreach (char c in text)
        {
            switch (c)
            {
                case '\\':
                    result.Append("\\\\");
                    break;
                case '\n':
                    result.Append("\\n");
                    break;
                case '\r':
                    result.Append("\\r");
                    break;
                case '\t':
                    result.Append("\\t");
                    break;
                default:
                    result.Append(c);
                    break;
            }
        }

        return result.ToString();
    }

    private static string UnescapeNiniValue(string escapedText)
    {
        var result = new StringBuilder(escapedText.Length);
        var i = 0;

        while (i < escapedText.Length)
        {
            if (escapedText[i] == '\\' && i + 1 < escapedText.Length)
            {
                char nextChar = escapedText[i + 1];
                switch (nextChar)
                {
                    case '\\':
                        result.Append('\\');
                        i += 2;
                        break;
                    case 'n':
                        result.Append('\n');
                        i += 2;
                        break;
                    case 'r':
                        result.Append('\r');
                        i += 2;
                        break;
                    case 't':
                        result.Append('\t');
                        i += 2;
                        break;
                    default:
                        // Unknown escape sequence - keep the backslash literal
                        result.Append('\\');
                        i++;
                        break;
                }
            }
            else
            {
                result.Append(escapedText[i]);
                i++;
            }
        }

        return result.ToString();
    }

    private static string EscapeCsv(string text)
    {
        // Check if escaping is needed
        bool needsQuotes = text.Contains(',') || text.Contains('"') ||
                          text.Contains('\n') || text.Contains('\r');

        if (!needsQuotes)
            return text;

        var result = new StringBuilder(text.Length + 16);
        result.Append('"');

        foreach (char c in text)
        {
            if (c == '"')
                result.Append("\"\""); // Double the quote
            else
                result.Append(c);
        }

        result.Append('"');
        return result.ToString();
    }

    private static string UnescapeCsv(string escapedText)
    {
        // Check if text is quoted
        if (escapedText.Length < 2 || escapedText[0] != '"' || escapedText[^1] != '"')
            return escapedText;

        var result = new StringBuilder(escapedText.Length);
        var i = 1; // Skip opening quote
        var end = escapedText.Length - 1; // Skip closing quote

        while (i < end)
        {
            if (escapedText[i] == '"' && i + 1 < end && escapedText[i + 1] == '"')
            {
                result.Append('"');
                i += 2;
            }
            else
            {
                result.Append(escapedText[i]);
                i++;
            }
        }

        return result.ToString();
    }

    private static string EscapeUrl(string text)
    {
        // Built-in Uri.EscapeDataString is highly optimized and handles RFC 3986 correctly,
        // including correct UTF-8 encoding of surrogate pairs (emojis).
        return Uri.EscapeDataString(text);
    }

    private static string UnescapeUrl(string escapedText)
    {
        // Built-in Uri.UnescapeDataString is more robust than manual parsing,
        // correctly reconstructing Unicode characters from multiple percent-encoded bytes.
        return Uri.UnescapeDataString(escapedText);
    }

    private static string EscapeHtml(string text)
    {
        // WebUtility.HtmlEncode is the standard .NET way to safely encode HTML content.
        return WebUtility.HtmlEncode(text);
    }

    private static string UnescapeHtml(string escapedText)
    {
        // WebUtility.HtmlDecode handles the massive HTML5 entity set and complex 
        // numeric sequences (&#123;, &#xABC;) that are difficult to parse manually.
        return WebUtility.HtmlDecode(escapedText);
    }
}

