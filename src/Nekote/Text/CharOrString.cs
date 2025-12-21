namespace Nekote.Text;

/// <summary>
/// Represents a text value that can be either a single character or a string.
/// </summary>
/// <remarks>
/// <para>
/// This struct provides a unified representation for scenarios where either a character or string
/// is acceptable. It is optimized for the common case of single characters while supporting
/// multi-character strings when needed. The struct is small (16 bytes) and allocation-free.
/// </para>
/// <para>
/// This design uses a simple discriminated union rather than polymorphism (abstract base class
/// or interface), which would be overkill and introduce unnecessary heap allocations and virtual
/// dispatch overhead for such a simple value type.
/// </para>
/// <para>
/// Exactly one of the two values (char or string) must be present. The string cannot be null;
/// if null representation is needed, use <c>CharOrString?</c> instead.
/// </para>
/// </remarks>
public readonly struct CharOrString : IEquatable<CharOrString>
{
    private readonly char? _char;
    private readonly string? _string;

    private CharOrString(char c)
    {
        _char = c;
        _string = null;
    }

    private CharOrString(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        _char = null;
        _string = s;
    }

    /// <summary>
    /// Creates a <see cref="CharOrString"/> from a character.
    /// </summary>
    /// <param name="c">The character value.</param>
    /// <returns>A new <see cref="CharOrString"/> containing the character.</returns>
    public static CharOrString FromChar(char c) => new(c);

    /// <summary>
    /// Creates a <see cref="CharOrString"/> from a string.
    /// </summary>
    /// <param name="s">The string value.</param>
    /// <returns>A new <see cref="CharOrString"/> containing the string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="s"/> is null.</exception>
    public static CharOrString FromString(string s) => new(s);

    /// <summary>
    /// Gets a value indicating whether this instance contains a character.
    /// </summary>
    public bool IsChar => _char.HasValue;

    /// <summary>
    /// Gets a value indicating whether this instance contains a string.
    /// </summary>
    public bool IsString => _string != null;

    /// <summary>
    /// Gets the character value.
    /// </summary>
    /// <returns>The character value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when this instance does not contain a character.</exception>
    public char AsChar()
    {
        if (!_char.HasValue)
            throw new InvalidOperationException("This CharOrString instance does not contain a character.");
        return _char.Value;
    }

    /// <summary>
    /// Gets the string value.
    /// </summary>
    /// <returns>The string value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when this instance does not contain a string.</exception>
    public string AsString()
    {
        if (_string == null)
            throw new InvalidOperationException("This CharOrString instance does not contain a string.");
        return _string;
    }

    /// <summary>
    /// Gets the string value as a read-only span of characters.
    /// </summary>
    /// <returns>A read-only span of the string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when this instance does not contain a string.</exception>
    /// <remarks>
    /// This method only works for string values. For character values, use <see cref="AsChar"/> directly
    /// and create a span if needed: <c>stackalloc char[1] { value.AsChar() }</c>.
    /// </remarks>
    public ReadOnlySpan<char> AsSpan()
    {
        if (_string == null)
            throw new InvalidOperationException("This CharOrString instance does not contain a string.");
        return _string.AsSpan();
    }

    /// <summary>
    /// Gets the length of the text value.
    /// </summary>
    /// <returns>1 if this is a char, or the string length if this is a string.</returns>
    public int Length => _char.HasValue ? 1 : _string!.Length;

    /// <summary>
    /// Writes the value to a destination span.
    /// </summary>
    /// <param name="destination">The destination span.</param>
    /// <returns>The number of characters written.</returns>
    /// <exception cref="ArgumentException">Thrown when the destination is too small.</exception>
    public int WriteTo(Span<char> destination)
    {
        if (_char.HasValue)
        {
            if (destination.Length < 1)
                throw new ArgumentException("Destination too small.", nameof(destination));
            destination[0] = _char.Value;
            return 1;
        }

        if (destination.Length < _string!.Length)
            throw new ArgumentException($"Destination too small. Required: {_string.Length}, Available: {destination.Length}", nameof(destination));
        _string.AsSpan().CopyTo(destination);
        return _string.Length;
    }

    /// <summary>
    /// Implicitly converts a character to a <see cref="CharOrString"/>.
    /// </summary>
    public static implicit operator CharOrString(char c) => new(c);

    /// <summary>
    /// Implicitly converts a string to a <see cref="CharOrString"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="s"/> is null.</exception>
    public static implicit operator CharOrString(string s) => new(s);

    /// <inheritdoc/>
    public bool Equals(CharOrString other)
    {
        // Both chars: compare values
        if (_char.HasValue && other._char.HasValue)
            return _char.Value == other._char.Value;

        // Both strings: compare strings (ordinal)
        if (_string != null && other._string != null)
            return string.Equals(_string, other._string, StringComparison.Ordinal);

        // Different types: not equal
        return false;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is CharOrString other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _char.HasValue ? _char.Value.GetHashCode() : _string!.GetHashCode();
    }

    /// <summary>
    /// Determines whether two <see cref="CharOrString"/> instances are equal.
    /// </summary>
    public static bool operator ==(CharOrString left, CharOrString right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="CharOrString"/> instances are not equal.
    /// </summary>
    public static bool operator !=(CharOrString left, CharOrString right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => _char.HasValue ? _char.Value.ToString() : _string!;
}