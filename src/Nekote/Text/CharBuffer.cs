using System.Buffers;

namespace Nekote.Text;

/// <summary>
/// A mutable character buffer backed by ArrayPool for efficient span editing operations.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Conceptual model:</strong> This class functions as a raw collection of characters (similar to <see cref="List{T}"/> for <see cref="char"/>)
/// rather than a high-level text builder like <see cref="System.Text.StringBuilder"/>. It provides direct, mutable access to the underlying
/// data via <see cref="AsSpan()"/>, prioritizing performance and flexibility. Consequently, it acts as a low-level buffer and does not
/// enforce Unicode validity; managing surrogate pairs, combining characters, and ensuring valid text encoding is the explicit responsibility of the caller.
/// </para>
/// <para>
/// <strong>Key advantages:</strong>
/// </para>
/// <list type="bullet">
/// <item><strong>Zero-allocation resizing:</strong> Uses <see cref="ArrayPool{T}"/> to reduce GC pressure by pooling the underlying arrays.</item>
/// <item><strong>Span-based editing:</strong> Exposes the buffer as a mutable <see cref="Span{T}"/>, enabling efficient in-place operations.</item>
/// <item><strong>Explicit cleanup:</strong> Implements <see cref="IDisposable"/> to return arrays to the pool promptly.</item>
/// </list>
/// <para>
/// <strong>Usage guidelines:</strong> Always use this class within a <c>using</c> statement or ensure <see cref="Dispose"/> is called to prevent memory leaks in the pool.
/// Spans obtained from <see cref="AsSpan()"/> should not be stored; they are only valid until the next mutation (which may replace the underlying array)
/// or until disposal.
/// </para>
/// </remarks>
public sealed class CharBuffer : IDisposable
{
    private char[]? _buffer;
    private int _capacity;
    private int _length;

    /// <summary>
    /// Gets the default initial buffer size (256 characters).
    /// </summary>
    /// <remarks>
    /// This value is used when the parameterless constructor is called.
    /// Exposed publicly to allow users to make informed decisions about buffer sizing.
    /// </remarks>
    public static readonly int DefaultInitialSize = 256;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharBuffer"/> struct with the default initial size.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="DefaultInitialSize"/> (256 characters) as the initial capacity.
    /// Note that <see cref="ArrayPool{T}"/> may return a buffer larger than requested due to bucket sizes.
    /// </remarks>
    public CharBuffer()
    {
        _buffer = ArrayPool<char>.Shared.Rent(DefaultInitialSize);
        _capacity = _buffer.Length;
        _length = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharBuffer"/> struct with the specified initial size.
    /// </summary>
    /// <param name="initialSize">
    /// The initial buffer capacity (0 or greater).
    /// Note that <see cref="ArrayPool{T}"/> may return a buffer larger than requested due to bucket sizes.
    /// Specifying 0 is valid and will rent the pool's minimum buffer size.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Initial size is negative.</exception>
    public CharBuffer(int initialSize)
    {
        if (initialSize < 0)
            throw new ArgumentOutOfRangeException(nameof(initialSize), "Initial size must be 0 or greater.");

        _buffer = ArrayPool<char>.Shared.Rent(initialSize);
        _capacity = _buffer.Length;
        _length = 0;
    }

    /// <summary>
    /// Gets or sets the character at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the character.</param>
    /// <returns>The character at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">Index is less than zero or greater than or equal to <see cref="Length"/>.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public char this[int index]
    {
        get
        {
            if (_buffer == null)
                throw new ObjectDisposedException(nameof(CharBuffer));

            if (index < 0 || index >= _length)
                throw new IndexOutOfRangeException($"Index {index} is out of range [0..{_length}).");

            return _buffer[index];
        }
        set
        {
            if (_buffer == null)
                throw new ObjectDisposedException(nameof(CharBuffer));

            if (index < 0 || index >= _length)
                throw new IndexOutOfRangeException($"Index {index} is out of range [0..{_length}).");

            _buffer[index] = value;
        }
    }

    /// <summary>
    /// Gets or sets the current used length of the buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Setting this to a smaller value effectively truncates the buffer content.
    /// Setting to a larger value (up to <see cref="Capacity"/>) extends the used region,
    /// zero-filling the extended portion to match <see cref="StringBuilder"/> behavior.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Value is negative or exceeds <see cref="Capacity"/>.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public int Length
    {
        get
        {
            if (_buffer == null)
                throw new ObjectDisposedException(nameof(CharBuffer));
            return _length;
        }
        set
        {
            if (_buffer == null)
                throw new ObjectDisposedException(nameof(CharBuffer));

            if (value < 0 || value > _capacity)
                throw new ArgumentOutOfRangeException(nameof(value), $"Length must be in range [0..{_capacity}].");

            // Zero-fill extended region to match StringBuilder behavior
            if (value > _length)
            {
                _buffer.AsSpan(_length, value - _length).Clear();
            }

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
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public int Capacity
    {
        get
        {
            if (_buffer == null)
                throw new ObjectDisposedException(nameof(CharBuffer));
            return _capacity;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the buffer is empty.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public bool IsEmpty
    {
        get
        {
            if (_buffer == null)
                throw new ObjectDisposedException(nameof(CharBuffer));
            return _length == 0;
        }
    }

    /// <summary>
    /// Ensures the buffer has at least the specified capacity.
    /// </summary>
    /// <param name="requiredSize">The minimum required capacity.</param>
    /// <remarks>
    /// If the current capacity is insufficient, the buffer will grow using an exponential
    /// doubling strategy until the required size is met. The old buffer is returned to the pool.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void EnsureCapacity(int requiredSize)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (requiredSize <= _capacity)
            return;

        if (requiredSize < 0)
            throw new ArgumentOutOfRangeException(nameof(requiredSize), "Required size must be non-negative.");

        // Growth loop: double capacity until we meet or exceed requiredSize
        int newCapacity = _capacity == 0 ? DefaultInitialSize : _capacity;
        while (newCapacity < requiredSize)
        {
            int nextCapacity = newCapacity * 2;
            if (nextCapacity < 0) // Overflow
            {
                newCapacity = requiredSize;
                break;
            }
            newCapacity = nextCapacity;
        }

        char[] oldBuffer = _buffer;
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
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void Append(char c)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        EnsureCapacity(_length + 1);
        _buffer[_length++] = c;
    }

    /// <summary>
    /// Appends a character to the buffer a specified number of times.
    /// </summary>
    /// <param name="c">The character to append.</param>
    /// <param name="repeatCount">The number of times to append the character.</param>
    /// <exception cref="ArgumentOutOfRangeException">Repeat count is negative.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void Append(char c, int repeatCount)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (repeatCount < 0)
            throw new ArgumentOutOfRangeException(nameof(repeatCount));

        if (repeatCount == 0)
            return;

        EnsureCapacity(_length + repeatCount);
        _buffer.AsSpan(_length, repeatCount).Fill(c);
        _length += repeatCount;
    }

    /// <summary>
    /// Appends a span of characters to the buffer.
    /// </summary>
    /// <param name="chars">The characters to append.</param>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void Append(ReadOnlySpan<char> chars)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

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
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
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
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
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
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void Insert(int index, char c)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (index < 0 || index > _length)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index must be in range [0..{_length}].");

        EnsureCapacity(_length + 1);

        // Shift existing content right
        if (index < _length)
        {
            _buffer.AsSpan(index, _length - index).CopyTo(_buffer.AsSpan(index + 1));
        }

        _buffer[index] = c;
        _length++;
    }

    /// <summary>
    /// Inserts a character at the specified index a specified number of times.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert the character.</param>
    /// <param name="c">The character to insert.</param>
    /// <param name="repeatCount">The number of times to insert the character.</param>
    /// <exception cref="ArgumentOutOfRangeException">Index is out of range or repeat count is negative.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void Insert(int index, char c, int repeatCount)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (index < 0 || index > _length)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index must be in range [0..{_length}].");

        if (repeatCount < 0)
            throw new ArgumentOutOfRangeException(nameof(repeatCount));

        if (repeatCount == 0)
            return;

        EnsureCapacity(_length + repeatCount);

        // Shift existing content right
        if (index < _length)
        {
            _buffer.AsSpan(index, _length - index).CopyTo(_buffer.AsSpan(index + repeatCount));
        }

        _buffer.AsSpan(index, repeatCount).Fill(c);
        _length += repeatCount;
    }

    /// <summary>
    /// Inserts a span of characters at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert the characters.</param>
    /// <param name="chars">The characters to insert.</param>
    /// <exception cref="ArgumentOutOfRangeException">Index is negative or greater than <see cref="Length"/>.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void Insert(int index, ReadOnlySpan<char> chars)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

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
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void Remove(int start, int length)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

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
    /// Reports the zero-based index of the first occurrence of the specified character.
    /// </summary>
    /// <param name="value">The character to seek.</param>
    /// <returns>The zero-based index of the first occurrence of the character, or -1 if not found.</returns>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public int IndexOf(char value)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        return AsSpan().IndexOf(value);
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of the specified character, starting at the specified position.
    /// </summary>
    /// <param name="value">The character to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index of the first occurrence of the character, or -1 if not found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Start index is negative or greater than <see cref="Length"/>.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public int IndexOf(char value, int startIndex)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

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
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public int LastIndexOf(char value)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        return AsSpan().LastIndexOf(value);
    }

    /// <summary>
    /// Reports the zero-based index of the last occurrence of the specified character, searching backward from the specified position.
    /// </summary>
    /// <param name="value">The character to seek.</param>
    /// <param name="startIndex">The search starting position (searches backward from this position).</param>
    /// <returns>The zero-based index of the last occurrence of the character, or -1 if not found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Start index is negative or greater than or equal to <see cref="Length"/>.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public int LastIndexOf(char value, int startIndex)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (startIndex < 0 || startIndex >= _length)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        return AsSpan(0, startIndex + 1).LastIndexOf(value);
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of any character in the specified span.
    /// </summary>
    /// <param name="values">The set of characters to seek.</param>
    /// <returns>The zero-based index of the first occurrence of any character, or -1 if not found.</returns>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public int IndexOfAny(ReadOnlySpan<char> values)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        return AsSpan().IndexOfAny(values);
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of any character in the specified span, starting at the specified position.
    /// </summary>
    /// <param name="values">The set of characters to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index of the first occurrence of any character, or -1 if not found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Start index is negative or greater than <see cref="Length"/>.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public int IndexOfAny(ReadOnlySpan<char> values, int startIndex)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

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
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public int LastIndexOfAny(ReadOnlySpan<char> values)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        return AsSpan().LastIndexOfAny(values);
    }

    /// <summary>
    /// Reports the zero-based index of the last occurrence of any character in the specified span, searching backward from the specified position.
    /// </summary>
    /// <param name="values">The set of characters to seek.</param>
    /// <param name="startIndex">The search starting position (searches backward from this position).</param>
    /// <returns>The zero-based index of the last occurrence of any character, or -1 if not found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Start index is negative or greater than or equal to <see cref="Length"/>.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public int LastIndexOfAny(ReadOnlySpan<char> values, int startIndex)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (startIndex < 0 || startIndex >= _length)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        return AsSpan(0, startIndex + 1).LastIndexOfAny(values);
    }

    /// <summary>
    /// Determines whether the buffer contains the specified character.
    /// </summary>
    /// <param name="value">The character to locate.</param>
    /// <returns><c>true</c> if the character is found; otherwise, <c>false</c>.</returns>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public bool Contains(char value)
    {
        return IndexOf(value) >= 0;
    }

    /// <summary>
    /// Determines whether the buffer contains the specified character within the specified range.
    /// </summary>
    /// <param name="value">The character to locate.</param>
    /// <param name="start">The starting index of the range to search.</param>
    /// <returns><c>true</c> if the character is found; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Start index is negative or greater than <see cref="Length"/>.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public bool Contains(char value, int start)
    {
        return IndexOf(value, start) >= 0;
    }

    /// <summary>
    /// Determines whether the buffer contains the specified character within the specified range.
    /// </summary>
    /// <param name="value">The character to locate.</param>
    /// <param name="start">The starting index of the range to search.</param>
    /// <param name="length">The number of characters in the range to search.</param>
    /// <returns><c>true</c> if the character is found; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Start or length is negative, or start + length exceeds <see cref="Length"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public bool Contains(char value, int start, int length)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (start < 0 || start > _length)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (length < 0 || start + length > _length)
            throw new ArgumentOutOfRangeException(nameof(length));

        int result = AsSpan(start, length).IndexOf(value);
        return result >= 0;
    }

    /// <summary>
    /// Determines whether the buffer contains any of the specified characters.
    /// </summary>
    /// <param name="values">The set of characters to locate.</param>
    /// <returns><c>true</c> if any of the characters are found; otherwise, <c>false</c>.</returns>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public bool ContainsAny(ReadOnlySpan<char> values)
    {
        return IndexOfAny(values) >= 0;
    }

    /// <summary>
    /// Determines whether the buffer contains any of the specified characters within the specified range.
    /// </summary>
    /// <param name="values">The set of characters to locate.</param>
    /// <param name="start">The starting index of the range to search.</param>
    /// <returns><c>true</c> if any of the characters are found; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Start index is negative or greater than <see cref="Length"/>.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public bool ContainsAny(ReadOnlySpan<char> values, int start)
    {
        return IndexOfAny(values, start) >= 0;
    }

    /// <summary>
    /// Determines whether the buffer contains any of the specified characters within the specified range.
    /// </summary>
    /// <param name="values">The set of characters to locate.</param>
    /// <param name="start">The starting index of the range to search.</param>
    /// <param name="length">The number of characters in the range to search.</param>
    /// <returns><c>true</c> if any of the characters are found; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Start or length is negative, or start + length exceeds <see cref="Length"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public bool ContainsAny(ReadOnlySpan<char> values, int start, int length)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (start < 0 || start > _length)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (length < 0 || start + length > _length)
            throw new ArgumentOutOfRangeException(nameof(length));

        int result = AsSpan(start, length).IndexOfAny(values);
        return result >= 0;
    }

    /// <summary>
    /// Replaces all occurrences of a specified character with another character.
    /// </summary>
    /// <param name="oldChar">The character to be replaced.</param>
    /// <param name="newChar">The character to replace all occurrences of oldChar.</param>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void Replace(char oldChar, char newChar)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

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
    /// Replaces all occurrences of a specified character with another character within the specified range.
    /// </summary>
    /// <param name="oldChar">The character to be replaced.</param>
    /// <param name="newChar">The character to replace all occurrences of oldChar.</param>
    /// <param name="start">The starting index of the range.</param>
    /// <exception cref="ArgumentOutOfRangeException">Start index is negative or greater than <see cref="Length"/>.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void Replace(char oldChar, char newChar, int start)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (start < 0 || start > _length)
            throw new ArgumentOutOfRangeException(nameof(start));

        Span<char> span = AsSpan(start);
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == oldChar)
            {
                span[i] = newChar;
            }
        }
    }

    /// <summary>
    /// Replaces all occurrences of a specified character with another character within the specified range.
    /// </summary>
    /// <param name="oldChar">The character to be replaced.</param>
    /// <param name="newChar">The character to replace all occurrences of oldChar.</param>
    /// <param name="start">The starting index of the range.</param>
    /// <param name="length">The number of characters in the range.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Start or length is negative, or start + length exceeds <see cref="Length"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void Replace(char oldChar, char newChar, int start, int length)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (start < 0 || start > _length)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (length < 0 || start + length > _length)
            throw new ArgumentOutOfRangeException(nameof(length));

        Span<char> span = AsSpan(start, length);
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
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void ReplaceAny(ReadOnlySpan<char> oldChars, char newChar)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (oldChars.IsEmpty)
            return;

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
    /// Replaces all occurrences of any specified character with a single replacement character within the specified range.
    /// </summary>
    /// <param name="oldChars">The set of characters to be replaced.</param>
    /// <param name="newChar">The character to replace all occurrences of any character in oldChars.</param>
    /// <param name="start">The starting index of the range.</param>
    /// <exception cref="ArgumentOutOfRangeException">Start index is negative or greater than <see cref="Length"/>.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void ReplaceAny(ReadOnlySpan<char> oldChars, char newChar, int start)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (oldChars.IsEmpty)
            return;

        if (start < 0 || start > _length)
            throw new ArgumentOutOfRangeException(nameof(start));

        Span<char> span = AsSpan(start);
        for (int i = 0; i < span.Length; i++)
        {
            if (oldChars.Contains(span[i]))
            {
                span[i] = newChar;
            }
        }
    }

    /// <summary>
    /// Replaces all occurrences of any specified character with a single replacement character within the specified range.
    /// </summary>
    /// <param name="oldChars">The set of characters to be replaced.</param>
    /// <param name="newChar">The character to replace all occurrences of any character in oldChars.</param>
    /// <param name="start">The starting index of the range.</param>
    /// <param name="length">The number of characters in the range.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Start or length is negative, or start + length exceeds <see cref="Length"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void ReplaceAny(ReadOnlySpan<char> oldChars, char newChar, int start, int length)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (oldChars.IsEmpty)
            return;

        if (start < 0 || start > _length)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (length < 0 || start + length > _length)
            throw new ArgumentOutOfRangeException(nameof(length));

        Span<char> span = AsSpan(start, length);
        for (int i = 0; i < span.Length; i++)
        {
            if (oldChars.Contains(span[i]))
            {
                span[i] = newChar;
            }
        }
    }

    /// <summary>
    /// Copies the buffer contents to the specified span.
    /// </summary>
    /// <param name="destination">The destination span to copy to.</param>
    /// <exception cref="ArgumentException">Destination span is too small to hold the buffer contents.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void CopyTo(Span<char> destination)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (destination.Length < _length)
            throw new ArgumentException($"Destination span length {destination.Length} is less than buffer length {_length}.", nameof(destination));

        _buffer.AsSpan(0, _length).CopyTo(destination);
    }

    /// <summary>
    /// Copies a portion of the buffer contents to the specified span.
    /// </summary>
    /// <param name="destination">The destination span to copy to.</param>
    /// <param name="start">The starting index in the buffer to copy from.</param>
    /// <exception cref="ArgumentOutOfRangeException">Start index is negative or greater than <see cref="Length"/>.</exception>
    /// <exception cref="ArgumentException">Destination span is too small to hold the buffer contents.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void CopyTo(Span<char> destination, int start)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (start < 0 || start > _length)
            throw new ArgumentOutOfRangeException(nameof(start));

        int copyLength = _length - start;
        if (destination.Length < copyLength)
            throw new ArgumentException($"Destination span length {destination.Length} is less than copy length {copyLength}.", nameof(destination));

        _buffer.AsSpan(start, copyLength).CopyTo(destination);
    }

    /// <summary>
    /// Copies a portion of the buffer contents to the specified span.
    /// </summary>
    /// <param name="destination">The destination span to copy to.</param>
    /// <param name="start">The starting index in the buffer to copy from.</param>
    /// <param name="length">The number of characters to copy.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Start or length is negative, or start + length exceeds <see cref="Length"/>.
    /// </exception>
    /// <exception cref="ArgumentException">Destination span is too small to hold the buffer contents.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void CopyTo(Span<char> destination, int start, int length)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (start < 0 || start > _length)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (length < 0 || start + length > _length)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (destination.Length < length)
            throw new ArgumentException($"Destination span length {destination.Length} is less than copy length {length}.", nameof(destination));

        _buffer.AsSpan(start, length).CopyTo(destination);
    }

    /// <summary>
    /// Materializes the buffer contents as a string.
    /// </summary>
    /// <returns>A string containing the characters from index 0 to <see cref="Length"/> - 1.</returns>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public override string ToString()
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));
        return new string(_buffer.AsSpan(0, _length));
    }

    /// <summary>
    /// Materializes a portion of the buffer contents as a string.
    /// </summary>
    /// <param name="start">The starting index of the range to convert.</param>
    /// <returns>A string containing the characters from start to <see cref="Length"/> - 1.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Start index is negative or greater than <see cref="Length"/>.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public string ToString(int start)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (start < 0 || start > _length)
            throw new ArgumentOutOfRangeException(nameof(start));

        return new string(_buffer.AsSpan(start, _length - start));
    }

    /// <summary>
    /// Materializes a portion of the buffer contents as a string.
    /// </summary>
    /// <param name="start">The starting index of the range to convert.</param>
    /// <param name="length">The number of characters to include.</param>
    /// <returns>A string containing the specified characters.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Start or length is negative, or start + length exceeds <see cref="Length"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public string ToString(int start, int length)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

        if (start < 0 || start > _length)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (length < 0 || start + length > _length)
            throw new ArgumentOutOfRangeException(nameof(length));

        return new string(_buffer.AsSpan(start, length));
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
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public Span<char> AsSpan()
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));
        return _buffer.AsSpan(0, _length);
    }

    /// <summary>
    /// Returns a span over a portion of the buffer.
    /// </summary>
    /// <param name="start">The zero-based starting index.</param>
    /// <param name="length">The number of characters in the span.</param>
    /// <returns>A span containing the specified slice of the buffer.</returns>
    /// <remarks>
    /// See <see cref="AsSpan()"/> for details on span lifetime and mutability.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Start or length is negative, or start + length exceeds <see cref="Length"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public Span<char> AsSpan(int start, int length)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

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
    /// <remarks>
    /// See <see cref="AsSpan()"/> for details on span lifetime and mutability.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Start is negative or greater than <see cref="Length"/>.</exception>
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public Span<char> AsSpan(int start)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));

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
    /// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
    public void Clear()
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(CharBuffer));
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
