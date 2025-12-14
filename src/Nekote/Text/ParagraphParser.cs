using System.Text;

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
    /// <param name="newLine">The newline sequence to use when joining lines within paragraphs. Default is Environment.NewLine.</param>
    /// <returns>An array of paragraphs. Returns empty array if input is null or whitespace.</returns>
    public static string[] Parse(string text, string? newLine = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<string>();

        newLine ??= Environment.NewLine;
        var lines = LineParser.ToLines(text);
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
                    paragraphs.Add(paragraph);
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
            paragraphs.Add(paragraph);
        }

        return paragraphs.ToArray();
    }

    /// <summary>
    /// Splits text from a file into paragraphs by blank lines.
    /// </summary>
    /// <param name="filePath">The path to the file to parse.</param>
    /// <param name="newLine">The newline sequence to use when joining lines within paragraphs. Default is Environment.NewLine.</param>
    /// <param name="encoding">Text encoding (default: UTF-8 without BOM).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An array of paragraphs.</returns>
    public static async Task<string[]> ParseFileAsync(string filePath, string? newLine = null, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        string text = await File.ReadAllTextAsync(filePath, encoding ?? TextEncoding.Utf8NoBom, cancellationToken);
        return Parse(text, newLine);
    }
}
