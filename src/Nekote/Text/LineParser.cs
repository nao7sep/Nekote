using System.Text;

namespace Nekote.Text;

/// <summary>
/// Provides utilities for converting between text and line arrays, properly handling all line ending conventions
/// (\r\n, \n, \r).
/// </summary>
public static class LineParser
{
    /// <summary>
    /// Splits text into lines, properly handling all line ending conventions (\r\n, \n, \r).
    /// Line breaks are treated as terminators. A trailing line break does not generate an empty final line.
    /// </summary>
    /// <param name="text">The text to split into lines.</param>
    /// <returns>An array of lines. Returns empty array if input is null or empty.</returns>
    public static string[] ToLines(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<string>();

        var result = new List<string>();
        foreach (var line in LineProcessor.EnumerateLines(text.AsSpan()))
        {
            result.Add(line.ToString());
        }
        return result.ToArray();
    }

    /// <summary>
    /// Joins lines into a single text string using the specified line ending.
    /// </summary>
    /// <param name="lines">The lines to join.</param>
    /// <param name="newLine">The line ending to use. Default is Environment.NewLine.</param>
    /// <returns>The joined text. Returns empty string if lines is null or empty.</returns>
    public static string FromLines(string[]? lines, string? newLine = null)
    {
        if (lines == null || lines.Length == 0)
            return string.Empty;

        newLine ??= Environment.NewLine;
        return string.Join(newLine, lines);
    }

    /// <summary>
    /// Joins lines into a single text string using the specified line ending.
    /// </summary>
    /// <param name="lines">The lines to join.</param>
    /// <param name="newLine">The line ending to use. Default is Environment.NewLine.</param>
    /// <returns>The joined text. Returns empty string if lines is null.</returns>
    public static string FromLines(IEnumerable<string>? lines, string? newLine = null)
    {
        if (lines == null)
            return string.Empty;

        newLine ??= Environment.NewLine;
        return string.Join(newLine, lines);
    }

    /// <summary>
    /// Counts the number of lines in text, properly handling all line ending conventions.
    /// Line breaks are treated as terminators. A trailing line break does not increment the count.
    /// </summary>
    /// <param name="text">The text to count lines in.</param>
    /// <returns>The number of lines. Returns 0 if input is null or empty.</returns>
    public static int CountLines(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        return LineProcessor.CountLines(text.AsSpan());
    }

    /// <summary>
    /// Reads text from a file and splits it into lines.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
    /// <param name="encoding">Text encoding (default: UTF-8 without BOM).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An array of lines.</returns>
    public static async Task<string[]> ToLinesFromFileAsync(string filePath, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        string text = await File.ReadAllTextAsync(filePath, encoding ?? TextEncoding.Utf8NoBom, cancellationToken);
        return ToLines(text);
    }

    /// <summary>
    /// Writes lines to a file using the specified line ending.
    /// </summary>
    /// <param name="filePath">The path to the file to write.</param>
    /// <param name="lines">The lines to write.</param>
    /// <param name="newLine">The line ending to use. Default is Environment.NewLine.</param>
    /// <param name="encoding">Text encoding (default: UTF-8 without BOM).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public static async Task FromLinesToFileAsync(string filePath, string[] lines, string? newLine = null, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        string text = FromLines(lines, newLine);
        await File.WriteAllTextAsync(filePath, text, encoding ?? TextEncoding.Utf8NoBom, cancellationToken);
    }

    /// <summary>
    /// Writes lines to a file using the specified line ending.
    /// </summary>
    /// <param name="filePath">The path to the file to write.</param>
    /// <param name="lines">The lines to write.</param>
    /// <param name="newLine">The line ending to use. Default is Environment.NewLine.</param>
    /// <param name="encoding">Text encoding (default: UTF-8 without BOM).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public static async Task FromLinesToFileAsync(string filePath, IEnumerable<string> lines, string? newLine = null, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        string text = FromLines(lines, newLine);
        await File.WriteAllTextAsync(filePath, text, encoding ?? TextEncoding.Utf8NoBom, cancellationToken);
    }
}
