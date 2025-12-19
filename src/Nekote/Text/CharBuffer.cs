using System.Buffers;

namespace Nekote.Text;

/// <summary>
/// A mutable character buffer backed by ArrayPool for efficient span editing operations.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Problem this solves:</strong> ReadOnlySpan&lt;char&gt; cannot be modified in-place.
/// When you need to transform span content (filter, replace, append), you need a writable buffer.
/// This class provides that buffer while minimizing allocations by using ArrayPool.
/// </para>
/// <para>
/// <strong>Design philosophy:</strong> Similar to System.Drawing.Graphics - you create an editable
/// workspace, perform mutations, extract the result as a span, and dispose to release resources.
/// The buffer can be reused multiple times by calling <see cref="Clear"/> between operations.
/// </para>
/// <para>
/// <strong>Use cases:</strong>
/// - Filtering characters from a span (e.g., remove whitespace)
/// - Replacing characters in a span (e.g., normalize whitespace)
/// - Building strings from multiple spans without intermediate allocations
/// - Any transformation where you need to write modified span content
/// </para>
/// <para>
/// <strong>Memory safety:</strong> This is a ref struct and cannot escape to the heap.
/// Spans returned from <see cref="AsSpan()"/> are only valid until the next mutation operation
/// or until <see cref="Dispose"/> is called. The buffer is rented from ArrayPool and reused
/// to minimize GC pressure.
/// </para>
/// <para>
/// <strong>Example usage:</strong>
/// <code>
/// using var buffer = new CharBuffer(initialSize: 256);
///
/// // Append multiple spans
/// buffer.Append("Hello");
/// buffer.Append(' ');
/// buffer.Append("World");
///
/// ReadOnlySpan&lt;char&gt; result = buffer.AsSpan(); // "Hello World"
///
/// // Reuse the buffer
/// buffer.Clear();
/// buffer.Append("Foo");
/// result = buffer.AsSpan(); // "Foo"
/// </code>
/// </para>
/// </remarks>
public ref struct CharBuffer
{
    private char[]? _buffer;
    private int _capacity;
    private int _length;
    private readonly int? _maxSize;

    /// <summary>
    /// Gets the default initial buffer size (256 characters).
    /// </summary>
    /// <remarks>
    /// This value is used when <c>null</c> is passed to the <see cref="CharBuffer(int?, int?)"/> constructor.
    /// Exposed publicly to allow users to make informed decisions about buffer sizing.
    /// </remarks>
    public static readonly int DefaultInitialSize = 256;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharBuffer"/> struct.
    /// </summary>
    /// <param name="initialSize">
    /// The initial buffer capacity (0 or greater), or <c>null</c> to use <see cref="DefaultInitialSize"/> (256 characters).
    /// Note that <see cref="ArrayPool{T}"/> may return a buffer larger than requested due to bucket sizes.
    /// Specifying 0 is valid and will rent the pool's minimum buffer size.
    /// </param>
    /// <param name="maxSize">
    /// The maximum buffer size in characters. If null (default), the buffer can grow unbounded.
    /// If specified and capacity would exceed this size, an <see cref="InvalidOperationException"/> is thrown.
    /// </param>
    public CharBuffer(int? initialSize = null, int? maxSize = null)
    {
        int effectiveInitialSize = initialSize ?? DefaultInitialSize;

        if (effectiveInitialSize < 0)
            throw new ArgumentOutOfRangeException(nameof(initialSize), "Initial size must be 0 or greater.");

        if (maxSize.HasValue && maxSize.Value < effectiveInitialSize)
            throw new ArgumentException("Max size cannot be less than initial size.", nameof(maxSize));

        _buffer = ArrayPool<char>.Shared.Rent(effectiveInitialSize);
        _capacity = _buffer.Length;
        _length = 0;
        _maxSize = maxSize;
    }

    /// <summary>
    /// Gets or sets the character at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the character.</param>
    /// <returns>The character at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">Index is less than zero or greater than or equal to <see cref="Length"/>.</exception>
    public char this[int index]
    {
        get
        {
            if (index < 0 || index >= _length)
                throw new IndexOutOfRangeException($"Index {index} is out of range [0..{_length}).");

            return _buffer![index];
        }
        set
        {
            if (index < 0 || index >= _length)
                throw new IndexOutOfRangeException($"Index {index} is out of range [0..{_length}).");

            _buffer![index] = value;
        }
    }

    /// <summary>
    /// Gets or sets the current used length of the buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Setting this to a smaller value effectively truncates the buffer content.
    /// Setting to a larger value (up to <see cref="Capacity"/>) extends the used region
    /// without modifying existing characters. The extended region may contain garbage data
    /// from the <see cref="ArrayPool{T}"/> and must be initialized via the indexer before use.
    /// </para>
    /// <para>
    /// This pattern allows pre-allocation for scenarios where you know the final size:
    /// <code>
    /// buffer.Length = 100;
    /// for (int i = 0; i &lt; 100; i++)
    ///     buffer[i] = ComputeChar(i);
    /// </code>
    /// This is more efficient than calling <see cref="Append(char)"/> 100 times.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Value is negative or exceeds <see cref="Capacity"/>.</exception>
    public int Length
    {
        get => _length;
        set
        {
            if (value < 0 || value > _capacity)
                throw new ArgumentOutOfRangeException(nameof(value), $"Length must be in range [0..{_capacity}].");

            _length = value;
        }
    }

    /// <summary>
    /// Gets the current capacity of the buffer.
    /// </summary>
    /// <remarks>
    /// This is the total size of the rented array, which may be larger than the requested size
    /// due to ArrayPool's bucket sizes.
    /// </remarks>
    public int Capacity => _capacity;

    /// <summary>
    /// Gets a value indicating whether the buffer is empty.
    /// </summary>
    public bool IsEmpty => _length == 0;

    /// <summary>
    /// Ensures the buffer has at least the specified capacity.
    /// </summary>
    /// <param name="requiredSize">The minimum required capacity.</param>
    /// <exception cref="InvalidOperationException">
    /// Required size exceeds <see cref="_maxSize"/> if a maximum was specified.
    /// </exception>
    /// <remarks>
    /// If the current capacity is insufficient, the buffer will grow using an exponential
    /// doubling strategy until the required size is met. The old buffer is returned to the pool.
    /// </remarks>
    public void EnsureCapacity(int requiredSize)
    {
        if (requiredSize <= _capacity)
            return;

        // CRITICAL: Validate requiredSize is achievable before entering the growth loop.
        // Without this check, if requiredSize > maxSize, the loop would clamp to maxSize
        // but never satisfy the loop condition (newCapacity < requiredSize), causing an infinite loop.
        if (_maxSize.HasValue && requiredSize > _maxSize.Value)
            throw new InvalidOperationException($"Required size {requiredSize} exceeds maximum size {_maxSize.Value}.");

        // Growth loop: double capacity until we meet or exceed requiredSize.
        // Invariant: requiredSize <= maxSize (enforced by check above).
        int newCapacity = _capacity;
        while (newCapacity < requiredSize)
        {
            newCapacity *= 2;

            // Clamp to maxSize if doubling overshoots. This is safe because we know
            // requiredSize <= maxSize, so the clamped value still satisfies the requirement.
            if (_maxSize.HasValue && newCapacity > _maxSize.Value)
                newCapacity = _maxSize.Value;
        }

        char[] oldBuffer = _buffer!;
        char[] newBuffer = ArrayPool<char>.Shared.Rent(newCapacity);

        // Copy existing content
        oldBuffer.AsSpan(0, _length).CopyTo(newBuffer);

        _buffer = newBuffer;
        _capacity = newBuffer.Length;

        ArrayPool<char>.Shared.Return(oldBuffer);
    }

    /// <summary>
    /// Appends a character to the buffer.
    /// </summary>
    /// <param name="c">The character to append.</param>
    public void Append(char c)
    {
        EnsureCapacity(_length + 1);
        _buffer![_length++] = c;
    }

    /// <summary>
    /// Appends a span of characters to the buffer.
    /// </summary>
    /// <param name="chars">The characters to append.</param>
    public void Append(ReadOnlySpan<char> chars)
    {
        if (chars.IsEmpty)
            return;

        EnsureCapacity(_length + chars.Length);
        chars.CopyTo(_buffer.AsSpan(_length));
        _length += chars.Length;
    }

    /// <summary>
    /// Appends the default line terminator to the buffer.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="Environment.NewLine"/> as the line terminator.
    /// </remarks>
    public void AppendLine()
    {
        Append(Environment.NewLine);
    }

    /// <summary>
    /// Appends a span of characters followed by the default line terminator to the buffer.
    /// </summary>
    /// <param name="chars">The characters to append.</param>
    /// <remarks>
    /// Uses <see cref="Environment.NewLine"/> as the line terminator.
    /// </remarks>
    public void AppendLine(ReadOnlySpan<char> chars)
    {
        Append(chars);
        Append(Environment.NewLine);
    }

    /// <summary>
    /// Inserts a character at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert the character.</param>
    /// <param name="c">The character to insert.</param>
    /// <exception cref="ArgumentOutOfRangeException">Index is negative or greater than <see cref="Length"/>.</exception>
    public void Insert(int index, char c)
    {
        if (index < 0 || index > _length)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index must be in range [0..{_length}].");

        EnsureCapacity(_length + 1);

        // Shift existing content right
        if (index < _length)
        {
            _buffer.AsSpan(index, _length - index).CopyTo(_buffer.AsSpan(index + 1));
        }

        _buffer![index] = c;
        _length++;
    }

    /// <summary>
    /// Inserts a span of characters at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert the characters.</param>
    /// <param name="chars">The characters to insert.</param>
    /// <exception cref="ArgumentOutOfRangeException">Index is negative or greater than <see cref="Length"/>.</exception>
    public void Insert(int index, ReadOnlySpan<char> chars)
    {
        if (chars.IsEmpty)
            return;

        if (index < 0 || index > _length)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index must be in range [0..{_length}].");

        EnsureCapacity(_length + chars.Length);

        // Shift existing content right
        if (index < _length)
        {
            _buffer.AsSpan(index, _length - index).CopyTo(_buffer.AsSpan(index + chars.Length));
        }

        // Insert new content
        chars.CopyTo(_buffer.AsSpan(index));
        _length += chars.Length;
    }

    /// <summary>
    /// Removes a range of characters from the buffer.
    /// </summary>
    /// <param name="start">The zero-based starting index of the range to remove.</param>
    /// <param name="length">The number of characters to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Start or length is negative, or start + length exceeds <see cref="Length"/>.
    /// </exception>
    public void Remove(int start, int length)
    {
        if (start < 0 || start > _length)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (length < 0 || start + length > _length)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (length == 0)
            return;

        // Shift content left
        if (start + length < _length)
        {
            _buffer.AsSpan(start + length).CopyTo(_buffer.AsSpan(start));
        }

        _length -= length;
    }

    /// <summary>
    /// Removes all leading whitespace characters from the buffer.
    /// </summary>
    public void TrimStart()
    {
        int trimLength = 0;
        for (int i = 0; i < _length; i++)
        {
            if (!char.IsWhiteSpace(_buffer![i]))
                break;
            trimLength++;
        }

        if (trimLength > 0)
        {
            Remove(0, trimLength);
        }
    }

    /// <summary>
    /// Removes all trailing whitespace characters from the buffer.
    /// </summary>
    public void TrimEnd()
    {
        int trimLength = 0;
        for (int i = _length - 1; i >= 0; i--)
        {
            if (!char.IsWhiteSpace(_buffer![i]))
                break;
            trimLength++;
        }

        if (trimLength > 0)
        {
            _length -= trimLength;
        }
    }

    /// <summary>
    /// Removes all leading and trailing whitespace characters from the buffer.
    /// </summary>
    public void Trim()
    {
        TrimEnd();
        TrimStart();
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of the specified character.
    /// </summary>
    /// <param name="value">The character to seek.</param>
    /// <returns>The zero-based index of the first occurrence of the character, or -1 if not found.</returns>
    public int IndexOf(char value)
    {
        return AsSpan().IndexOf(value);
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of the specified character, starting at the specified position.
    /// </summary>
    /// <param name="value">The character to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index of the first occurrence of the character, or -1 if not found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Start index is negative or greater than <see cref="Length"/>.</exception>
    public int IndexOf(char value, int startIndex)
    {
        if (startIndex < 0 || startIndex > _length)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        int result = AsSpan(startIndex).IndexOf(value);
        return result >= 0 ? result + startIndex : -1;
    }

    /// <summary>
    /// Reports the zero-based index of the last occurrence of the specified character.
    /// </summary>
    /// <param name="value">The character to seek.</param>
    /// <returns>The zero-based index of the last occurrence of the character, or -1 if not found.</returns>
    public int LastIndexOf(char value)
    {
        return AsSpan().LastIndexOf(value);
    }

    /// <summary>
    /// Reports the zero-based index of the last occurrence of the specified character, searching backward from the specified position.
    /// </summary>
    /// <param name="value">The character to seek.</param>
    /// <param name="startIndex">The search starting position (searches backward from this position).</param>
    /// <returns>The zero-based index of the last occurrence of the character, or -1 if not found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Start index is negative or greater than or equal to <see cref="Length"/>.</exception>
    public int LastIndexOf(char value, int startIndex)
    {
        if (startIndex < 0 || startIndex >= _length)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        return AsSpan(0, startIndex + 1).LastIndexOf(value);
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of any character in the specified span.
    /// </summary>
    /// <param name="values">The set of characters to seek.</param>
    /// <returns>The zero-based index of the first occurrence of any character, or -1 if not found.</returns>
    public int IndexOfAny(ReadOnlySpan<char> values)
    {
        return AsSpan().IndexOfAny(values);
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of any character in the specified span, starting at the specified position.
    /// </summary>
    /// <param name="values">The set of characters to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index of the first occurrence of any character, or -1 if not found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Start index is negative or greater than <see cref="Length"/>.</exception>
    public int IndexOfAny(ReadOnlySpan<char> values, int startIndex)
    {
        if (startIndex < 0 || startIndex > _length)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        int result = AsSpan(startIndex).IndexOfAny(values);
        return result >= 0 ? result + startIndex : -1;
    }

    /// <summary>
    /// Reports the zero-based index of the last occurrence of any character in the specified span.
    /// </summary>
    /// <param name="values">The set of characters to seek.</param>
    /// <returns>The zero-based index of the last occurrence of any character, or -1 if not found.</returns>
    public int LastIndexOfAny(ReadOnlySpan<char> values)
    {
        return AsSpan().LastIndexOfAny(values);
    }

    /// <summary>
    /// Reports the zero-based index of the last occurrence of any character in the specified span, searching backward from the specified position.
    /// </summary>
    /// <param name="values">The set of characters to seek.</param>
    /// <param name="startIndex">The search starting position (searches backward from this position).</param>
    /// <returns>The zero-based index of the last occurrence of any character, or -1 if not found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Start index is negative or greater than or equal to <see cref="Length"/>.</exception>
    public int LastIndexOfAny(ReadOnlySpan<char> values, int startIndex)
    {
        if (startIndex < 0 || startIndex >= _length)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        return AsSpan(0, startIndex + 1).LastIndexOfAny(values);
    }

    /// <summary>
    /// Determines whether the buffer contains the specified character.
    /// </summary>
    /// <param name="value">The character to locate.</param>
    /// <returns><c>true</c> if the character is found; otherwise, <c>false</c>.</returns>
    public bool Contains(char value)
    {
        return IndexOf(value) >= 0;
    }

    /// <summary>
    /// Replaces all occurrences of a specified character with another character.
    /// </summary>
    /// <param name="oldChar">The character to be replaced.</param>
    /// <param name="newChar">The character to replace all occurrences of oldChar.</param>
    public void Replace(char oldChar, char newChar)
    {
        Span<char> span = AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == oldChar)
            {
                span[i] = newChar;
            }
        }
    }

    /// <summary>
    /// Replaces all occurrences of any specified character with a single replacement character.
    /// </summary>
    /// <param name="oldChars">The set of characters to be replaced.</param>
    /// <param name="newChar">The character to replace all occurrences of any character in oldChars.</param>
    /// <remarks>
    /// Useful for sanitization scenarios, e.g., replacing all whitespace variants with a single space.
    /// </remarks>
    public void ReplaceAny(ReadOnlySpan<char> oldChars, char newChar)
    {
        Span<char> span = AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            if (oldChars.Contains(span[i]))
            {
                span[i] = newChar;
            }
        }
    }

    /// <summary>
    /// Reverses the sequence of characters in the buffer.
    /// </summary>
    public void Reverse()
    {
        AsSpan().Reverse();
    }

    /// <summary>
    /// Copies the buffer contents to the specified span.
    /// </summary>
    /// <param name="destination">The destination span to copy to.</param>
    /// <exception cref="ArgumentException">Destination span is too small to hold the buffer contents.</exception>
    public void CopyTo(Span<char> destination)
    {
        if (destination.Length < _length)
            throw new ArgumentException($"Destination span length {destination.Length} is less than buffer length {_length}.", nameof(destination));

        _buffer.AsSpan(0, _length).CopyTo(destination);
    }

    /// <summary>
    /// Materializes the buffer contents as a string.
    /// </summary>
    /// <returns>A string containing the characters from index 0 to <see cref="Length"/> - 1.</returns>
    public override string ToString()
    {
        return new string(_buffer.AsSpan(0, _length));
    }

    /// <summary>
    /// Returns a span over the used portion of the buffer.
    /// </summary>
    /// <returns>A span containing the characters from index 0 to <see cref="Length"/> - 1.</returns>
    /// <remarks>
    /// <para>
    /// Returns <see cref="Span{T}"/> (mutable) rather than <see cref="ReadOnlySpan{T}"/>
    /// to match BCL conventions (e.g., <see cref="List{T}.AsSpan()"/>). This provides direct
    /// mutable access to the buffer for bulk operations.
    /// </para>
    /// <para>
    /// When read-only access is needed, <see cref="Span{T}"/> implicitly converts to
    /// <see cref="ReadOnlySpan{T}"/>, so a separate AsReadOnlySpan() method is unnecessary.
    /// </para>
    /// <para>
    /// The returned span is only valid until the next mutation operation (Append, Insert, Remove, etc.)
    /// or until <see cref="Dispose"/> is called.
    /// </para>
    /// </remarks>
    public Span<char> AsSpan()
    {
        return _buffer.AsSpan(0, _length);
    }

    /// <summary>
    /// Returns a span over a portion of the buffer.
    /// </summary>
    /// <param name="start">The zero-based starting index.</param>
    /// <param name="length">The number of characters in the span.</param>
    /// <returns>A span containing the specified slice of the buffer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Start or length is negative, or start + length exceeds <see cref="Length"/>.
    /// </exception>
    public Span<char> AsSpan(int start, int length)
    {
        if (start < 0 || start > _length)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (length < 0 || start + length > _length)
            throw new ArgumentOutOfRangeException(nameof(length));

        return _buffer.AsSpan(start, length);
    }

    /// <summary>
    /// Returns a span over a portion of the buffer from the specified start index to the end.
    /// </summary>
    /// <param name="start">The zero-based starting index.</param>
    /// <returns>A span containing the characters from start to <see cref="Length"/> - 1.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Start is negative or greater than <see cref="Length"/>.</exception>
    public Span<char> AsSpan(int start)
    {
        if (start < 0 || start > _length)
            throw new ArgumentOutOfRangeException(nameof(start));

        return _buffer.AsSpan(start, _length - start);
    }

    /// <summary>
    /// Clears the buffer, setting <see cref="Length"/> to 0.
    /// </summary>
    /// <remarks>
    /// This does not release the underlying buffer or clear its contents.
    /// It simply resets the used length to allow reusing the buffer.
    /// </remarks>
    public void Clear()
    {
        _length = 0;
    }

    /// <summary>
    /// Releases the rented buffer back to the ArrayPool.
    /// </summary>
    /// <remarks>
    /// After calling Dispose, no further operations should be performed on this instance.
    /// The buffer is returned to the pool for reuse by other code.
    /// </remarks>
    public void Dispose()
    {
        if (_buffer != null)
        {
            ArrayPool<char>.Shared.Return(_buffer);
            _buffer = null;
        }
    }
}
