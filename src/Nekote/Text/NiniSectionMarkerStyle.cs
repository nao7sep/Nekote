namespace Nekote.Text;

/// <summary>
/// Defines how NINI section markers are formatted in text.
/// </summary>
public enum NiniSectionMarkerStyle
{
    /// <summary>No section marker.</summary>
    None,

    /// <summary>INI-style brackets: [SectionName]</summary>
    IniBrackets,

    /// <summary>At-prefix style: @SectionName</summary>
    AtPrefix
}

