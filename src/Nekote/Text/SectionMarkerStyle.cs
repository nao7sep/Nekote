namespace Nekote.Text;

/// <summary>
/// Defines how section markers are formatted in text.
/// </summary>
public enum SectionMarkerStyle
{
    /// <summary>
    /// No section marker - paragraph contains only key-value pairs.
    /// </summary>
    None,

    /// <summary>
    /// INI-style brackets: [SectionName]
    /// </summary>
    IniBrackets,

    /// <summary>
    /// At-prefix style: @SectionName
    /// </summary>
    AtPrefix
}
