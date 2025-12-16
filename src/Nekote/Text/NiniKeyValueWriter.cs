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
    /// <param name="options">Configuration options. If null, uses <see cref="NiniOptions.Default"/>.</param>
    /// <returns>NINI format text.</returns>
    /// <exception cref="ArgumentException">Thrown when a key contains invalid characters or starts with '#', '//', '[', or '@'.</exception>
    public static string Write(Dictionary<string, string> data, NiniOptions? options = null)
    {
        if (data == null || data.Count == 0)
            return string.Empty;

        options ??= NiniOptions.Default;
        var lines = new List<string>(data.Count);
        var keys = options.SortKeys
            ? data.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList()
            : data.Keys.ToList();

        foreach (var key in keys)
        {
            // Validate key using centralized validator
            StringValidator.ValidateNiniKey(key, options);

            string? value = data[key];

            // Reject null values explicitly - the format cannot distinguish between null and empty string
            // (both would be serialized as "key: "), so we require explicit empty string instead
            if (value == null)
                throw new ArgumentException($"Value for key '{key}' cannot be null. Use string.Empty for empty values.");

            string escapedValue = TextEscaper.Escape(value, EscapeMode.NiniValue)!; // value is non-null here
            lines.Add($"{key}{options.OutputSeparator}{escapedValue}");
        }

        return string.Join(options.NewLine, lines);
    }

    /// <summary>
    /// Writes a dictionary to a NINI format file. Values are escaped for multi-line content.
    /// </summary>
    /// <param name="filePath">The path to the file to write.</param>
    /// <param name="data">The dictionary to write.</param>
    /// <param name="options">Configuration options. If null, uses <see cref="NiniOptions.Default"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public static async Task WriteFileAsync(string filePath, Dictionary<string, string> data, NiniOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= NiniOptions.Default;
        string text = Write(data, options);
        await File.WriteAllTextAsync(filePath, text, options.Encoding, cancellationToken);
    }
}


