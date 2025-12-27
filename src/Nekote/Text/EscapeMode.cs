namespace Nekote.Text;

/// <summary>
/// Defines the escaping strategy for encoding multi-line or special text.
/// </summary>
public enum EscapeMode
{
    /// <summary>NINI escaping: line breaks become \n, backslashes become \\.</summary>
    NiniValue,

    /// <summary>CSV escaping per RFC 4180: quoted values with doubled quotes.</summary>
    Csv,

    /// <summary>URL percent encoding: special characters as %XX hex.</summary>
    Url,

    /// <summary>HTML entity encoding: special characters as HTML entities.</summary>
    Html
}
