using System.Text;

namespace Nekote.Text;

/// <summary>
/// Represents a sectioned key-value file format with typed value access.
/// </summary>
/// <remarks>
/// <para>
/// A line-based text format organizing key-value pairs into named sections.
/// Uses key:value syntax (not key=value), supports [section] or @section markers,
/// and includes # and // style comments. Not a full INI parser - this is a
/// simplified format designed for clarity and culture-safe portable configuration.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var file = SectionedKeyValueFile.Load("config.txt");
/// int port = file.GetInt32("Database", "Port", defaultValue: 5432);
/// file.SetValue("Database", "Host", "localhost");
/// file.Save("config.txt");
/// </code>
/// </para>
/// </remarks>
public class SectionedKeyValueFile
{
    private readonly Dictionary<string, Dictionary<string, string>> _sections = new();
    private readonly SectionMarkerStyle _markerStyle;

    /// <summary>
    /// Creates a new empty SectionedKeyValueFile with the specified marker style.
    /// </summary>
    public SectionedKeyValueFile(SectionMarkerStyle markerStyle = SectionMarkerStyle.IniBrackets)
    {
        _markerStyle = markerStyle;
    }

    /// <summary>
    /// Loads a sectioned key-value file from the specified path.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <param name="markerStyle">Section marker style (default: IniBrackets).</param>
    /// <param name="encoding">Text encoding (default: UTF-8 without BOM).</param>
    /// <returns>Loaded SectionedKeyValueFile instance.</returns>
    public static SectionedKeyValueFile Load(string path, SectionMarkerStyle markerStyle = SectionMarkerStyle.IniBrackets, Encoding? encoding = null)
    {
        var text = File.ReadAllText(path, encoding ?? TextEncoding.Utf8NoBom);
        return Parse(text, markerStyle);
    }

    /// <summary>
    /// Parses a sectioned key-value file from a string.
    /// </summary>
    /// <param name="content">File content.</param>
    /// <param name="markerStyle">Section marker style (default: IniBrackets).</param>
    /// <returns>Parsed SectionedKeyValueFile instance.</returns>
    public static SectionedKeyValueFile Parse(string content, SectionMarkerStyle markerStyle = SectionMarkerStyle.IniBrackets)
    {
        var sections = SectionParser.Parse(content);
        var file = new SectionedKeyValueFile(markerStyle);

        foreach (var section in sections)
        {
            // Merge duplicate sections (standard INI behavior)
            if (file._sections.TryGetValue(section.Name, out var existing))
            {
                // Add or overwrite keys from this section
                foreach (var (key, value) in section.KeyValues)
                {
                    existing[key] = value;
                }
            }
            else
            {
                file._sections[section.Name] = section.KeyValues;
            }
        }

        return file;
    }

    /// <summary>
    /// Saves the file to the specified path.
    /// </summary>
    /// <param name="path">Path to save the file.</param>
    /// <param name="encoding">Text encoding (default: UTF-8 without BOM).</param>
    public void Save(string path, Encoding? encoding = null)
    {
        var content = ToString();
        File.WriteAllText(path, content, encoding ?? TextEncoding.Utf8NoBom);
    }

    /// <summary>
    /// Converts the file to a string representation.
    /// </summary>
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();

        // 1. Write preamble (keys without section) first
        if (_sections.TryGetValue("", out var preamble) && preamble.Count > 0)
        {
            var kvText = KeyValueWriter.Write(preamble);
            if (!string.IsNullOrEmpty(kvText))
            {
                sb.AppendLine(kvText);
                sb.AppendLine();
            }
        }

        // 2. Write named sections
        foreach (var (sectionName, keyValues) in _sections)
        {
            if (sectionName == "") continue; // Already handled

            // Write section marker
            if (_markerStyle == SectionMarkerStyle.IniBrackets)
                sb.AppendLine($"[{sectionName}]");
            else
                sb.AppendLine($"@{sectionName}");

            // Write key-value pairs
            var kvText = KeyValueWriter.Write(keyValues);
            if (!string.IsNullOrEmpty(kvText))
            {
                sb.AppendLine(kvText);
            }

            // Blank line between sections
            if (keyValues.Count > 0)
                sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    // Section access

    /// <summary>
    /// Gets the key-value dictionary for the specified section.
    /// Throws KeyNotFoundException if section does not exist.
    /// </summary>
    public Dictionary<string, string> this[string section] => _sections[section];

    /// <summary>
    /// Tries to get the key-value dictionary for the specified section.
    /// Returns false if section does not exist.
    /// </summary>
    public bool TryGetSection(string name, out Dictionary<string, string>? section)
    {
        if (_sections.TryGetValue(name, out var dict))
        {
            section = dict;
            return true;
        }
        section = null;
        return false;
    }

    /// <summary>
    /// Gets all section names.
    /// </summary>
    public IEnumerable<string> GetSectionNames() => _sections.Keys;

    /// <summary>
    /// Checks if a section exists.
    /// </summary>
    public bool HasSection(string name) => _sections.ContainsKey(name);

    /// <summary>
    /// Ensures a section exists, creating it if necessary.
    /// Returns the section's key-value dictionary.
    /// </summary>
    private Dictionary<string, string> EnsureSection(string name)
    {
        if (!_sections.TryGetValue(name, out var section))
        {
            section = new Dictionary<string, string>();
            _sections[name] = section;
        }
        return section;
    }

    /// <summary>
    /// Removes a section and all its key-value pairs.
    /// </summary>
    public void RemoveSection(string name) => _sections.Remove(name);

    // String value access

    /// <summary>
    /// Gets a string value from the specified section and key.
    /// Returns the default value if section or key does not exist.
    /// </summary>
    public string GetString(string section, string key, string defaultValue = "")
    {
        if (!_sections.TryGetValue(section, out var dict))
            return defaultValue;
        return dict.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Sets a string value for the specified section and key.
    /// Creates the section if it does not exist.
    /// </summary>
    /// <param name="section">The section name.</param>
    /// <param name="key">The key name.</param>
    /// <param name="value">The value to set. Cannot be null (use string.Empty for empty values).</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <remarks>
    /// The file format cannot distinguish between null and empty string (both serialize as "key: "),
    /// so null values are explicitly rejected to avoid ambiguity.
    /// </remarks>
    public void SetValue(string section, string key, string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value), "Value cannot be null. The file format cannot distinguish between null and empty string. Use string.Empty for empty values.");

        EnsureSection(section)[key] = value;
    }

    /// <summary>
    /// Removes a key from the specified section.
    /// </summary>
    public void RemoveValue(string section, string key)
    {
        if (_sections.TryGetValue(section, out var dict))
            dict.Remove(key);
    }

    // Typed value access

    /// <summary>
    /// Gets an Int32 value from the specified section and key.
    /// Returns the default value if section or key does not exist, or if the value cannot be parsed.
    /// </summary>
    public int GetInt32(string section, string key, int defaultValue = 0)
    {
        if (!_sections.TryGetValue(section, out var dict))
            return defaultValue;
        dict.TryGetValue(key, out var value);
        return TypedStringConverter.ToInt32(value, defaultValue);
    }

    /// <summary>
    /// Sets an Int32 value for the specified section and key.
    /// </summary>
    public void SetInt32(string section, string key, int value)
    {
        EnsureSection(section)[key] = TypedStringConverter.FromInt32(value);
    }

    /// <summary>
    /// Gets an Int64 value from the specified section and key.
    /// </summary>
    public long GetInt64(string section, string key, long defaultValue = 0)
    {
        if (!_sections.TryGetValue(section, out var dict))
            return defaultValue;
        dict.TryGetValue(key, out var value);
        return TypedStringConverter.ToInt64(value, defaultValue);
    }

    /// <summary>
    /// Sets an Int64 value for the specified section and key.
    /// </summary>
    public void SetInt64(string section, string key, long value)
    {
        EnsureSection(section)[key] = TypedStringConverter.FromInt64(value);
    }

    /// <summary>
    /// Gets a Double value from the specified section and key.
    /// </summary>
    public double GetDouble(string section, string key, double defaultValue = 0.0)
    {
        if (!_sections.TryGetValue(section, out var dict))
            return defaultValue;
        dict.TryGetValue(key, out var value);
        return TypedStringConverter.ToDouble(value, defaultValue);
    }

    /// <summary>
    /// Sets a Double value for the specified section and key.
    /// </summary>
    public void SetDouble(string section, string key, double value)
    {
        EnsureSection(section)[key] = TypedStringConverter.FromDouble(value);
    }

    /// <summary>
    /// Gets a Decimal value from the specified section and key.
    /// </summary>
    public decimal GetDecimal(string section, string key, decimal defaultValue = 0m)
    {
        if (!_sections.TryGetValue(section, out var dict))
            return defaultValue;
        dict.TryGetValue(key, out var value);
        return TypedStringConverter.ToDecimal(value, defaultValue);
    }

    /// <summary>
    /// Sets a Decimal value for the specified section and key.
    /// </summary>
    public void SetDecimal(string section, string key, decimal value)
    {
        EnsureSection(section)[key] = TypedStringConverter.FromDecimal(value);
    }

    /// <summary>
    /// Gets a Boolean value from the specified section and key.
    /// </summary>
    public bool GetBool(string section, string key, bool defaultValue = false)
    {
        if (!_sections.TryGetValue(section, out var dict))
            return defaultValue;
        dict.TryGetValue(key, out var value);
        return TypedStringConverter.ToBool(value, defaultValue);
    }

    /// <summary>
    /// Sets a Boolean value for the specified section and key.
    /// </summary>
    public void SetBool(string section, string key, bool value)
    {
        EnsureSection(section)[key] = TypedStringConverter.FromBool(value);
    }

    /// <summary>
    /// Gets a DateTime value from the specified section and key.
    /// </summary>
    public DateTime GetDateTime(string section, string key, DateTime defaultValue = default)
    {
        if (!_sections.TryGetValue(section, out var dict))
            return defaultValue;
        dict.TryGetValue(key, out var value);
        return TypedStringConverter.ToDateTime(value, defaultValue);
    }

    /// <summary>
    /// Sets a DateTime value for the specified section and key.
    /// </summary>
    public void SetDateTime(string section, string key, DateTime value)
    {
        EnsureSection(section)[key] = TypedStringConverter.FromDateTime(value);
    }

    /// <summary>
    /// Gets a Guid value from the specified section and key.
    /// </summary>
    public Guid GetGuid(string section, string key, Guid defaultValue = default)
    {
        if (!_sections.TryGetValue(section, out var dict))
            return defaultValue;
        dict.TryGetValue(key, out var value);
        return TypedStringConverter.ToGuid(value, defaultValue);
    }

    /// <summary>
    /// Sets a Guid value for the specified section and key.
    /// </summary>
    public void SetGuid(string section, string key, Guid value)
    {
        EnsureSection(section)[key] = TypedStringConverter.FromGuid(value);
    }

    /// <summary>
    /// Gets a TimeSpan value from the specified section and key.
    /// </summary>
    public TimeSpan GetTimeSpan(string section, string key, TimeSpan defaultValue = default)
    {
        if (!_sections.TryGetValue(section, out var dict))
            return defaultValue;
        dict.TryGetValue(key, out var value);
        return TypedStringConverter.ToTimeSpan(value, defaultValue);
    }

    /// <summary>
    /// Sets a TimeSpan value for the specified section and key.
    /// </summary>
    public void SetTimeSpan(string section, string key, TimeSpan value)
    {
        EnsureSection(section)[key] = TypedStringConverter.FromTimeSpan(value);
    }

    /// <summary>
    /// Gets a DateTimeOffset value from the specified section and key.
    /// </summary>
    public DateTimeOffset GetDateTimeOffset(string section, string key, DateTimeOffset defaultValue = default)
    {
        if (!_sections.TryGetValue(section, out var dict))
            return defaultValue;
        dict.TryGetValue(key, out var value);
        return TypedStringConverter.ToDateTimeOffset(value, defaultValue);
    }

    /// <summary>
    /// Sets a DateTimeOffset value for the specified section and key.
    /// </summary>
    public void SetDateTimeOffset(string section, string key, DateTimeOffset value)
    {
        EnsureSection(section)[key] = TypedStringConverter.FromDateTimeOffset(value);
    }

    /// <summary>
    /// Gets an enum value from the specified section and key.
    /// </summary>
    public TEnum GetEnum<TEnum>(string section, string key, TEnum defaultValue = default) where TEnum : struct, Enum
    {
        if (!_sections.TryGetValue(section, out var dict))
            return defaultValue;
        dict.TryGetValue(key, out var value);
        return TypedStringConverter.ToEnum(value, defaultValue);
    }

    /// <summary>
    /// Sets an enum value for the specified section and key.
    /// </summary>
    public void SetEnum<TEnum>(string section, string key, TEnum value) where TEnum : struct, Enum
    {
        EnsureSection(section)[key] = TypedStringConverter.FromEnum(value);
    }

    /// <summary>
    /// Gets a Uri value from the specified section and key.
    /// </summary>
    public Uri? GetUri(string section, string key, Uri? defaultValue = null)
    {
        if (!_sections.TryGetValue(section, out var dict))
            return defaultValue;
        dict.TryGetValue(key, out var value);
        return TypedStringConverter.ToUri(value, defaultValue);
    }

    /// <summary>
    /// Sets a Uri value for the specified section and key.
    /// </summary>
    public void SetUri(string section, string key, Uri value)
    {
        EnsureSection(section)[key] = TypedStringConverter.FromUri(value);
    }

    /// <summary>
    /// Gets a Version value from the specified section and key.
    /// </summary>
    public Version? GetVersion(string section, string key, Version? defaultValue = null)
    {
        if (!_sections.TryGetValue(section, out var dict))
            return defaultValue;
        dict.TryGetValue(key, out var value);
        return TypedStringConverter.ToVersion(value, defaultValue);
    }

    /// <summary>
    /// Sets a Version value for the specified section and key.
    /// </summary>
    public void SetVersion(string section, string key, Version value)
    {
        EnsureSection(section)[key] = TypedStringConverter.FromVersion(value);
    }
}
