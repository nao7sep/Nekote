using System.Text;

namespace Nekote.Text;

/// <summary>
/// Writes dictionaries to Key:Value format text. Values are automatically escaped using KeyValue mode.
/// </summary>
public static class KeyValueWriter
{
    /// <summary>
    /// Writes a dictionary to Key:Value format text. Values are escaped for multi-line content.
    /// </summary>
    /// <param name="data">The dictionary to write.</param>
    /// <param name="sortKeys">If true, keys are sorted alphabetically. Default is false.</param>
    /// <returns>Key:Value format text.</returns>
    /// <exception cref="ArgumentException">Thrown when a key contains invalid characters (':', '\n', '\r', '[', ']', '@') or starts with '#' or '//'.</exception>
    public static string Write(Dictionary<string, string> data, bool sortKeys = false)
    {
        if (data == null || data.Count == 0)
            return string.Empty;

        var result = new StringBuilder();
        var keys = sortKeys ? data.Keys.OrderBy(k => k).ToList() : data.Keys.ToList();

        foreach (var key in keys)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"Key cannot be null or whitespace.");

            if (key.Contains(':'))
                throw new ArgumentException($"Key '{key}' contains invalid character ':'. Keys cannot contain colons.");

            if (key.Contains('\n') || key.Contains('\r'))
                throw new ArgumentException($"Key '{key}' contains line breaks. Keys cannot contain newlines.");

            if (key.TrimStart().StartsWith('#'))
                throw new ArgumentException($"Key '{key}' starts with '#'. Keys cannot start with a hash as it denotes a comment.");

            if (key.TrimStart().StartsWith("//"))
                throw new ArgumentException($"Key '{key}' starts with '//'. Keys cannot start with double slashes as they denote a comment.");

            if (key.Contains('[') || key.Contains(']') || key.Contains('@'))
                throw new ArgumentException($"Key '{key}' contains section marker characters ('[', ']', or '@'). Keys cannot contain these characters as they denote section boundaries.");

            string value = data[key];
            string escapedValue = TextEscaper.Escape(value, EscapeMode.KeyValue);
            result.AppendLine($"{key}: {escapedValue}");
        }

        return result.ToString();
    }

    /// <summary>
    /// Writes a dictionary to Key:Value format file. Values are escaped for multi-line content.
    /// </summary>
    /// <param name="filePath">The path to the file to write.</param>
    /// <param name="data">The dictionary to write.</param>
    /// <param name="sortKeys">If true, keys are sorted alphabetically. Default is false.</param>
    /// <param name="encoding">Text encoding (default: UTF-8 without BOM).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public static async Task WriteFileAsync(string filePath, Dictionary<string, string> data, bool sortKeys = false, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        string text = Write(data, sortKeys);
        await File.WriteAllTextAsync(filePath, text, encoding ?? TextEncoding.Utf8NoBom, cancellationToken);
    }
}
