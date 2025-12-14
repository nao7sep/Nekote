namespace Nekote.Text;

/// <summary>
/// Parses text into NINI sections with INI-style ([NiniSection]) or at-prefix (@NiniSection) markers.
/// NINI NiniSection content is parsed as key:value pairs.
/// </summary>
public static class NiniSectionParser
{
    /// <summary>
    /// Parses text into NINI sections. Supports both [INI-style] and @at-prefix markers automatically.
    /// Text is split into paragraphs (by blank lines). Each paragraph may optionally
    /// have a NiniSection marker as its first line, which labels the keys in that paragraph.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <returns>Array of sections. Paragraphs without markers have Marker=None and Name="".</returns>
    public static NiniSection[] Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<NiniSection>();

        // Split into paragraphs - natural boundaries separated by blank lines
        var paragraphs = ParagraphParser.Parse(text, trimParagraphs: false);
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
                // First line is a NiniSection marker - this paragraph defines that NiniSection
                // Content is the rest of the paragraph (after the marker line)
                contentLines = lines.Skip(1);
            }
            else
            {
                // No NiniSection marker - entire paragraph is content
                sectionName = "";
                contentLines = lines;
            }

            // Parse content as key-value pairs
            var keyValues = NiniKeyValueParser.Parse(contentLines);

            // Add NiniSection if it has content OR if it's an explicitly marked NiniSection
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
    /// Gets a NiniSection by name from parsed sections.
    /// </summary>
    /// <param name="sections">The array of parsed sections.</param>
    /// <param name="name">The NiniSection name to find.</param>
    /// <returns>The NiniSection if found, null otherwise.</returns>
    public static NiniSection? GetSection(NiniSection[] sections, string name)
    {
        return sections.FirstOrDefault(s => s.Name == name);
    }

    /// <summary>
    /// Tries to parse a line as a NiniSection marker. Supports both [INI-style] and @at-prefix markers.
    /// Line must start at column 0 (no leading whitespace).
    /// </summary>
    /// <returns>Tuple of (marker style, NiniSection name). Returns (None, null) if not a marker.</returns>
    private static (NiniSectionMarkerStyle style, string? name) TryParseSectionMarker(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return (NiniSectionMarkerStyle.None, null);

        // NiniSection markers must start at column 0
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
    /// Tries to parse a line as an INI-style [NiniSection] marker.
    /// </summary>
    /// <remarks>
    /// Does not support comments on the same line as NiniSection headers.
    /// "[NiniSection] # comment" will not be recognized as a NiniSection marker.
    /// </remarks>
    private static string? TryParseIniBrackets(string line)
    {
        if (!line.StartsWith('[') || !line.EndsWith(']'))
            return null;

        // Extract everything between brackets
        string sectionName = line.Substring(1, line.Length - 2);

        // Empty or whitespace-only NiniSection names in explicit markers are syntax errors
        // (Preamble is implicit, not marked with "[]")
        if (string.IsNullOrWhiteSpace(sectionName))
            throw new ArgumentException("NiniSection name cannot be empty or whitespace-only. Use content without NiniSection markers for preamble.", nameof(sectionName));

        // Validate NiniSection name has no leading/trailing whitespace
        StringValidator.ValidateNoLeadingOrTrailingWhitespace(sectionName, "NiniSection name");

        return sectionName;
    }

    /// <summary>
    /// Tries to parse a line as an @-prefix NiniSection marker.
    /// </summary>
    /// <remarks>
    /// Does not support comments on the same line as NiniSection headers.
    /// "@NiniSection # comment" will not be recognized as a NiniSection marker.
    /// </remarks>
    private static string? TryParseAtPrefix(string line)
    {
        if (!line.StartsWith('@'))
            return null;

        // Extract everything after @
        string sectionName = line.Substring(1);

        // Empty or whitespace-only NiniSection names in explicit markers are syntax errors
        // (Preamble is implicit, not marked with "@")
        if (string.IsNullOrWhiteSpace(sectionName))
            throw new ArgumentException("NiniSection name cannot be empty or whitespace-only. Use content without NiniSection markers for preamble.", nameof(sectionName));

        // Validate NiniSection name has no leading/trailing whitespace
        StringValidator.ValidateNoLeadingOrTrailingWhitespace(sectionName, "NiniSection name");

        return sectionName;
    }
}


