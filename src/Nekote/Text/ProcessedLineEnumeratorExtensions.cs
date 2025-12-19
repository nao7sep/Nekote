namespace Nekote.Text;

/// <summary>
/// Extension methods for creating and working with <see cref="ProcessedLineEnumerator"/> instances.
/// </summary>
public static class ProcessedLineEnumeratorExtensions
{
    /// <summary>
    /// Creates a <see cref="ProcessedLineEnumerator"/> to enumerate processed lines from text.
    /// </summary>
    /// <param name="text">The text to enumerate lines from.</param>
    /// <param name="options">The line processing options to apply.</param>
    /// <returns>A <see cref="ProcessedLineEnumerator"/> that applies the specified processing.</returns>
    /// <remarks>
    /// This is a convenience method that combines line enumeration and processing.
    /// The returned enumerator must be disposed if inline whitespace processing is used.
    /// </remarks>
    public static ProcessedLineEnumerator EnumerateProcessedLines(this ReadOnlySpan<char> text, LineProcessingOptions options)
    {
        return new ProcessedLineEnumerator(text.EnumerateLines(), options);
    }

    /// <summary>
    /// Creates a <see cref="ProcessedLineEnumerator"/> to enumerate processed lines from a string.
    /// </summary>
    /// <param name="text">The string to enumerate lines from.</param>
    /// <param name="options">The line processing options to apply.</param>
    /// <returns>A <see cref="ProcessedLineEnumerator"/> that applies the specified processing.</returns>
    /// <remarks>
    /// This is a convenience method that combines line enumeration and processing.
    /// The returned enumerator must be disposed if inline whitespace processing is used.
    /// Returns an empty enumerator if the string is null or empty.
    /// </remarks>
    public static ProcessedLineEnumerator EnumerateProcessedLines(this string? text, LineProcessingOptions options)
    {
        if (string.IsNullOrEmpty(text))
            return new ProcessedLineEnumerator(new LineEnumerator(ReadOnlySpan<char>.Empty), options);

        return new ProcessedLineEnumerator(text.AsSpan().EnumerateLines(), options);
    }
}
