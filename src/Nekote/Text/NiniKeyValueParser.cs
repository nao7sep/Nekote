using System.Text;

namespace Nekote.Text;

/// <summary>
/// Parses NINI format text into a dictionary. Values are automatically unescaped.
/// </summary>
public static class NiniKeyValueParser
{
    /// <summary>
    /// Parses NINI format text into a dictionary. Empty lines and lines starting with #, //, or ; are ignored.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="options">Configuration options. If null, uses <see cref="NiniOptions.Default"/>.</param>
    /// <returns>A dictionary of key-value pairs with unescaped values.</returns>
    /// <exception cref="ArgumentException">Thrown when a line is not in valid key:value format.</exception>
    public static Dictionary<string, string> Parse(string text, NiniOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new Dictionary<string, string>();

        var lines = LineParser.ToLines(text);
        return Parse(lines, options);
    }

    /// <summary>
    /// Parses NINI format lines into a dictionary. Empty lines and lines starting with #, //, or ; are ignored.
    /// </summary>
    /// <param name="lines">The lines to parse.</param>
    /// <param name="options">Configuration options. If null, uses <see cref="NiniOptions.Default"/>.</param>
    /// <returns>A dictionary of key-value pairs with unescaped values.</returns>
    /// <exception cref="ArgumentException">Thrown when a line is not in valid key:value format.</exception>
    public static Dictionary<string, string> Parse(IEnumerable<string> lines, NiniOptions? options = null)
    {
        options ??= NiniOptions.Default;

        if (lines == null)
            return new Dictionary<string, string>(options.KeyComparer);

        var result = new Dictionary<string, string>(options.KeyComparer);
        int lineNumber = 0;

        foreach (var line in lines)
        {
            lineNumber++;

            // Skip empty/whitespace lines and comments (comments must start at column 0)
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#') || line.StartsWith("//") || line.StartsWith(';'))
                continue;

            // Find first separator
            int separatorIndex = line.IndexOf(options.SeparatorChar);
            if (separatorIndex == -1)
                throw new ArgumentException($"Line {lineNumber} is not in valid key{options.SeparatorChar}value format (missing separator): {line}");

            if (separatorIndex == 0)
                throw new ArgumentException($"Line {lineNumber} has empty key: {line}");

            // Extract key from untrimmed line
            string key = line.Substring(0, separatorIndex);

            // Validate key has no leading/trailing whitespace
            // This enforces that keys must start at column 0 with no indentation
            StringValidator.ValidateNiniKey(key, options);

            string escapedValue = separatorIndex + 1 < line.Length ? line.Substring(separatorIndex + 1).Trim() : string.Empty;
            string unescapedValue = TextEscaper.Unescape(escapedValue, EscapeMode.NiniValue) ?? string.Empty;

            if (result.ContainsKey(key))
                throw new ArgumentException($"Line {lineNumber} has duplicate key: {key}");

            result[key] = unescapedValue;
        }

        return result;
    }

    /// <summary>
    /// Parses a NINI format file into a dictionary.
    /// </summary>
    /// <param name="filePath">The path to the file to parse.</param>
    /// <param name="options">Configuration options. If null, uses <see cref="NiniOptions.Default"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A dictionary of key-value pairs with unescaped values.</returns>
    public static async Task<Dictionary<string, string>> ParseFileAsync(string filePath, NiniOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= NiniOptions.Default;
        string text = await File.ReadAllTextAsync(filePath, options.Encoding, cancellationToken);
        return Parse(text, options);
    }
}
