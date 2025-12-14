namespace Nekote.Text;

/// <summary>
/// Parses text into sections with INI-style ([section]) or at-prefix (@section) markers.
/// Section content is parsed as key:value pairs.
/// </summary>
public static class SectionParser
{
    /// <summary>
    /// Parses text into sections. Supports both [INI-style] and @at-prefix markers automatically.
    /// Text is split into paragraphs (by blank lines). Each paragraph may optionally
    /// have a section marker as its first line, which labels the keys in that paragraph.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <returns>Array of sections. Paragraphs without markers have Marker=None and Name="".</returns>
    public static Section[] Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<Section>();

        // Split into paragraphs - natural boundaries separated by blank lines
        var paragraphs = ParagraphParser.Parse(text, trimParagraphs: false);
        var sections = new List<Section>();

        foreach (var paragraph in paragraphs)
        {
            var lines = LineParser.ToLines(paragraph);
            if (lines.Length == 0)
                continue;

            var firstLine = lines[0];
            var (markerStyle, sectionName) = TryParseSectionMarker(firstLine);

            IEnumerable<string> contentLines;
            if (markerStyle != SectionMarkerStyle.None)
            {
                // First line is a section marker - this paragraph defines that section
                // Content is the rest of the paragraph (after the marker line)
                contentLines = lines.Skip(1);
            }
            else
            {
                // No section marker - entire paragraph is content
                sectionName = "";
                contentLines = lines;
            }

            // Parse content as key-value pairs
            var keyValues = KeyValueParser.Parse(contentLines);

            // Add section if it has content OR if it's an explicitly marked section
            if (keyValues.Count > 0 || markerStyle != SectionMarkerStyle.None)
            {
                sections.Add(new Section
                {
                    Marker = markerStyle,
                    Name = sectionName ?? "",
                    KeyValues = keyValues
                });
            }
        }

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
    /// Tries to parse a line as a section marker. Supports both [INI-style] and @at-prefix markers.
    /// Line must start at column 0 (no leading whitespace).
    /// </summary>
    /// <returns>Tuple of (marker style, section name). Returns (None, null) if not a marker.</returns>
    private static (SectionMarkerStyle style, string? name) TryParseSectionMarker(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return (SectionMarkerStyle.None, null);

        // Section markers must start at column 0
        if (char.IsWhiteSpace(line[0]))
            return (SectionMarkerStyle.None, null);

        // Try INI brackets first
        string? name = TryParseIniBrackets(line);
        if (name != null)
            return (SectionMarkerStyle.IniBrackets, name);

        // Try at-prefix
        name = TryParseAtPrefix(line);
        if (name != null)
            return (SectionMarkerStyle.AtPrefix, name);

        return (SectionMarkerStyle.None, null);
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
        StringValidator.ValidateNoLeadingOrTrailingWhitespace(sectionName, "Section name");

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
        StringValidator.ValidateNoLeadingOrTrailingWhitespace(sectionName, "Section name");

        return sectionName;
    }
}
