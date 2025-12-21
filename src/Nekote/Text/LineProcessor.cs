using System;
using System.Text;

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

    /// <summary>
    /// Processes a single line of text according to the specified options, handling leading, inner, and trailing whitespace.
    /// </summary>
    /// <param name="line">The line content to process.</param>
    /// <param name="options">The options defining how whitespace should be handled.</param>
    /// <param name="builder">A StringBuilder used for constructing the result when modifications are necessary.</param>
    /// <returns>A span representing the processed line. This may be a slice of the original line or a new string.</returns>
    /// <remarks>
    /// <para>
    /// This method is optimized for the most common scenarios:
    /// <list type="bullet">
    /// <item>Trailing whitespace is checked first as it is significantly more frequently trimmed than indentation.</item>
    /// <item>If <see cref="LeadingWhitespaceHandling"/> and <see cref="InnerWhitespaceHandling"/> are both 
    /// <see cref="LeadingWhitespaceHandling.Preserve"/>, the method returns a slice of the original span, 
    /// avoiding allocations regardless of <see cref="TrailingWhitespaceHandling"/>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// If inner whitespace requires modification (collapse or remove) and the region between visible characters
    /// contains whitespace, the method will use the provided <paramref name="builder"/> and return a new string.
    /// </para>
    /// </remarks>
    public static ReadOnlySpan<char> ProcessLine(ReadOnlySpan<char> line, LineProcessingOptions options, StringBuilder builder)
    {
        if (line.IsEmpty)
            return line;

        // We check trailing first because trailing white spaces are significantly more frequently trimmed than indentation.
        int lastVisible = -1;
        for (int i = line.Length - 1; i >= 0; i--)
        {
            if (!char.IsWhiteSpace(line[i]))
            {
                lastVisible = i;
                break;
            }
        }

        // If no visible characters are found, the line is entirely whitespace.
        if (lastVisible < 0)
        {
            // If we are removing leading OR trailing whitespace, an all-whitespace line becomes empty.
            // Inner handling doesn't apply because there is no "inner" region.
            if (options.LeadingWhitespaceHandling == LeadingWhitespaceHandling.Remove ||
                options.TrailingWhitespaceHandling == TrailingWhitespaceHandling.Remove)
            {
                return ReadOnlySpan<char>.Empty;
            }

            // If preserving both, return the original line.
            return line;
        }

        // Optimization: In most cases, leading handling and inner handling are preserve.
        // If both are preserve, we can always return a slice regardless of trailing handling.
        if (options.LeadingWhitespaceHandling == LeadingWhitespaceHandling.Preserve &&
            options.InnerWhitespaceHandling == InnerWhitespaceHandling.Preserve)
        {
            int end = (options.TrailingWhitespaceHandling == TrailingWhitespaceHandling.Remove) ? lastVisible + 1 : line.Length;
            return line.Slice(0, end);
        }

        // Find the first visible character.
        int firstVisible = -1;
        for (int i = 0; i < line.Length; i++)
        {
            if (!char.IsWhiteSpace(line[i]))
            {
                firstVisible = i;
                break;
            }
        }

        // Fast Path: Preserve Inner Whitespace (Already handled Leading+Inner=Preserve above, 
        // this handles Leading=Remove, Inner=Preserve)
        if (options.InnerWhitespaceHandling == InnerWhitespaceHandling.Preserve)
        {
            int start = (options.LeadingWhitespaceHandling == LeadingWhitespaceHandling.Remove) ? firstVisible : 0;
            int end = (options.TrailingWhitespaceHandling == TrailingWhitespaceHandling.Remove) ? lastVisible + 1 : line.Length;

            return line.Slice(start, end - start);
        }

        // Slow Path: Allocation Required
        // At this point, InnerWhitespaceHandling is either Collapse or Remove, and we know 
        // there is content between firstVisible and lastVisible that may contain whitespace.
        builder.Clear();

        // 1. Handle Leading
        if (options.LeadingWhitespaceHandling == LeadingWhitespaceHandling.Preserve)
        {
            builder.Append(line.Slice(0, firstVisible));
        }

        // 2. Handle Inner (The "Core")
        // Iterate from first visible to last visible.
        if (options.InnerWhitespaceHandling == InnerWhitespaceHandling.Remove)
        {
            for (int i = firstVisible; i <= lastVisible; i++)
            {
                if (!char.IsWhiteSpace(line[i]))
                {
                    builder.Append(line[i]);
                }
            }
        }
        else if (options.InnerWhitespaceHandling == InnerWhitespaceHandling.Collapse)
        {
            bool pendingWhitespace = false;

            // Split logic for char vs string replacement to avoid checks inside the loop.
            if (options.InnerWhitespaceReplacement.IsChar)
            {
                char replacement = options.InnerWhitespaceReplacement.AsChar();
                for (int i = firstVisible; i <= lastVisible; i++)
                {
                    char c = line[i];
                    if (char.IsWhiteSpace(c))
                    {
                        pendingWhitespace = true;
                    }
                    else
                    {
                        if (pendingWhitespace)
                        {
                            builder.Append(replacement);
                            pendingWhitespace = false;
                        }
                        builder.Append(c);
                    }
                }
            }
            else
            {
                string replacement = options.InnerWhitespaceReplacement.AsString();
                for (int i = firstVisible; i <= lastVisible; i++)
                {
                    char c = line[i];
                    if (char.IsWhiteSpace(c))
                    {
                        pendingWhitespace = true;
                    }
                    else
                    {
                        if (pendingWhitespace)
                        {
                            builder.Append(replacement);
                            pendingWhitespace = false;
                        }
                        builder.Append(c);
                    }
                }
            }
        }

        // 3. Handle Trailing
        if (options.TrailingWhitespaceHandling == TrailingWhitespaceHandling.Preserve)
        {
            builder.Append(line.Slice(lastVisible + 1));
        }

        return builder.ToString();
    }
}
