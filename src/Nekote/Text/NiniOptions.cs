using System.Text;
using Nekote.Platform;

namespace Nekote.Text;

/// <summary>
/// Configuration options for NINI format parsing and writing.
/// </summary>
/// <remarks>
/// <para>
/// Controls how NINI files are parsed and generated, including separator characters,
/// string comparison behavior, output formatting, and file I/O settings. All properties
/// are required. Use predefined instances for common scenarios or use <c>with</c> expressions
/// to customize specific properties.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// // Use predefined instance
/// var file = new NiniFile(NiniOptions.taskKiller);
///
/// // Customize using 'with' expression
/// var options = NiniOptions.Default with
/// {
///     SortKeys = true,
///     SortSections = true
/// };
/// var file = new NiniFile(options);
///
/// // Override output format when saving
/// file.Save("config.ini", outputOptions: NiniOptions.TraditionalIni);
/// </code>
/// </para>
/// </remarks>
public sealed record NiniOptions
{
    // ===== Parsing Settings =====
    // Control how input text is interpreted

    /// <summary>
    /// Gets the separator character used when parsing key-value pairs.
    /// </summary>
    /// <remarks>
    /// This character splits lines into key and value parts when reading input.
    /// Keys cannot contain this character. Common values: <c>':'</c> (NINI), <c>'='</c> (INI).
    /// This property is required.
    /// </remarks>
    public required char SeparatorChar { get; init; }

    /// <summary>
    /// Gets the string comparer for section names.
    /// </summary>
    /// <remarks>
    /// Used for section name dictionaries and equality comparisons when looking up sections.
    /// This property is required.
    /// </remarks>
    public required StringComparer SectionNameComparer { get; init; }

    /// <summary>
    /// Gets the string comparer for key names.
    /// </summary>
    /// <remarks>
    /// Used for key name dictionaries and equality comparisons when looking up keys.
    /// This property is required.
    /// </remarks>
    public required StringComparer KeyComparer { get; init; }

    // ===== Output Formatting =====
    // Control how output text is generated

    /// <summary>
    /// Gets the separator string used when writing key-value pairs.
    /// </summary>
    /// <remarks>
    /// This string appears between keys and values in generated output.
    /// Common values: <c>": "</c> (NINI), <c>":"</c> (taskKiller), <c>"="</c> (INI).
    /// This property is required.
    /// </remarks>
    public required string OutputSeparator { get; init; }

    /// <summary>
    /// Gets the section marker style used when writing files.
    /// </summary>
    /// <remarks>
    /// Choose <c>AtPrefix</c> for @Section or <c>IniBrackets</c> for [Section] style.
    /// This property is required.
    /// </remarks>
    public required NiniSectionMarkerStyle MarkerStyle { get; init; }

    /// <summary>
    /// Gets the newline sequence used when writing files.
    /// </summary>
    /// <remarks>
    /// Common values: <c>"\r\n"</c> (Windows), <c>"\n"</c> (Unix/Linux/macOS).
    /// This property is required.
    /// </remarks>
    public required string NewLine { get; init; }

    /// <summary>
    /// Gets whether to sort keys alphabetically when writing.
    /// </summary>
    /// <remarks>
    /// This property is required.
    /// </remarks>
    public required bool SortKeys { get; init; }

    /// <summary>
    /// Gets whether to sort section names alphabetically when writing.
    /// </summary>
    /// <remarks>
    /// The preamble (unnamed section) is always written first regardless of this setting.
    /// This property is required.
    /// </remarks>
    public required bool SortSections { get; init; }

    // ===== File I/O Settings =====
    // Control file reading and writing behavior

    /// <summary>
    /// Gets the text encoding used for file I/O operations.
    /// </summary>
    /// <remarks>
    /// This property is required.
    /// </remarks>
    public required Encoding Encoding { get; init; }

    // ===== Predefined Instances =====
    // Common configuration presets for different formats

    /// <summary>
    /// Gets the default NINI options.
    /// Uses <c>": "</c> separator with case-insensitive comparisons and @-prefix markers.
    /// </summary>
    public static NiniOptions Default { get; } = new()
    {
        SeparatorChar = ':',
        OutputSeparator = ": ",
        SectionNameComparer = StringComparer.OrdinalIgnoreCase,
        KeyComparer = StringComparer.OrdinalIgnoreCase,
        MarkerStyle = NiniSectionMarkerStyle.AtPrefix,
        NewLine = Environment.NewLine,
        SortKeys = false,
        SortSections = false,
        Encoding = TextEncoding.Utf8NoBom
    };

    /// <summary>
    /// Gets options for taskKiller format.
    /// Uses <c>":"</c> separator (no space) with case-sensitive comparisons, CRLF line endings, and no section markers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// taskKiller is a task management application that stores data as flat key-value pairs in paragraph-separated files.
    /// Each paragraph represents a task or note with fields like <c>Guid:</c>, <c>CreationUtc:</c>, <c>Content:</c>, etc.
    /// </para>
    /// <para>
    /// Format characteristics:
    /// <list type="bullet">
    /// <item><description>No section markers - files are flat key-value pairs only</description></item>
    /// <item><description>Colon separator with no space: <c>Key:Value</c></description></item>
    /// <item><description>Case-sensitive keys and values (preserves exact casing in GUIDs, field names)</description></item>
    /// <item><description>CRLF line endings (<c>\r\n</c>) for Windows compatibility</description></item>
    /// <item><description>UTF-8 with BOM (ensures Windows applications correctly detect encoding)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If named sections are encountered during output, an <see cref="InvalidOperationException"/> will be thrown
    /// because <see cref="NiniSectionMarkerStyle.None"/> cannot represent section boundaries.
    /// </para>
    /// </remarks>
    // The lowercase 't' in 'taskKiller' is intentional as it is the official product name (similar to iOS or xAI).
    public static NiniOptions taskKiller { get; } = new()
    {
        SeparatorChar = ':',
        OutputSeparator = ":",
        SectionNameComparer = StringComparer.Ordinal,
        KeyComparer = StringComparer.Ordinal,
        MarkerStyle = NiniSectionMarkerStyle.None,
        NewLine = LineEndings.CrLf,
        SortKeys = false,
        SortSections = false,
        Encoding = TextEncoding.Utf8WithBom
    };

    /// <summary>
    /// Gets options for traditional INI format.
    /// Uses <c>"="</c> separator with case-insensitive comparisons and [INI-style] brackets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Traditional INI files are configuration files widely used in Windows applications since the 1990s.
    /// This preset provides compatibility with Windows INI format conventions while using modern UTF-8 encoding.
    /// </para>
    /// <para>
    /// Format characteristics:
    /// <list type="bullet">
    /// <item><description>Section markers use brackets: <c>[Section]</c></description></item>
    /// <item><description>Equal sign separator: <c>Key=Value</c></description></item>
    /// <item><description>Case-insensitive keys and sections (matches Windows API behavior)</description></item>
    /// <item><description>CRLF line endings (<c>\r\n</c>) following Windows INI tradition</description></item>
    /// <item><description>UTF-8 without BOM (modern standard, ASCII-compatible for legacy tools)</description></item>
    /// <item><description>Supports semicolon comments (<c>;</c>) in addition to <c>#</c> and <c>//</c></description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Note: Traditional Windows INI files often used system codepage (like Windows-1252 or Shift-JIS),
    /// but UTF-8 without BOM is recommended for modern applications as it's backward-compatible with ASCII
    /// and handles international characters correctly.
    /// </para>
    /// </remarks>
    public static NiniOptions TraditionalIni { get; } = new()
    {
        SeparatorChar = '=',
        OutputSeparator = "=",
        SectionNameComparer = StringComparer.OrdinalIgnoreCase,
        KeyComparer = StringComparer.OrdinalIgnoreCase,
        MarkerStyle = NiniSectionMarkerStyle.IniBrackets,
        NewLine = LineEndings.CrLf,
        SortKeys = false,
        SortSections = false,
        Encoding = TextEncoding.Utf8NoBom
    };
}
