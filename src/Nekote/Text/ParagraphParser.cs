namespace Nekote.Text;

/// <summary>
/// Parses text into paragraphs by splitting on blank lines (lines containing only whitespace or completely empty).
/// </summary>
public static class ParagraphParser
{
    /// <summary>
    /// Splits text into paragraphs by blank lines. Consecutive blank lines are treated as a single separator.
    /// Leading and trailing blank lines are ignored.
    /// </summary>
    /// <param name="text">The text to split into paragraphs.</param>
    /// <param name="trimParagraphs">If true, each paragraph is trimmed of leading/trailing whitespace. Default is true.</param>
    /// <param name="newLine">The newline sequence to use when joining lines within paragraphs. Default is Environment.NewLine.</param>
    /// <returns>An array of paragraphs. Returns empty array if input is null or whitespace.</returns>
    public static string[] Parse(string text, bool trimParagraphs = true, string? newLine = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<string>();

        newLine ??= Environment.NewLine;
        
        // Normalize CRLF to LF to ensure consistent splitting
        // This prevents \r\n from being split into "line\r", "", "line" where the empty string triggers a paragraph break
        var normalizedText = text.Replace("\r\n", "\n");
        var lines = normalizedText.Split('\n');
        
        var paragraphs = new List<string>();
        var currentParagraph = new List<string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                // Blank line - end current paragraph if it has content
                if (currentParagraph.Count > 0)
                {
                    var paragraph = string.Join(newLine, currentParagraph);
                    paragraphs.Add(trimParagraphs ? paragraph.Trim() : paragraph);
                    currentParagraph.Clear();
                }
            }
            else
            {
                // Non-blank line - add to current paragraph
                currentParagraph.Add(line);
            }
        }

        // Add final paragraph if it has content
        if (currentParagraph.Count > 0)
        {
            var paragraph = string.Join(newLine, currentParagraph);
            paragraphs.Add(trimParagraphs ? paragraph.Trim() : paragraph);
        }

        return paragraphs.ToArray();
    }

    /// <summary>
    /// Splits text from a file into paragraphs by blank lines.
    /// </summary>
    /// <param name="filePath">The path to the file to parse.</param>
    /// <param name="trimParagraphs">If true, each paragraph is trimmed of leading/trailing whitespace. Default is true.</param>
    /// <param name="newLine">The newline sequence to use when joining lines within paragraphs. Default is Environment.NewLine.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An array of paragraphs.</returns>
    public static async Task<string[]> ParseFileAsync(string filePath, bool trimParagraphs = true, string? newLine = null, CancellationToken cancellationToken = default)
    {
        string text = await File.ReadAllTextAsync(filePath, cancellationToken);
        return Parse(text, trimParagraphs, newLine);
    }
}
