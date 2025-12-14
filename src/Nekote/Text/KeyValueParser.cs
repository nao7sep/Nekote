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

        var result = new Dictionary<string, string>();
        var lines = LineParser.ToLines(text);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            // Skip empty lines and comments
            if (string.IsNullOrEmpty(line) || line.StartsWith('#') || line.StartsWith("//"))
                continue;

            // Find first colon
            int colonIndex = line.IndexOf(':');
            if (colonIndex == -1)
                throw new ArgumentException($"Line {i + 1} is not in valid Key:Value format (missing colon): {line}");

            if (colonIndex == 0)
                throw new ArgumentException($"Line {i + 1} has empty key: {line}");

            // Extract key WITHOUT trimming, so we can validate it doesn't have leading/trailing whitespace
            string key = line.Substring(0, colonIndex);

            // Validate key has no leading/trailing whitespace (security and clarity)
            // This checks the extracted key BEFORE any trimming
            StringValidator.ValidateKey(key);

            string escapedValue = colonIndex + 1 < line.Length ? line.Substring(colonIndex + 1).Trim() : string.Empty;
            string unescapedValue = TextEscaper.Unescape(escapedValue, EscapeMode.KeyValue) ?? string.Empty;

            if (result.ContainsKey(key))
                throw new ArgumentException($"Line {i + 1} has duplicate key: {key}");

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
