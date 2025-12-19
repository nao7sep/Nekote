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

    /// <summary>
    /// Initializes a new instance of the <see cref="LineEnumerator"/> struct.
    /// </summary>
    /// <param name="text">The text to enumerate lines from.</param>
    public LineEnumerator(ReadOnlySpan<char> text)
    {
        _remaining = text;
        _current = default;
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
        if (_remaining.IsEmpty)
        {
            _current = default;
            return false;
        }

        int lineStart = 0;
        int i = 0;

        // Scan for line ending
        while (i < _remaining.Length)
        {
            char c = _remaining[i];

            if (c == '\r')
            {
                // Found \r - extract line before it
                _current = _remaining.Slice(lineStart, i - lineStart);

                // Check if next character is \n (Windows CRLF)
                if (i + 1 < _remaining.Length && _remaining[i + 1] == '\n')
                {
                    // \r\n - skip both characters
                    _remaining = _remaining.Slice(i + 2);
                }
                else
                {
                    // \r alone - old Mac line ending
                    _remaining = _remaining.Slice(i + 1);
                }

                return true;
            }
            else if (c == '\n')
            {
                // Found \n - extract line before it
                _current = _remaining.Slice(lineStart, i - lineStart);
                _remaining = _remaining.Slice(i + 1);
                return true;
            }

            i++;
        }

        // No line ending found - return rest of text as final line
        _current = _remaining;
        _remaining = ReadOnlySpan<char>.Empty;
        return true;
    }

    /// <summary>
    /// Returns this enumerator instance (enables foreach support).
    /// </summary>
    /// <returns>This enumerator instance.</returns>
    public LineEnumerator GetEnumerator() => this;
}
