namespace Nekote.Text;

/// <summary>
/// Enumerates lines in text, properly handling all line ending conventions (\r\n, \n, \r).
/// </summary>
/// <remarks>
/// This is a zero-allocation enumerator that returns <see cref="ReadOnlySpan{T}"/> slices
/// of the original text. No processing is performed - lines are returned exactly as they
/// appear in the source, excluding the line ending characters themselves.
/// <para>
/// Supports foreach iteration and can be composed with <see cref="ProcessedLineEnumerator"/>
/// for line processing operations.
/// </para>
/// </remarks>
public ref struct LineEnumerator
{
    private ReadOnlySpan<char> _remaining;
    private ReadOnlySpan<char> _current;
    private int _position;

    /// <summary>
    /// Initializes a new instance of the <see cref="LineEnumerator"/> struct.
    /// </summary>
    /// <param name="text">The text to enumerate lines from.</param>
    public LineEnumerator(ReadOnlySpan<char> text)
    {
        _remaining = text;
        _current = default;
        _position = 0;
    }

    /// <summary>
    /// Gets the current line as a <see cref="ReadOnlySpan{T}"/> slice of the original text.
    /// </summary>
    /// <remarks>
    /// The returned span is only valid until the next call to <see cref="MoveNext"/>.
    /// Line ending characters are not included in the returned span.
    /// </remarks>
    public ReadOnlySpan<char> Current => _current;

    /// <summary>
    /// Advances the enumerator to the next line.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the enumerator successfully advanced to the next line;
    /// <c>false</c> if the enumerator has reached the end of the text.
    /// </returns>
    public bool MoveNext()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns this enumerator instance (enables foreach support).
    /// </summary>
    /// <returns>This enumerator instance.</returns>
    public LineEnumerator GetEnumerator() => this;
}
