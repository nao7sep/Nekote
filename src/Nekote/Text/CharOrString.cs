namespace Nekote.Text;

/// <summary>
/// Represents a text value that stores its content as a string while optionally remembering
/// if it originated from a single character for performance optimization.
/// </summary>
/// <remarks>
/// <para>
/// This struct provides a unified representation for scenarios where either a character or string
/// is acceptable. It internally stores all values as strings, but remembers when the value came
/// from a single character. This allows callers to use <see cref="HasChar"/> to check if they
/// can optimize operations by treating the value as a character.
/// </para>
/// <para>
/// Unlike a traditional discriminated union, this design ensures that string operations
/// (AsString, AsSpan, ToString) always work without throwing exceptions. The struct is
/// small (16 bytes) and provides convenient implicit conversions from both char and string.
/// </para>
/// <para>
/// The string value is never null. For null representation, use <c>CharOrString?</c> instead.
/// </para>
/// </remarks>
public readonly struct CharOrString : IEquatable<CharOrString>
{
    /// <summary>
    /// Gets an empty <see cref="CharOrString"/> instance.
    /// </summary>
    public static readonly CharOrString Empty = new();

    private readonly char? _char;
    private readonly string _string;

    /// <summary>
    /// Initializes a new instance with an empty string.
    /// </summary>
    public CharOrString()
    {
        _char = null;
        _string = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance from a character.
    /// </summary>
    /// <param name="c">The character value.</param>
    public CharOrString(char c)
    {
        _char = c;
        _string = c.ToString();
    }

    /// <summary>
    /// Initializes a new instance from a string.
    /// </summary>
    /// <param name="s">The string value. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="s"/> is null.</exception>
    public CharOrString(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        _char = s.Length == 1 ? s[0] : null;
        _string = s;
    }

    /// <summary>
    /// Gets a value indicating whether this instance originated from a single character.
    /// This can be used as a performance hint to optimize character-based operations.
    /// </summary>
    public bool HasChar => _char.HasValue;

    /// <summary>
    /// Gets the character value.
    /// </summary>
    /// <returns>The character value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when this instance was not constructed from a single character.</exception>
    public char AsChar()
    {
        if (!_char.HasValue)
            throw new InvalidOperationException("This CharOrString instance was not constructed from a single character. Use HasChar to check before calling AsChar().");
        return _char.Value;
    }

    /// <summary>
    /// Gets the string value. Always succeeds since all instances store a string internally.
    /// </summary>
    /// <returns>The string value.</returns>
    public string AsString() => _string;

    /// <summary>
    /// Gets the string value as a read-only span of characters. Always succeeds since all instances store a string internally.
    /// </summary>
    /// <returns>A read-only span of the string.</returns>
    public ReadOnlySpan<char> AsSpan() => _string.AsSpan();

    /// <summary>
    /// Gets the length of the text value.
    /// </summary>
    public int Length => _string.Length;

    /// <summary>
    /// Writes the value to a destination span.
    /// </summary>
    /// <param name="destination">The destination span.</param>
    /// <returns>The number of characters written.</returns>
    /// <exception cref="ArgumentException">Thrown when the destination is too small.</exception>
    public int WriteTo(Span<char> destination)
    {
        if (destination.Length < _string.Length)
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
        // Compare string content for semantic equality
        return string.Equals(_string, other._string, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is CharOrString other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        // Use string hash code to ensure consistent hashing for same content
        return _string.GetHashCode();
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
    public override string ToString() => _string;
}