namespace Nekote.Text;

/// <summary>
/// Writes NINI sections to formatted text with section markers and key-value pairs.
/// </summary>
public static class NiniSectionWriter
{
    private static readonly string EmptySectionComment = "# (empty section)";

    /// <summary>
    /// Writes an array of NINI sections to formatted text.
    /// </summary>
    /// <param name="sections">The sections to write. Sections with Name="" are treated as preamble.</param>
    /// <param name="options">Configuration options. If null, uses <see cref="NiniOptions.Default"/>.</param>
    /// <returns>Formatted NINI text with sections separated by blank lines.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a named section is encountered with MarkerStyle.None.</exception>
    public static string Write(IEnumerable<NiniSection> sections, NiniOptions? options = null)
    {
        if (sections == null)
            return string.Empty;

        options ??= NiniOptions.Default;
        var newLine = options.NewLine;
        var paragraphs = new List<string>();

        var sectionArray = sections.ToArray();

        // 1. Write preamble (unnamed sections) first
        var preambles = sectionArray.Where(s => string.IsNullOrEmpty(s.Name));
        foreach (var preamble in preambles)
        {
            if (preamble.KeyValues.Count > 0)
            {
                paragraphs.Add(NiniKeyValueWriter.Write(preamble.KeyValues, options));
            }
        }

        // 2. Write named sections (optionally sorted)
        var namedSections = sectionArray.Where(s => !string.IsNullOrEmpty(s.Name));
        if (options.SortSections)
        {
            namedSections = namedSections.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase);
        }

        foreach (var section in namedSections)
        {
            // Validate: MarkerStyle.None cannot have named sections
            if (options.MarkerStyle == NiniSectionMarkerStyle.None)
            {
                throw new InvalidOperationException(
                    $"Cannot write named section '{section.Name}' with MarkerStyle.None. " +
                    "Named sections require MarkerStyle.AtPrefix or MarkerStyle.IniBrackets.");
            }

            var sectionParts = new List<string>();

            // Write section marker
            if (options.MarkerStyle == NiniSectionMarkerStyle.IniBrackets)
                sectionParts.Add($"[{section.Name}]");
            else
                sectionParts.Add($"@{section.Name}");

            // Write key-value pairs
            var kvText = NiniKeyValueWriter.Write(section.KeyValues, options);
            if (!string.IsNullOrEmpty(kvText))
            {
                sectionParts.Add(kvText);
            }
            else
            {
                // Empty section - append comment
                sectionParts.Add(EmptySectionComment);
            }

            paragraphs.Add(string.Join(newLine, sectionParts));
        }

        // Join paragraphs with blank lines (double newline)
        return string.Join($"{newLine}{newLine}", paragraphs);
    }

    /// <summary>
    /// Writes sections from a dictionary structure where keys are section names.
    /// </summary>
    /// <param name="sections">Dictionary where keys are section names (empty string for preamble) and values are key-value dictionaries.</param>
    /// <param name="options">Configuration options. If null, uses <see cref="NiniOptions.Default"/>.</param>
    /// <returns>Formatted NINI text with sections separated by blank lines.</returns>
    public static string Write(Dictionary<string, Dictionary<string, string>> sections, NiniOptions? options = null)
    {
        if (sections == null || sections.Count == 0)
            return string.Empty;

        options ??= NiniOptions.Default;

        // Convert dictionary to NiniSection array
        var sectionArray = sections.Select(kvp => new NiniSection
        {
            Name = kvp.Key,
            Marker = string.IsNullOrEmpty(kvp.Key) ? NiniSectionMarkerStyle.None : options.MarkerStyle,
            KeyValues = kvp.Value
        }).ToArray();

        return Write(sectionArray, options);
    }
}
