namespace Nekote.Text;

/// <summary>
/// Defines the escaping strategy for encoding multi-line or special text.
/// </summary>
public enum EscapeMode
{
    /// <summary>
    /// Key-value file format escaping. Line breaks become literal \n, backslashes become \\.
    /// Used for embedding multi-line text in key-value configuration files.
    /// </summary>
    KeyValue,

    /// <summary>
    /// CSV format escaping following RFC 4180. Values containing commas, quotes, or line breaks
    /// are wrapped in quotes with internal quotes doubled.
    /// </summary>
    Csv,

    /// <summary>
    /// URL percent encoding. Encodes special characters as %XX hex values. Spaces may become + or %20.
    /// </summary>
    Url,

    /// <summary>
    /// HTML entity encoding. Converts special characters to HTML entities like &amp;, &lt;, &gt;, &quot;, etc.
    /// </summary>
    Html
}
