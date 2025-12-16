namespace Nekote.Text;

/// <summary>
/// Provides validation methods for strings used in structured text formats.
/// </summary>
public static class StringValidator
{
    /// <summary>
    /// Validates that a string does not start or end with whitespace characters.
    /// Whitespace at boundaries can cause ambiguity and be used as an attack vector
    /// (e.g., "key " vs "key" appearing identical but behaving differently).
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="parameterName">The name of the parameter being validated (for error messages).</param>
    /// <exception cref="ArgumentException">Thrown when the string starts or ends with whitespace.</exception>
    public static void ValidateNoLeadingOrTrailingWhitespace(string value, string parameterName)
    {
        if (string.IsNullOrEmpty(value))
            return;

        // Note: char.IsWhiteSpace() is SAFE here. All Unicode whitespace characters are in the
        // Basic Multilingual Plane (BMP) - none require surrogate pairs. This includes all common
        // whitespace (space, tab, newlines) and even exotic ones (U+2000-U+200A, U+3000, etc.).
        if (char.IsWhiteSpace(value[0]))
            throw new ArgumentException($"{parameterName} cannot start with whitespace. Leading whitespace can cause ambiguity in text formats.", parameterName);

        if (char.IsWhiteSpace(value[^1]))
            throw new ArgumentException($"{parameterName} cannot end with whitespace. Trailing whitespace can cause ambiguity in text formats.", parameterName);
    }

    /// <summary>
    /// Validates that a key is suitable for use in key-value formats.
    /// Checks for invalid characters and patterns that would conflict with the format syntax.
    /// </summary>
    /// <param name="key">The key to validate.</param>
    /// <param name="options">Configuration options. If null, uses <see cref="NiniOptions.Default"/>.</param>
    /// <exception cref="ArgumentException">Thrown when the key contains invalid characters or patterns.</exception>
    public static void ValidateNiniKey(string key, NiniOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

        options ??= NiniOptions.Default;
        ValidateNoLeadingOrTrailingWhitespace(key, "Key");

        // Note: All checks below are SAFE with char-based string methods. We're looking for
        // specific ASCII characters (separator, \n, \r, #, /, [, @) that never appear in surrogate pairs.
        // String.Contains() and String.StartsWith() correctly handle surrogate pairs in the
        // parts we're NOT checking, so emoji and other non-BMP characters pass through safely.
        if (key.Contains(options.SeparatorChar))
            throw new ArgumentException($"Key '{key}' contains invalid character '{options.SeparatorChar}'. Keys cannot contain the separator character.", nameof(key));

        if (key.Contains('\n') || key.Contains('\r'))
            throw new ArgumentException($"Key '{key}' contains line breaks. Keys cannot contain newlines.", nameof(key));

        string trimmedKey = key.TrimStart();
        if (trimmedKey.StartsWith('#'))
            throw new ArgumentException($"Key '{key}' starts with '#'. Keys cannot start with a hash as it denotes a comment.", nameof(key));

        if (trimmedKey.StartsWith("//"))
            throw new ArgumentException($"Key '{key}' starts with '//'. Keys cannot start with double slashes as they denote a comment.", nameof(key));
        if (trimmedKey.StartsWith(';'))
            throw new ArgumentException($"Key '{key}' starts with ';'. Keys cannot start with a semicolon as it denotes a comment.", nameof(key));
        if (trimmedKey.StartsWith('['))
            throw new ArgumentException($"Key '{key}' starts with '['. Keys cannot start with an opening bracket as it denotes a section marker.", nameof(key));

        if (trimmedKey.StartsWith('@'))
            throw new ArgumentException($"Key '{key}' starts with '@'. Keys cannot start with at-sign as it denotes a section marker.", nameof(key));
    }
}

