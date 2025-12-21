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
    /// Returns an enumerator that iterates through the lines in the text.
    /// </summary>
    /// <param name="text">The text to enumerate lines from.</param>
    /// <returns>A <see cref="LineEnumerator"/> for the specified text.</returns>
    /// <remarks>
    /// This method treats line breaks as terminators, not separators. 
    /// A line break followed by the end of the text does not start a new empty line.
    /// For example, "A\n" yields one line ("A"), while "A\n\n" yields two lines ("A", "").
    /// </remarks>
    public static LineEnumerator EnumerateLines(ReadOnlySpan<char> text)
    {
        return new LineEnumerator(text);
    }

    /// <summary>
    /// Counts the number of lines in the specified text span.
    /// </summary>
    /// <param name="text">The text to count lines in.</param>
    /// <returns>The number of lines in the text. Returns 0 if the text is empty.</returns>
    /// <remarks>
    /// This method properly handles all standard line ending conventions (\r\n, \n, \r).
    /// It matches the behavior of <see cref="EnumerateLines"/>, treating line breaks as terminators.
    /// For example, "A\n" counts as 1 line, while "A\nB" counts as 2 lines.
    /// </remarks>
    public static int CountLines(ReadOnlySpan<char> text)
    {
        int count = 0;
        while (TryReadLine(text, out _, out text))
        {
            count++;
        }
        return count;
    }

    /// <summary>
    /// Determines whether the specified line is empty or consists only of whitespace characters.
    /// </summary>
    /// <param name="line">The line to check.</param>
    /// <returns><c>true</c> if the line is blank; otherwise, <c>false</c>.</returns>
    public static bool IsBlank(ReadOnlySpan<char> line)
    {
        for (int i = 0; i < line.Length; i++)
        {
            if (!char.IsWhiteSpace(line[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the leading whitespace at the beginning of the line.
    /// </summary>
    /// <param name="line">The line to inspect.</param>
    /// <returns>A span containing the leading whitespace characters. If the line consists entirely of whitespace, the entire line is returned.</returns>
    public static ReadOnlySpan<char> GetLeadingWhitespace(ReadOnlySpan<char> line)
    {
        int i = 0;
        while (i < line.Length && char.IsWhiteSpace(line[i]))
        {
            i++;
        }

        return line.Slice(0, i);
    }

    /// <summary>
    /// Gets the trailing whitespace at the end of the line.
    /// </summary>
    /// <param name="line">The line to inspect.</param>
    /// <returns>A span containing the trailing whitespace characters. If the line consists entirely of whitespace, the entire line is returned.</returns>
    public static ReadOnlySpan<char> GetTrailingWhitespace(ReadOnlySpan<char> line)
    {
        int i = line.Length - 1;
        while (i >= 0 && char.IsWhiteSpace(line[i]))
        {
            i--;
        }

        return line.Slice(i + 1);
    }

    /// <summary>
    /// Returns an enumerator that iterates through and processes lines in the text according to the specified options.
    /// </summary>
    /// <param name="text">The text to enumerate lines from.</param>
    /// <param name="options">The options defining how whitespace and blank lines should be handled. If <c>null</c>, <see cref="LineProcessingOptions.Default"/> is used.</param>
    /// <param name="builder">Optional StringBuilder buffer. If null, a new one is created internally.</param>
    /// <returns>A <see cref="ProcessedLineEnumerator"/> for the specified text and options.</returns>
    public static ProcessedLineEnumerator EnumerateProcessedLines(ReadOnlySpan<char> text, LineProcessingOptions? options = null, StringBuilder? builder = null)
    {
        return new ProcessedLineEnumerator(text, options, builder);
    }

    /// <summary>
    /// Processes the text according to the specified options and returns the resulting string.
    /// </summary>
    /// <param name="text">The text to process.</param>
    /// <param name="options">The options defining how lines, whitespace, and blank lines should be handled. If <c>null</c>, <see cref="LineProcessingOptions.Default"/> is used.</param>
    /// <param name="resultBuilder">Optional StringBuilder to use as the accumulator for the final result. If <c>null</c>, a new one is created. The processed text is appended to this builder.</param>
    /// <param name="lineBuffer">Optional StringBuilder to use as a temporary buffer for line processing. If <c>null</c>, a new one is created.</param>
    /// <returns>The processed text string (the content of the result builder).</returns>
    /// <remarks>
    /// This method orchestrates the entire processing pipeline: splitting the text into sections (leading, content, trailing),
    /// iterating through each line to apply whitespace and blank line policies, and finally joining the processed lines
    /// using the configured <see cref="LineProcessingOptions.NewLine"/> separator.
    /// </remarks>
    public static string Process(ReadOnlySpan<char> text, LineProcessingOptions? options = null,
        StringBuilder? resultBuilder = null, StringBuilder? lineBuffer = null)
    {
        if (text.IsEmpty)
        {
            return string.Empty;
        }

        options ??= LineProcessingOptions.Default;
        CharOrString separator = options.NewLine;

        // Use provided result builder or create a new one.
        resultBuilder ??= new StringBuilder(text.Length);

        // Use provided line buffer or create a new one.
        lineBuffer ??= new StringBuilder();

        var enumerator = EnumerateProcessedLines(text, options, lineBuffer);
        bool first = true;

        if (separator.IsChar)
        {
            char sep = separator.AsChar();
            foreach (var line in enumerator)
            {
                if (!first)
                {
                    resultBuilder.Append(sep);
                }

                resultBuilder.Append(line);
                first = false;
            }
        }
        else
        {
            ReadOnlySpan<char> sep = separator.AsSpan();
            foreach (var line in enumerator)
            {
                if (!first)
                {
                    resultBuilder.Append(sep);
                }

                resultBuilder.Append(line);
                first = false;
            }
        }

        return resultBuilder.ToString();
    }

    /// <summary>
    /// Processes the text into a single line using <see cref="LineProcessingOptions.SingleLine"/>.
    /// </summary>
    /// <param name="text">The text to process.</param>
    /// <param name="resultBuilder">Optional StringBuilder to use as the accumulator for the final result. If <c>null</c>, a new one is created. The processed text is appended to this builder.</param>
    /// <param name="lineBuffer">Optional StringBuilder to use as a temporary buffer for line processing. If <c>null</c>, a new one is created.</param>
    /// <returns>The processed text string (the content of the result builder).</returns>
    /// <remarks>
    /// This method is a convenience wrapper for <see cref="Process"/> using the <see cref="LineProcessingOptions.SingleLine"/> preset.
    /// It effectively flattens the text by removing all blank lines and whitespace, and joining content with a single space.
    /// For custom separators or processing rules, use <see cref="Process"/> directly.
    /// </remarks>
    public static string ToSingleLine(ReadOnlySpan<char> text, StringBuilder? resultBuilder = null, StringBuilder? lineBuffer = null)
    {
        return Process(text, LineProcessingOptions.SingleLine, resultBuilder, lineBuffer);
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
    /// This is a low-level method. For high-level processing, use <see cref="Process"/> or <see cref="EnumerateProcessedLines"/>.
    /// As a low-level method, it requires explicit options and does not provide convenience defaulting to <see cref="LineProcessingOptions.Default"/>.
    /// </para>
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
    /// contains whitespace, the method will use the provided <paramref name="builder"/> to construct a new string
    /// and return a span pointing to that string. The returned span remains valid as long as the string is referenced.
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

    /// <summary>
    /// Splits the text into three parts: leading blank lines, the main content (from the first visible line to the last visible line), and trailing blank lines.
    /// </summary>
    /// <param name="text">The text to split.</param>
    /// <param name="leadingBlankLines">When this method returns, contains the span of leading blank lines. If no visible content is found, contains the entire text.</param>
    /// <param name="content">When this method returns, contains the main content span. Empty if no visible content is found.</param>
    /// <param name="trailingBlankLines">When this method returns, contains the span of trailing blank lines.</param>
    /// <returns><c>true</c> if at least one visible character is found; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// <para>
    /// If the text is empty, all output spans are empty, and the method returns <c>false</c>.
    /// </para>
    /// <para>
    /// If the text contains only whitespace, it is considered to have no visible content. In this case,
    /// <paramref name="leadingBlankLines"/> will contain the entire text, and the other outputs will be empty.
    /// This fallback behavior ensures that "lines before visible content" (leading) captures the structure
    /// even when "lines after visible content" (trailing) cannot exist.
    /// </para>
    /// </remarks>
    public static bool SplitIntoSections(ReadOnlySpan<char> text, out ReadOnlySpan<char> leadingBlankLines, out ReadOnlySpan<char> content, out ReadOnlySpan<char> trailingBlankLines)
    {
        if (text.IsEmpty)
        {
            leadingBlankLines = ReadOnlySpan<char>.Empty;
            content = ReadOnlySpan<char>.Empty;
            trailingBlankLines = ReadOnlySpan<char>.Empty;
            return false;
        }

        // 1. Find the first visible character
        int firstVisibleIndex = -1;
        for (int i = 0; i < text.Length; i++)
        {
            if (!char.IsWhiteSpace(text[i]))
            {
                firstVisibleIndex = i;
                break;
            }
        }

        // If no visible char found, the whole text is "leading blank lines"
        if (firstVisibleIndex < 0)
        {
            leadingBlankLines = text;
            content = ReadOnlySpan<char>.Empty;
            trailingBlankLines = ReadOnlySpan<char>.Empty;
            return false;
        }

        // 2. Calculate Leading Cut
        // Find the last newline character before the first visible character.
        // The leading section ends after that newline.
        // Edge case: if firstVisibleIndex is 0 (visible content starts immediately),
        // lastNewlineBefore will be -1, correctly leaving leadingEnd at 0 (no leading blank lines).
        int leadingEnd = 0;
        int lastNewlineBefore = text.Slice(0, firstVisibleIndex).LastIndexOfAny('\r', '\n');

        if (lastNewlineBefore >= 0)
        {
            leadingEnd = lastNewlineBefore + 1;
        }

        leadingBlankLines = text.Slice(0, leadingEnd);

        // 3. Find the last visible character
        int lastVisibleIndex = -1;
        for (int i = text.Length - 1; i >= firstVisibleIndex; i--)
        {
            if (!char.IsWhiteSpace(text[i]))
            {
                lastVisibleIndex = i;
                break;
            }
        }

        // 4. Calculate Trailing Cut
        // Find the first newline character after the last visible character.
        // The content section ends after that newline (including it).
        int contentEnd = text.Length;

        // We look for the first newline starting AFTER the last visible character.
        int newlineIndex = text.Slice(lastVisibleIndex + 1).IndexOfAny('\r', '\n');

        if (newlineIndex >= 0)
        {
            // Found a newline. Need to include it in the content.
            int absoluteNewlineIndex = lastVisibleIndex + 1 + newlineIndex;

            // Check for CRLF (\r\n)
            if (text[absoluteNewlineIndex] == '\r' &&
                absoluteNewlineIndex + 1 < text.Length &&
                text[absoluteNewlineIndex + 1] == '\n')
            {
                contentEnd = absoluteNewlineIndex + 2;
            }
            else
            {
                contentEnd = absoluteNewlineIndex + 1;
            }
        }

        content = text.Slice(leadingEnd, contentEnd - leadingEnd);
        trailingBlankLines = text.Slice(contentEnd);

        return true;
    }
}