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
    /// <exception cref="ArgumentException">Thrown when the key contains invalid characters or patterns.</exception>
    public static void ValidateKeyValueFileKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

        ValidateNoLeadingOrTrailingWhitespace(key, "Key");

        if (key.Contains(':'))
            throw new ArgumentException($"Key '{key}' contains invalid character ':'. Keys cannot contain colons.", nameof(key));

        if (key.Contains('\n') || key.Contains('\r'))
            throw new ArgumentException($"Key '{key}' contains line breaks. Keys cannot contain newlines.", nameof(key));

        string trimmedKey = key.TrimStart();
        if (trimmedKey.StartsWith('#'))
            throw new ArgumentException($"Key '{key}' starts with '#'. Keys cannot start with a hash as it denotes a comment.", nameof(key));

        if (trimmedKey.StartsWith("//"))
            throw new ArgumentException($"Key '{key}' starts with '//'. Keys cannot start with double slashes as they denote a comment.", nameof(key));

        if (trimmedKey.StartsWith('['))
            throw new ArgumentException($"Key '{key}' starts with '['. Keys cannot start with an opening bracket as it denotes a section marker.", nameof(key));

        if (trimmedKey.StartsWith('@'))
            throw new ArgumentException($"Key '{key}' starts with '@'. Keys cannot start with at-sign as it denotes a section marker.", nameof(key));
    }
}
