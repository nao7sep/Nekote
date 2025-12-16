namespace Nekote.Platform;

/// <summary>
/// Provides standard line ending sequences for text files across different operating systems.
/// </summary>
public static class LineEndings
{
    /// <summary>
    /// Gets the Windows/DOS line ending (Carriage Return + Line Feed: <c>\r\n</c>).
    /// </summary>
    /// <remarks>
    /// Used by Windows, DOS, and most Internet protocols (HTTP, SMTP, etc.).
    /// </remarks>
    public static string CrLf => "\r\n";

    /// <summary>
    /// Gets the Unix/Linux/macOS line ending (Line Feed only: <c>\n</c>).
    /// </summary>
    /// <remarks>
    /// Used by Unix, Linux, macOS (since OS X), and most modern text files.
    /// </remarks>
    public static string Lf => "\n";

    /// <summary>
    /// Gets the legacy Macintosh line ending (Carriage Return only: <c>\r</c>).
    /// </summary>
    /// <remarks>
    /// Used by classic Mac OS (before OS X). Rarely encountered in modern systems.
    /// </remarks>
    public static string Cr => "\r";

    /// <summary>
    /// Gets the platform-native line ending for the current operating system.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="CrLf"/> on Windows and <see cref="Lf"/> on Unix-like systems.
    /// Equivalent to <see cref="Environment.NewLine"/>.
    /// </remarks>
    public static string Native => Environment.NewLine;
}
