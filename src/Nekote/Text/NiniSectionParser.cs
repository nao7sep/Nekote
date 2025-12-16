namespace Nekote.Text;

/// <summary>
/// Parses text into NINI sections with INI-style ([Section]) or at-prefix (@Section) markers.
/// Section content is parsed as key:value pairs.
/// </summary>
public static class NiniSectionParser
{
    /// <summary>
    /// Parses text into NINI sections. Supports both [INI-style] and @at-prefix markers automatically.
    /// Text is split into paragraphs (by blank lines). Each paragraph may optionally
    /// have a section marker as its first line, which labels the keys in that paragraph.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="options">Configuration options. If null, uses <see cref="NiniOptions.Default"/>.</param>
    /// <returns>Array of sections. Paragraphs without markers have Marker=None and Name="".</returns>
    public static NiniSection[] Parse(string text, NiniOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<NiniSection>();

        options ??= NiniOptions.Default;

        // Split into paragraphs - natural boundaries separated by blank lines
        var paragraphs = ParagraphParser.Parse(text);
        var sections = new List<NiniSection>();

        foreach (var paragraph in paragraphs)
        {
            var lines = LineParser.ToLines(paragraph);
            if (lines.Length == 0)
                continue;

            var firstLine = lines[0];
            var (markerStyle, sectionName) = TryParseSectionMarker(firstLine);

            IEnumerable<string> contentLines;
            if (markerStyle != NiniSectionMarkerStyle.None)
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
            var keyValues = NiniKeyValueParser.Parse(contentLines, options);

            // Add section if it has content OR if it's explicitly marked
            if (keyValues.Count > 0 || markerStyle != NiniSectionMarkerStyle.None)
            {
                sections.Add(new NiniSection
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
    /// <param name="options">Configuration options. If null, uses <see cref="NiniOptions.Default"/>.</param>
    /// <returns>The section if found, null otherwise.</returns>
    public static NiniSection? GetSection(NiniSection[] sections, string name, NiniOptions? options = null)
    {
        options ??= NiniOptions.Default;
        return sections.FirstOrDefault(s => options.SectionNameComparer.Equals(s.Name, name));
    }

    /// <summary>
    /// Tries to parse a line as a section marker. Supports both [INI-style] and @at-prefix markers.
    /// Line must start at column 0 (no leading whitespace).
    /// </summary>
    /// <returns>Tuple of (marker style, section name). Returns (None, null) if not a marker.</returns>
    private static (NiniSectionMarkerStyle style, string? name) TryParseSectionMarker(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return (NiniSectionMarkerStyle.None, null);

        // Section markers must start at column 0
        if (char.IsWhiteSpace(line[0]))
            return (NiniSectionMarkerStyle.None, null);

        // Try INI brackets first
        string? name = TryParseIniBrackets(line);
        if (name != null)
            return (NiniSectionMarkerStyle.IniBrackets, name);

        // Try at-prefix
        name = TryParseAtPrefix(line);
        if (name != null)
            return (NiniSectionMarkerStyle.AtPrefix, name);

        return (NiniSectionMarkerStyle.None, null);
    }

    /// <summary>
    /// Tries to parse a line as an INI-style [Section] marker.
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
        StringValidator.ValidateNoLeadingOrTrailingWhitespace(sectionName, "section name");

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
        StringValidator.ValidateNoLeadingOrTrailingWhitespace(sectionName, "section name");

        return sectionName;
    }
}


