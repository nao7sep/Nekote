using System.Globalization;

namespace Nekote.Text;

/// <summary>
/// Converts between nullable strings and typed values with culture-safe defaults.
/// </summary>
/// <remarks>
/// <para>
/// This "wrapper" class exists to enforce InvariantCulture for all conversions.
/// Configuration files are admin/power-user facing content that must work globally.
/// </para>
/// <para>
/// Without InvariantCulture:
/// - A German user saves "Timeout: 1,5" (decimal comma)
/// - A US user opens the file and fails to parse "1,5" (expects decimal point)
/// - The same happens for dates: "14.12.2025" vs "12/14/2025"
/// </para>
/// <para>
/// By enforcing InvariantCulture, configuration values remain portable across all locales.
/// Users editing config files manually must use invariant format (e.g., "1.5" not "1,5").
/// This is standard practice for .ini, .conf, .properties, and similar files.
/// </para>
/// </remarks>
public static class TypedStringConverter
{
    // Integer types

    /// <summary>
    /// Converts a nullable string to an Int32, using InvariantCulture.
    /// Returns the default value if the string is null, empty, or invalid.
    /// </summary>
    public static int ToInt32(string? value, int defaultValue = 0)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;
        return defaultValue;
    }

    /// <summary>
    /// Converts an Int32 to a string using InvariantCulture.
    /// </summary>
    public static string FromInt32(int value)
        => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a nullable string to an Int64, using InvariantCulture.
    /// Returns the default value if the string is null, empty, or invalid.
    /// </summary>
    public static long ToInt64(string? value, long defaultValue = 0)
    {
        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;
        return defaultValue;
    }

    /// <summary>
    /// Converts an Int64 to a string using InvariantCulture.
    /// </summary>
    public static string FromInt64(long value)
        => value.ToString(CultureInfo.InvariantCulture);

    // Floating-point types

    /// <summary>
    /// Converts a nullable string to a Double, using InvariantCulture (decimal point, not comma).
    /// Returns the default value if the string is null, empty, or invalid.
    /// </summary>
    public static double ToDouble(string? value, double defaultValue = 0.0)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            return result;
        return defaultValue;
    }

    /// <summary>
    /// Converts a Double to a string using InvariantCulture (always uses decimal point).
    /// </summary>
    public static string FromDouble(double value)
        => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a nullable string to a Decimal, using InvariantCulture (decimal point, not comma).
    /// Returns the default value if the string is null, empty, or invalid.
    /// </summary>
    public static decimal ToDecimal(string? value, decimal defaultValue = 0m)
    {
        if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            return result;
        return defaultValue;
    }

    /// <summary>
    /// Converts a Decimal to a string using InvariantCulture (always uses decimal point).
    /// </summary>
    public static string FromDecimal(decimal value)
        => value.ToString(CultureInfo.InvariantCulture);

    // Boolean type

    /// <summary>
    /// Converts a nullable string to a Boolean, using InvariantCulture.
    /// Accepts "true"/"false" (case-insensitive).
    /// Returns the default value if the string is null, empty, or invalid.
    /// </summary>
    public static bool ToBool(string? value, bool defaultValue = false)
    {
        if (bool.TryParse(value, out var result))
            return result;
        return defaultValue;
    }

    /// <summary>
    /// Converts a Boolean to a string using lowercase "true" or "false".
    /// </summary>
    public static string FromBool(bool value)
        => value ? "true" : "false";

    // DateTime type

    /// <summary>
    /// Converts a nullable string to a DateTime, using InvariantCulture with ISO 8601 format.
    /// Returns the default value if the string is null, empty, or invalid.
    /// </summary>
    /// <remarks>
    /// Accepts various ISO 8601 formats: "2025-12-14", "2025-12-14T15:30:00", "2025-12-14T15:30:00Z"
    /// </remarks>
    public static DateTime ToDateTime(string? value, DateTime defaultValue = default)
    {
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
            return result;
        return defaultValue;
    }

    /// <summary>
    /// Converts a DateTime to an ISO 8601 string using InvariantCulture.
    /// Format: "yyyy-MM-ddTHH:mm:ss.fffffffK" (roundtrip format preserving timezone).
    /// </summary>
    public static string FromDateTime(DateTime value)
        => value.ToString("o", CultureInfo.InvariantCulture);  // ISO 8601 roundtrip format

    // Guid type

    /// <summary>
    /// Converts a nullable string to a Guid.
    /// Returns the default value if the string is null, empty, or invalid.
    /// </summary>
    public static Guid ToGuid(string? value, Guid defaultValue = default)
    {
        if (Guid.TryParse(value, out var result))
            return result;
        return defaultValue;
    }

    /// <summary>
    /// Converts a Guid to a lowercase string without braces.
    /// Format: "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
    /// </summary>
    public static string FromGuid(Guid value)
        => value.ToString("D");  // Standard format: lowercase with hyphens

    // TimeSpan type

    /// <summary>
    /// Converts a nullable string to a TimeSpan, using InvariantCulture.
    /// Returns the default value if the string is null, empty, or invalid.
    /// </summary>
    /// <remarks>
    /// Accepts constant format: "[-][d.]hh:mm:ss[.fffffff]" (e.g., "00:05:00" for 5 minutes, "1.12:30:00" for 1 day 12.5 hours).
    /// </remarks>
    public static TimeSpan ToTimeSpan(string? value, TimeSpan defaultValue = default)
    {
        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result))
            return result;
        return defaultValue;
    }

    /// <summary>
    /// Converts a TimeSpan to a constant format string using InvariantCulture.
    /// Format: "[-][d.]hh:mm:ss[.fffffff]"
    /// </summary>
    public static string FromTimeSpan(TimeSpan value)
        => value.ToString("c", CultureInfo.InvariantCulture);  // Constant format

    // DateTimeOffset type

    /// <summary>
    /// Converts a nullable string to a DateTimeOffset, using InvariantCulture with ISO 8601 format.
    /// Returns the default value if the string is null, empty, or invalid.
    /// </summary>
    /// <remarks>
    /// Accepts ISO 8601 formats: "2025-12-14T15:30:00+00:00", "2025-12-14T15:30:00Z"
    /// </remarks>
    public static DateTimeOffset ToDateTimeOffset(string? value, DateTimeOffset defaultValue = default)
    {
        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
            return result;
        return defaultValue;
    }

    /// <summary>
    /// Converts a DateTimeOffset to an ISO 8601 string using InvariantCulture.
    /// Format: "yyyy-MM-ddTHH:mm:ss.fffffffK" (roundtrip format preserving offset).
    /// </summary>
    public static string FromDateTimeOffset(DateTimeOffset value)
        => value.ToString("o", CultureInfo.InvariantCulture);  // ISO 8601 roundtrip format

    // Enum type

    /// <summary>
    /// Converts a nullable string to an enum value, case-insensitive.
    /// Returns the default value if the string is null, empty, or invalid.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to parse.</typeparam>
    public static TEnum ToEnum<TEnum>(string? value, TEnum defaultValue = default) where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
            return result;
        return defaultValue;
    }

    /// <summary>
    /// Converts an enum value to its string representation.
    /// </summary>
    public static string FromEnum<TEnum>(TEnum value) where TEnum : struct, Enum
        => value.ToString();

    // Uri type

    /// <summary>
    /// Converts a nullable string to a Uri.
    /// Returns the default value if the string is null, empty, or invalid.
    /// </summary>
    public static Uri? ToUri(string? value, Uri? defaultValue = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var result))
            return result;
        return defaultValue;
    }

    /// <summary>
    /// Converts a Uri to its string representation.
    /// </summary>
    public static string FromUri(Uri value)
        => value.ToString();

    // Version type

    /// <summary>
    /// Converts a nullable string to a Version.
    /// Returns the default value if the string is null, empty, or invalid.
    /// </summary>
    /// <remarks>
    /// Accepts formats like "1.2", "1.2.3", "1.2.3.4"
    /// </remarks>
    public static Version? ToVersion(string? value, Version? defaultValue = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        if (Version.TryParse(value, out var result))
            return result;
        return defaultValue;
    }

    /// <summary>
    /// Converts a Version to its string representation.
    /// </summary>
    public static string FromVersion(Version value)
        => value.ToString();
}
