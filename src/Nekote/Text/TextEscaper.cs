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
    /// <param name="text">The text to escape. Null values are treated as empty strings.</param>
    /// <param name="mode">The escaping strategy to use.</param>
    /// <returns>The escaped text.</returns>
    /// <exception cref="NotImplementedException">Thrown when the specified mode is not yet implemented.</exception>
    public static string Escape(string? text, EscapeMode mode)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return mode switch
        {
            EscapeMode.KeyValue => EscapeKeyValue(text),
            EscapeMode.Csv => EscapeCsv(text),
            EscapeMode.Url => EscapeUrl(text),
            EscapeMode.Html => EscapeHtml(text),
            _ => throw new NotImplementedException($"Escape mode '{mode}' is not yet implemented.")
        };
    }

    /// <summary>
    /// Unescapes text according to the specified mode.
    /// </summary>
    /// <param name="escapedText">The escaped text to decode. Null values are treated as empty strings.</param>
    /// <param name="mode">The escaping strategy that was used.</param>
    /// <returns>The unescaped original text.</returns>
    /// <exception cref="NotImplementedException">Thrown when the specified mode is not yet implemented.</exception>
    public static string Unescape(string? escapedText, EscapeMode mode)
    {
        if (string.IsNullOrEmpty(escapedText))
            return string.Empty;

        return mode switch
        {
            EscapeMode.KeyValue => UnescapeKeyValue(escapedText),
            EscapeMode.Csv => UnescapeCsv(escapedText),
            EscapeMode.Url => UnescapeUrl(escapedText),
            EscapeMode.Html => UnescapeHtml(escapedText),
            _ => throw new NotImplementedException($"Unescape mode '{mode}' is not yet implemented.")
        };
    }

    /// <summary>
    /// Escapes text for key-value file format. Line breaks become literal \n,
    /// backslashes become \\, carriage returns become \r, and tabs become \t.
    /// </summary>
    private static string EscapeKeyValue(string text)
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

    /// <summary>
    /// Unescapes text from key-value file format. Literal \n becomes line breaks,
    /// \\ becomes backslash, \r becomes carriage return, and \t becomes tab.
    /// </summary>
    private static string UnescapeKeyValue(string escapedText)
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

    /// <summary>
    /// Escapes text for CSV format following RFC 4180. Values containing commas,
    /// double quotes, or line breaks are wrapped in double quotes with internal
    /// quotes doubled.
    /// </summary>
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

    /// <summary>
    /// Unescapes text from CSV format. Removes surrounding quotes and converts
    /// doubled quotes back to single quotes.
    /// </summary>
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

    /// <summary>
    /// Escapes text for URL query strings using percent encoding.
    /// Unreserved characters (A-Z, a-z, 0-9, -, _, ., ~) are not encoded.
    /// All other characters become %XX hex values.
    /// </summary>
    private static string EscapeUrl(string text)
    {
        var result = new StringBuilder(text.Length + 16);

        foreach (char c in text)
        {
            if (IsUnreservedUrlChar(c))
            {
                result.Append(c);
            }
            else
            {
                // Convert to UTF-8 bytes and percent-encode each byte
                byte[] bytes = Encoding.UTF8.GetBytes(new[] { c });
                foreach (byte b in bytes)
                {
                    result.Append('%');
                    result.Append(b.ToString("X2"));
                }
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Unescapes text from URL percent encoding. Converts %XX sequences back
    /// to their original characters.
    /// </summary>
    private static string UnescapeUrl(string escapedText)
    {
        var bytes = new List<byte>();
        var i = 0;

        while (i < escapedText.Length)
        {
            if (escapedText[i] == '%' && i + 2 < escapedText.Length)
            {
                string hex = escapedText.Substring(i + 1, 2);
                if (byte.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out byte b))
                {
                    bytes.Add(b);
                    i += 3;
                }
                else
                {
                    // Invalid hex sequence, treat % as literal
                    bytes.Add((byte)escapedText[i]);
                    i++;
                }
            }
            else
            {
                bytes.Add((byte)escapedText[i]);
                i++;
            }
        }

        return Encoding.UTF8.GetString(bytes.ToArray());
    }

    /// <summary>
    /// Checks if a character is unreserved in URL encoding (RFC 3986).
    /// </summary>
    private static bool IsUnreservedUrlChar(char c)
    {
        return (c >= 'A' && c <= 'Z') ||
               (c >= 'a' && c <= 'z') ||
               (c >= '0' && c <= '9') ||
               c == '-' || c == '_' || c == '.' || c == '~';
    }

    /// <summary>
    /// Escapes text for HTML by converting special characters to HTML entities.
    /// </summary>
    private static string EscapeHtml(string text)
    {
        var result = new StringBuilder(text.Length + 16);

        foreach (char c in text)
        {
            switch (c)
            {
                case '&':
                    result.Append("&amp;");
                    break;
                case '<':
                    result.Append("&lt;");
                    break;
                case '>':
                    result.Append("&gt;");
                    break;
                case '"':
                    result.Append("&quot;");
                    break;
                case '\'':
                    result.Append("&#39;");
                    break;
                default:
                    result.Append(c);
                    break;
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Unescapes text from HTML entities back to original characters.
    /// </summary>
    private static string UnescapeHtml(string escapedText)
    {
        var result = new StringBuilder(escapedText.Length);
        var i = 0;

        while (i < escapedText.Length)
        {
            if (escapedText[i] == '&')
            {
                // Find the ending semicolon
                int semicolonPos = escapedText.IndexOf(';', i);
                if (semicolonPos > i && semicolonPos - i <= 10) // Reasonable entity length
                {
                    string entity = escapedText.Substring(i, semicolonPos - i + 1);
                    string? replacement = entity switch
                    {
                        "&amp;" => "&",
                        "&lt;" => "<",
                        "&gt;" => ">",
                        "&quot;" => "\"",
                        "&#39;" => "'",
                        _ => null
                    };

                    if (replacement != null)
                    {
                        result.Append(replacement);
                        i = semicolonPos + 1;
                        continue;
                    }
                }
            }

            result.Append(escapedText[i]);
            i++;
        }

        return result.ToString();
    }
}
