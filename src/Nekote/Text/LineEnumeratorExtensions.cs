namespace Nekote.Text;

/// <summary>
/// Extension methods for creating <see cref="LineEnumerator"/> instances.
/// </summary>
public static class LineEnumeratorExtensions
{
    /// <summary>
    /// Creates a <see cref="LineEnumerator"/> to enumerate lines in the text.
    /// </summary>
    /// <param name="text">The text to enumerate lines from.</param>
    /// <returns>A <see cref="LineEnumerator"/> for the text.</returns>
    /// <remarks>
    /// This is a zero-allocation operation that returns raw line spans from the original text.
    /// </remarks>
    public static LineEnumerator EnumerateLines(this ReadOnlySpan<char> text)
    {
        return new LineEnumerator(text);
    }

    /// <summary>
    /// Creates a <see cref="LineEnumerator"/> to enumerate lines in the string.
    /// </summary>
    /// <param name="text">The string to enumerate lines from.</param>
    /// <returns>A <see cref="LineEnumerator"/> for the string.</returns>
    /// <remarks>
    /// This is a zero-allocation operation that returns raw line spans from the original string.
    /// Returns an empty enumerator if the string is null or empty.
    /// </remarks>
    public static LineEnumerator EnumerateLines(this string? text)
    {
        if (string.IsNullOrEmpty(text))
            return new LineEnumerator(ReadOnlySpan<char>.Empty);

        return new LineEnumerator(text.AsSpan());
    }

    /// <summary>
    /// Creates a <see cref="ProcessedLineEnumerator"/> to enumerate processed lines.
    /// </summary>
    /// <param name="lines">The line enumerator to process.</param>
    /// <param name="options">The line processing options to apply.</param>
    /// <returns>A <see cref="ProcessedLineEnumerator"/> that applies the specified processing.</returns>
    /// <remarks>
    /// This wraps the line enumerator with processing capabilities. If processing requires
    /// buffers (e.g., for inline whitespace collapse), they are rented from ArrayPool and
    /// must be disposed via the returned enumerator's Dispose method.
    /// </remarks>
    public static ProcessedLineEnumerator ProcessLines(this LineEnumerator lines, LineProcessingOptions options)
    {
        return new ProcessedLineEnumerator(lines, options);
    }
}
