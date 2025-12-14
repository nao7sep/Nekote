namespace Nekote.Text;

/// <summary>
/// Defines how section markers are formatted in text.
/// </summary>
public enum SectionMarkerStyle
{
    /// <summary>
    /// INI-style brackets: [SectionName]
    /// </summary>
    IniBrackets,

    /// <summary>
    /// At-prefix style: @SectionName
    /// </summary>
    AtPrefix
}

/// <summary>
/// Represents a parsed section with its name, line number, and key-value content.
/// </summary>
public record Section
{
    /// <summary>
    /// The section name. Empty string for content before the first section marker.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The line number where this section starts (1-based).
    /// </summary>
    public required int StartLine { get; init; }

    /// <summary>
    /// The key-value pairs in this section. Empty dictionary if section has no content.
    /// </summary>
    public required Dictionary<string, string> KeyValues { get; init; }
}

/// <summary>
/// Parses text into sections with INI-style ([section]) or at-prefix (@section) markers.
/// Section content is parsed as key:value pairs.
/// </summary>
public static class SectionParser
{
    /// <summary>
    /// Parses text into sections using the specified marker style.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="markerStyle">The section marker style to use.</param>
    /// <returns>Array of sections. Content before first marker appears as section with Name="".</returns>
    public static Section[] Parse(string text, SectionMarkerStyle markerStyle = SectionMarkerStyle.IniBrackets)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<Section>();

        var lines = LineParser.ToLines(text);
        var sections = new List<Section>();
        var currentSectionName = "";
        var currentSectionStartLine = 1;
        var currentSectionLines = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            // Try to parse as section marker
            string? sectionName = TryParseSectionMarker(line, markerStyle);

            if (sectionName != null)
            {
                // Save previous section if it has any lines
                if (currentSectionLines.Count > 0 || sections.Count > 0 || currentSectionName != "")
                {
                    var sectionContent = string.Join(Environment.NewLine, currentSectionLines);
                    var keyValues = KeyValueParser.Parse(sectionContent);
                    sections.Add(new Section
                    {
                        Name = currentSectionName,
                        StartLine = currentSectionStartLine,
                        KeyValues = keyValues
                    });
                }

                // Start new section
                currentSectionName = sectionName;
                currentSectionStartLine = i + 1;
                currentSectionLines.Clear();
            }
            else
            {
                // Add to current section content
                currentSectionLines.Add(lines[i]);
            }
        }

        // Add final section
        var finalContent = string.Join(Environment.NewLine, currentSectionLines);
        var finalKeyValues = KeyValueParser.Parse(finalContent);
        sections.Add(new Section
        {
            Name = currentSectionName,
            StartLine = currentSectionStartLine,
            KeyValues = finalKeyValues
        });

        return sections.ToArray();
    }

    /// <summary>
    /// Gets a section by name from parsed sections.
    /// </summary>
    /// <param name="sections">The array of parsed sections.</param>
    /// <param name="name">The section name to find.</param>
    /// <returns>The section if found, null otherwise.</returns>
    public static Section? GetSection(Section[] sections, string name)
    {
        return sections.FirstOrDefault(s => s.Name == name);
    }

    /// <summary>
    /// Tries to parse a line as a section marker.
    /// </summary>
    /// <returns>Section name if line is a marker, null otherwise.</returns>
    private static string? TryParseSectionMarker(string line, SectionMarkerStyle style)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        return style switch
        {
            SectionMarkerStyle.IniBrackets => TryParseIniBrackets(line),
            SectionMarkerStyle.AtPrefix => TryParseAtPrefix(line),
            _ => null
        };
    }

    /// <summary>
    /// Tries to parse a line as an INI-style [section] marker.
    /// </summary>
    /// <remarks>
    /// Does not support comments on the same line as section headers.
    /// "[Section] # comment" will not be recognized as a section marker.
    /// </remarks>
    private static string? TryParseIniBrackets(string line)
    {
        if (!line.StartsWith('[') || !line.EndsWith(']'))
            return null;

        // Extract everything between brackets
        string sectionName = line.Substring(1, line.Length - 2);

        // Empty or whitespace-only section names in explicit markers are syntax errors
        // (Preamble is implicit, not marked with "[]")
        if (string.IsNullOrWhiteSpace(sectionName))
            throw new ArgumentException("Section name cannot be empty or whitespace-only. Use content without section markers for preamble.", nameof(sectionName));

        // Validate section name has no leading/trailing whitespace
        StringValidator.ValidateSectionName(sectionName);

        return sectionName;
    }

    /// <summary>
    /// Tries to parse a line as an @-prefix section marker.
    /// </summary>
    /// <remarks>
    /// Does not support comments on the same line as section headers.
    /// "@Section # comment" will not be recognized as a section marker.
    /// </remarks>
    private static string? TryParseAtPrefix(string line)
    {
        if (!line.StartsWith('@'))
            return null;

        // Extract everything after @
        string sectionName = line.Substring(1);

        // Empty or whitespace-only section names in explicit markers are syntax errors
        // (Preamble is implicit, not marked with "@")
        if (string.IsNullOrWhiteSpace(sectionName))
            throw new ArgumentException("Section name cannot be empty or whitespace-only. Use content without section markers for preamble.", nameof(sectionName));

        // Validate section name has no leading/trailing whitespace
        StringValidator.ValidateSectionName(sectionName);

        return sectionName;
    }
}
