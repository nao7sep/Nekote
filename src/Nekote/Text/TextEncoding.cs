using System.Text;

namespace Nekote.Text;

/// <summary>
/// Provides commonly used text encodings for file operations.
/// </summary>
/// <remarks>
/// <para>
/// This utility provides cached encoding instances to avoid repeated allocation.
/// UTF-8 without BOM is the recommended default for cross-platform text files.
/// </para>
/// <para>
/// BOM (Byte Order Mark) considerations:
/// - UTF-8 with BOM can cause issues with Unix tools, shebangs, and some parsers
/// - UTF-8 without BOM is the standard for configuration files, source code, and cross-platform text
/// - UTF-16/UTF-32 require BOM for proper detection
/// </para>
/// </remarks>
public static class TextEncoding
{
    /// <summary>
    /// UTF-8 encoding without Byte Order Mark (BOM).
    /// Recommended default for cross-platform text files.
    /// </summary>
    public static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// UTF-8 encoding with Byte Order Mark (BOM).
    /// Used by Windows Notepad and some legacy applications.
    /// </summary>
    public static readonly Encoding Utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

    /// <summary>
    /// UTF-16 Little Endian with BOM.
    /// Native encoding for Windows (used by .NET strings internally).
    /// </summary>
    public static readonly Encoding Utf16 = Encoding.Unicode;

    /// <summary>
    /// UTF-32 Little Endian with BOM.
    /// Rarely used; included for completeness.
    /// </summary>
    public static readonly Encoding Utf32 = Encoding.UTF32;
}
