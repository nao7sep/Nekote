using System.Text;

namespace Nekote.Text;

/// <summary>
/// Parses text in Key:Value format into a dictionary. Values are automatically unescaped using KeyValue mode.
/// </summary>
public static class KeyValueParser
{
    /// <summary>
    /// Parses Key:Value format text into a dictionary. Empty lines and lines starting with # or // are ignored.
    /// </summary>
    /// <param name="text">The text to parse in Key:Value format.</param>
    /// <returns>A dictionary of key-value pairs with unescaped values.</returns>
    /// <exception cref="ArgumentException">Thrown when a line is not in valid Key:Value format.</exception>
    public static Dictionary<string, string> Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new Dictionary<string, string>();

        var lines = LineParser.ToLines(text);
        return Parse(lines);
    }

    /// <summary>
    /// Parses Key:Value format lines into a dictionary. Empty lines and lines starting with # or // are ignored.
    /// </summary>
    /// <param name="lines">The lines to parse in Key:Value format.</param>
    /// <returns>A dictionary of key-value pairs with unescaped values.</returns>
    /// <exception cref="ArgumentException">Thrown when a line is not in valid Key:Value format.</exception>
    public static Dictionary<string, string> Parse(IEnumerable<string> lines)
    {
        if (lines == null)
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        int lineNumber = 0;

        foreach (var line in lines)
        {
            lineNumber++;

            // Skip empty/whitespace lines and comments (comments must start at column 0)
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#') || line.StartsWith("//"))
                continue;

            // Find first colon
            int colonIndex = line.IndexOf(':');
            if (colonIndex == -1)
                throw new ArgumentException($"Line {lineNumber} is not in valid Key:Value format (missing colon): {line}");

            if (colonIndex == 0)
                throw new ArgumentException($"Line {lineNumber} has empty key: {line}");

            // Extract key from untrimmed line
            string key = line.Substring(0, colonIndex);

            // Validate key has no leading/trailing whitespace
            // This enforces that keys must start at column 0 with no indentation
            StringValidator.ValidateKeyValueFileKey(key);

            string escapedValue = colonIndex + 1 < line.Length ? line.Substring(colonIndex + 1).Trim() : string.Empty;
            string unescapedValue = TextEscaper.Unescape(escapedValue, EscapeMode.NiniValue) ?? string.Empty;

            if (result.ContainsKey(key))
                throw new ArgumentException($"Line {lineNumber} has duplicate key: {key}");

            result[key] = unescapedValue;
        }

        return result;
    }

    /// <summary>
    /// Parses Key:Value format text from a file into a dictionary.
    /// </summary>
    /// <param name="filePath">The path to the file to parse.</param>
    /// <param name="encoding">Text encoding (default: UTF-8 without BOM).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A dictionary of key-value pairs with unescaped values.</returns>
    public static async Task<Dictionary<string, string>> ParseFileAsync(string filePath, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        string text = await File.ReadAllTextAsync(filePath, encoding ?? TextEncoding.Utf8NoBom, cancellationToken);
        return Parse(text);
    }
}
