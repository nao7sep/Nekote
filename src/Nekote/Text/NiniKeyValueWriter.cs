using System.Text;

namespace Nekote.Text;

/// <summary>
/// Writes dictionaries to NINI format text. Values are automatically escaped.
/// </summary>
public static class NiniKeyValueWriter
{
    /// <summary>
    /// Writes a dictionary to NINI format text. Values are escaped for multi-line content.
    /// </summary>
    /// <param name="data">The dictionary to write.</param>
    /// <param name="sortKeys">If true, keys are sorted alphabetically using case-insensitive ordinal comparison. Default is false.</param>
    /// <param name="newLine">The newline sequence to use. Default is Environment.NewLine.</param>
    /// <returns>NINI format text.</returns>
    /// <exception cref="ArgumentException">Thrown when a key contains invalid characters (':', '\n', '\r') or starts with '#', '//', '[', or '@'.</exception>
    public static string Write(Dictionary<string, string> data, bool sortKeys = false, string? newLine = null)
    {
        if (data == null || data.Count == 0)
            return string.Empty;

        newLine ??= Environment.NewLine;
        var lines = new List<string>(data.Count);
        var keys = sortKeys ? data.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList() : data.Keys.ToList();

        foreach (var key in keys)
        {
            // Validate key using centralized validator
            StringValidator.ValidateNiniKey(key);

            string? value = data[key];

            // Reject null values explicitly - the format cannot distinguish between null and empty string
            // (both would be serialized as "key: "), so we require explicit empty string instead
            if (value == null)
                throw new ArgumentException($"Value for key '{key}' cannot be null. Use string.Empty for empty values.");

            string escapedValue = TextEscaper.Escape(value, EscapeMode.NiniValue)!; // value is non-null here
            lines.Add($"{key}: {escapedValue}");
        }

        return string.Join(newLine, lines);
    }

    /// <summary>
    /// Writes a dictionary to a NINI format file. Values are escaped for multi-line content.
    /// </summary>
    /// <param name="filePath">The path to the file to write.</param>
    /// <param name="data">The dictionary to write.</param>
    /// <param name="sortKeys">If true, keys are sorted alphabetically using case-insensitive ordinal comparison. Default is false.</param>
    /// <param name="newLine">The newline sequence to use. Default is Environment.NewLine.</param>
    /// <param name="encoding">Text encoding (default: UTF-8 without BOM).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public static async Task WriteFileAsync(string filePath, Dictionary<string, string> data, bool sortKeys = false, string? newLine = null, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        string text = Write(data, sortKeys, newLine);
        await File.WriteAllTextAsync(filePath, text, encoding ?? TextEncoding.Utf8NoBom, cancellationToken);
    }
}


