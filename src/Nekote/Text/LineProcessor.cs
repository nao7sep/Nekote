using System;

namespace Nekote.Text;

/// <summary>
/// Processes text lines according to configured options.
/// </summary>
public class LineProcessor
{
    /// <summary>
    /// Tries to read the next line from the given text span.
    /// </summary>
    /// <param name="text">The text to read from.</param>
    /// <param name="line">When this method returns, contains the line of text excluding the line break.</param>
    /// <param name="remaining">When this method returns, contains the remaining text after the line and its line break.</param>
    /// <returns><c>true</c> if a line was read; <c>false</c> if the input span was empty.</returns>
    public static bool TryReadLine(ReadOnlySpan<char> text, out ReadOnlySpan<char> line, out ReadOnlySpan<char> remaining)
    {
        if (text.IsEmpty)
        {
            line = default;
            remaining = default;
            return false;
        }

        int index = text.IndexOfAny('\r', '\n');

        if (index < 0)
        {
            // No more line breaks, return the rest of the text
            line = text;
            remaining = default; // Empty span
            return true;
        }

        line = text.Slice(0, index);

        // Handle CRLF (\r\n) vs LF (\n) vs CR (\r)
        if (text[index] == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
        {
            remaining = text.Slice(index + 2);
        }
        else
        {
            remaining = text.Slice(index + 1);
        }

        return true;
    }
}
