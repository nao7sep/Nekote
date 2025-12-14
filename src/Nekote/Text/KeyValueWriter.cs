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
    /// <param name="sortKeys">If true, keys are sorted alphabetically using ordinal comparison. Default is false.</param>
    /// <returns>Key:Value format text.</returns>
    /// <exception cref="ArgumentException">Thrown when a key contains invalid characters (':', '\n', '\r') or starts with '#', '//', '[', or '@'.</exception>
    public static string Write(Dictionary<string, string> data, bool sortKeys = false)
    {
        if (data == null || data.Count == 0)
            return string.Empty;

        var result = new StringBuilder();
        var keys = sortKeys ? data.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList() : data.Keys.ToList();

        foreach (var key in keys)
        {
            // Validate key using centralized validator
            StringValidator.ValidateKeyValueFileKey(key);

            string? value = data[key];

            // Reject null values explicitly - the format cannot distinguish between null and empty string
            // (both would be serialized as "key: "), so we require explicit empty string instead
            if (value == null)
                throw new ArgumentException($"Value for key '{key}' cannot be null. Use string.Empty for empty values.");

            string escapedValue = TextEscaper.Escape(value, EscapeMode.NiniValue)!; // value is non-null here
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
