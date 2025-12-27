namespace Nekote.Platform;

/// <summary>
/// Provides standard line ending sequences for text files across different operating systems.
/// </summary>
public static class LineEndings
{
    /// <summary>
    /// Windows/DOS line ending (Carriage Return + Line Feed: <c>\r\n</c>).
    /// </summary>
    public static string CrLf => "\r\n";

    /// <summary>
    /// Unix/Linux/macOS line ending (Line Feed only: <c>\n</c>).
    /// </summary>
    public static string Lf => "\n";

    /// <summary>
    /// Legacy Macintosh line ending (Carriage Return only: <c>\r</c>).
    /// </summary>
    public static string Cr => "\r";

    /// <summary>
    /// Platform-native line ending for the current operating system.
    /// </summary>
    public static string Native => Environment.NewLine;
}
