namespace Nekote.Text;

/// <summary>
/// Represents a parsed NINI section with its marker style, name, and key-value content.
/// </summary>
public record NiniSection
{
    /// <summary>
    /// The marker style used for this section. If None, this section has no explicit marker.
    /// </summary>
    public required NiniSectionMarkerStyle Marker { get; init; }

    /// <summary>
    /// The section name. Empty string when Marker is None (unmarked key-value pairs).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The key-value pairs in this section. Empty dictionary if section has no content.
    /// </summary>
    public required Dictionary<string, string> KeyValues { get; init; }
}

