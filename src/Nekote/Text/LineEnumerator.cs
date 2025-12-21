using System;

namespace Nekote.Text;

/// <summary>
/// Supports iterating over lines in a text span without allocating strings.
/// </summary>
public ref struct LineEnumerator
{
    private ReadOnlySpan<char> _remaining;
    private ReadOnlySpan<char> _current;

    /// <summary>
    /// Initializes a new instance of the <see cref="LineEnumerator"/> struct.
    /// </summary>
    /// <param name="text">The text to iterate over.</param>
    public LineEnumerator(ReadOnlySpan<char> text)
    {
        _remaining = text;
        _current = default;
    }

    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    public ReadOnlySpan<char> Current => _current;

    /// <summary>
    /// Returns the enumerator itself.
    /// </summary>
    /// <returns>The enumerator instance.</returns>
    public LineEnumerator GetEnumerator() => this;

    /// <summary>
    /// Advances the enumerator to the next element of the collection.
    /// </summary>
    /// <returns><c>true</c> if the enumerator was successfully advanced to the next element; <c>false</c> if the enumerator has passed the end of the collection.</returns>
    public bool MoveNext()
    {
        return LineProcessor.TryReadLine(_remaining, out _current, out _remaining);
    }
}