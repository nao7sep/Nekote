using Nekote.Text;

namespace NekoteTests.Text;

public class TypedStringConverterTests
{
    // Int32 conversion tests

    [Theory]
    [InlineData("123", 0, 123)]
    [InlineData("-456", 0, -456)]
    [InlineData("0", 0, 0)]
    [InlineData("2147483647", 0, int.MaxValue)]  // Max value
    [InlineData("-2147483648", 0, int.MinValue)]  // Min value
    public void ToInt32_ValidValues_ReturnsCorrectValue(string input, int defaultValue, int expected)
    {
        var result = TypedStringConverter.ToInt32(input, defaultValue);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, 42, 42)]
    [InlineData("", 42, 42)]
    [InlineData("   ", 42, 42)]
    [InlineData("abc", 42, 42)]
    [InlineData("12.5", 42, 42)]  // Decimal point not allowed for Int32
    [InlineData("1,000", 42, 42)]  // Comma not allowed in InvariantCulture
    [InlineData("😀", 42, 42)]    // Emoji is not a number
    public void ToInt32_InvalidValues_ReturnsDefault(string? input, int defaultValue, int expected)
    {
        var result = TypedStringConverter.ToInt32(input, defaultValue);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(123, "123")]
    [InlineData(-456, "-456")]
    [InlineData(0, "0")]
    public void FromInt32_ReturnsCorrectString(int input, string expected)
    {
        var result = TypedStringConverter.FromInt32(input);
        Assert.Equal(expected, result);
    }

    // Int64 conversion tests

    [Theory]
    [InlineData("9223372036854775807", 0L, 9223372036854775807L)]  // Max value
    [InlineData("-9223372036854775808", 0L, -9223372036854775808L)]  // Min value
    public void ToInt64_ValidValues_ReturnsCorrectValue(string input, long defaultValue, long expected)
    {
        var result = TypedStringConverter.ToInt64(input, defaultValue);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, 42L, 42L)]
    [InlineData("", 42L, 42L)]
    [InlineData("abc", 42L, 42L)]
    public void ToInt64_InvalidValues_ReturnsDefault(string? input, long defaultValue, long expected)
    {
        var result = TypedStringConverter.ToInt64(input, defaultValue);
        Assert.Equal(expected, result);
    }

    // Double conversion tests (culture-critical)

    [Theory]
    [InlineData("1.5", 0.0, 1.5)]
    [InlineData("3.14159", 0.0, 3.14159)]
    [InlineData("-0.5", 0.0, -0.5)]
    [InlineData("0.0", 0.0, 0.0)]
    [InlineData("1000.5", 0.0, 1000.5)]
    public void ToDouble_ValidValues_UsesDecimalPoint(string input, double defaultValue, double expected)
    {
        var result = TypedStringConverter.ToDouble(input, defaultValue);
        Assert.Equal(expected, result, precision: 5);
    }

    [Theory]
    [InlineData(null, 3.14, 3.14)]
    [InlineData("", 3.14, 3.14)]
    [InlineData("abc", 3.14, 3.14)]
    [InlineData("1,5", 3.14, 3.14)]  // CRITICAL: German decimal comma not accepted
    public void ToDouble_InvalidValues_ReturnsDefault(string? input, double defaultValue, double expected)
    {
        var result = TypedStringConverter.ToDouble(input, defaultValue);
        Assert.Equal(expected, result, precision: 2);
    }

    [Theory]
    [InlineData(1.5, "1.5")]  // CRITICAL: Always uses decimal point
    [InlineData(3.14159, "3.14159")]
    [InlineData(-0.5, "-0.5")]
    [InlineData(1000.5, "1000.5")]  // No thousand separator
    public void FromDouble_AlwaysUsesDecimalPoint(double input, string expected)
    {
        var result = TypedStringConverter.FromDouble(input);
        Assert.Equal(expected, result);
    }

    // Decimal conversion tests

    [Theory]
    [InlineData("1.5", 1.5)]
    [InlineData("123456.789", 123456.789)]
    public void ToDecimal_ValidValues_UsesDecimalPoint(string input, double expectedDouble)
    {
        var expected = (decimal)expectedDouble;
        var result = TypedStringConverter.ToDecimal(input, 0m);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, 3.14)]
    [InlineData("", 3.14)]
    [InlineData("abc", 3.14)]
    [InlineData("1,5", 3.14)]  // CRITICAL: German decimal comma not accepted
    public void ToDecimal_InvalidValues_ReturnsDefault(string? input, double defaultDouble)
    {
        var defaultValue = (decimal)defaultDouble;
        var result = TypedStringConverter.ToDecimal(input, defaultValue);
        Assert.Equal(defaultValue, result);
    }

    // Boolean conversion tests

    [Theory]
    [InlineData("true", false, true)]
    [InlineData("True", false, true)]
    [InlineData("TRUE", false, true)]
    [InlineData("false", true, false)]
    [InlineData("False", true, false)]
    [InlineData("FALSE", true, false)]
    public void ToBool_ValidValues_ReturnsCorrectValue(string input, bool defaultValue, bool expected)
    {
        var result = TypedStringConverter.ToBool(input, defaultValue);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, true, true)]
    [InlineData("", true, true)]
    [InlineData("yes", true, true)]
    [InlineData("1", true, true)]
    [InlineData("0", false, false)]
    public void ToBool_InvalidValues_ReturnsDefault(string? input, bool defaultValue, bool expected)
    {
        var result = TypedStringConverter.ToBool(input, defaultValue);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void FromBool_ReturnsLowercase(bool input, string expected)
    {
        var result = TypedStringConverter.FromBool(input);
        Assert.Equal(expected, result);
    }

    // DateTime conversion tests (culture-critical)

    [Theory]
    [InlineData("2025-12-14", 2025, 12, 14)]
    [InlineData("2025-12-14T15:30:00", 2025, 12, 14)]
    [InlineData("2025-12-14T15:30:00Z", 2025, 12, 14)]
    [InlineData("2025-01-01T00:00:00", 2025, 1, 1)]
    public void ToDateTime_ISO8601Format_ParsesCorrectly(string input, int year, int month, int day)
    {
        var result = TypedStringConverter.ToDateTime(input);
        Assert.Equal(year, result.Year);
        Assert.Equal(month, result.Month);
        Assert.Equal(day, result.Day);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-date")]
    [InlineData("14.12.2025")]  // German format not preferred (though might parse)
    [InlineData("12/14/2025")]  // US format not preferred (though might parse)
    public void ToDateTime_InvalidOrAmbiguousValues_ReturnsDefault(string? input)
    {
        var defaultValue = new DateTime(2000, 1, 1);
        var result = TypedStringConverter.ToDateTime(input, defaultValue);
        // Should return default for clearly invalid values
        if (input is null or "" or "not-a-date")
            Assert.Equal(defaultValue, result);
    }

    [Fact]
    public void FromDateTime_UsesISO8601Format()
    {
        var input = new DateTime(2025, 12, 14, 15, 30, 45, DateTimeKind.Utc);
        var result = TypedStringConverter.FromDateTime(input);

        // Should be ISO 8601 format: 2025-12-14T15:30:45.0000000Z
        Assert.StartsWith("2025-12-14T15:30:45", result);
        Assert.Contains("Z", result);  // UTC indicator
    }

    // Guid conversion tests

    [Fact]
    public void ToGuid_ValidGuid_ReturnsCorrectValue()
    {
        var expected = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        var input = "12345678-1234-1234-1234-123456789abc";

        var result = TypedStringConverter.ToGuid(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("12345678")]  // Too short
    public void ToGuid_InvalidValues_ReturnsDefault(string? input)
    {
        var defaultValue = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var result = TypedStringConverter.ToGuid(input, defaultValue);
        Assert.Equal(defaultValue, result);
    }

    [Fact]
    public void FromGuid_ReturnsLowercaseWithHyphens()
    {
        var input = Guid.Parse("12345678-ABCD-ABCD-ABCD-123456789ABC");
        var result = TypedStringConverter.FromGuid(input);

        Assert.Equal("12345678-abcd-abcd-abcd-123456789abc", result);
        Assert.Contains("-", result);
        Assert.DoesNotContain("{", result);
        Assert.DoesNotContain("}", result);
    }

    // Roundtrip tests (critical for config file compatibility)

    [Theory]
    [InlineData(123)]
    [InlineData(-456)]
    [InlineData(0)]
    public void Int32_Roundtrip_PreservesValue(int original)
    {
        var str = TypedStringConverter.FromInt32(original);
        var result = TypedStringConverter.ToInt32(str);
        Assert.Equal(original, result);
    }

    [Theory]
    [InlineData(1.5)]
    [InlineData(3.14159)]
    [InlineData(-0.5)]
    public void Double_Roundtrip_PreservesValue(double original)
    {
        var str = TypedStringConverter.FromDouble(original);
        var result = TypedStringConverter.ToDouble(str);
        Assert.Equal(original, result, precision: 5);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Bool_Roundtrip_PreservesValue(bool original)
    {
        var str = TypedStringConverter.FromBool(original);
        var result = TypedStringConverter.ToBool(str);
        Assert.Equal(original, result);
    }

    [Fact]
    public void DateTime_Roundtrip_PreservesValue()
    {
        var original = new DateTime(2025, 12, 14, 15, 30, 45, DateTimeKind.Utc);
        var str = TypedStringConverter.FromDateTime(original);
        var result = TypedStringConverter.ToDateTime(str);

        Assert.Equal(original.Year, result.Year);
        Assert.Equal(original.Month, result.Month);
        Assert.Equal(original.Day, result.Day);
        Assert.Equal(original.Hour, result.Hour);
        Assert.Equal(original.Minute, result.Minute);
        Assert.Equal(original.Second, result.Second);
    }

    [Fact]
    public void Guid_Roundtrip_PreservesValue()
    {
        var original = Guid.NewGuid();
        var str = TypedStringConverter.FromGuid(original);
        var result = TypedStringConverter.ToGuid(str);
        Assert.Equal(original, result);
    }

    // TimeSpan conversion tests

    [Theory]
    [InlineData("00:05:00", 0, 5, 0)]  // 5 minutes
    [InlineData("01:30:00", 1, 30, 0)]  // 1.5 hours
    [InlineData("1.12:00:00", 36, 0, 0)]  // 1 day 12 hours = 36 hours
    [InlineData("00:00:30", 0, 0, 30)]  // 30 seconds
    [InlineData("-00:05:00", 0, -5, 0)]  // Negative 5 minutes
    public void ToTimeSpan_ValidValues_ReturnsCorrectTimeSpan(string input, int expectedHours, int expectedMinutes, int expectedSeconds)
    {
        var result = TypedStringConverter.ToTimeSpan(input);
        var expected = TimeSpan.FromHours(expectedHours) + TimeSpan.FromMinutes(expectedMinutes) + TimeSpan.FromSeconds(expectedSeconds);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-timespan")]
    public void ToTimeSpan_InvalidValues_ReturnsDefault(string? input)
    {
        var defaultValue = TimeSpan.FromMinutes(10);
        var result = TypedStringConverter.ToTimeSpan(input, defaultValue);
        Assert.Equal(defaultValue, result);
    }

    [Theory]
    [InlineData(0, 5, 0, "00:05:00")]
    [InlineData(0, 90, 0, "01:30:00")]
    [InlineData(0, 0, 30, "00:00:30")]
    public void FromTimeSpan_ReturnsConstantFormat(int hours, int minutes, int seconds, string expected)
    {
        var timeSpan = new TimeSpan(hours, minutes, seconds);
        var result = TypedStringConverter.FromTimeSpan(timeSpan);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TimeSpan_Roundtrip_PreservesValue()
    {
        var original = TimeSpan.FromMinutes(127.5);
        var str = TypedStringConverter.FromTimeSpan(original);
        var result = TypedStringConverter.ToTimeSpan(str);
        Assert.Equal(original, result);
    }

    // DateTimeOffset conversion tests

    [Fact]
    public void ToDateTimeOffset_ValidISO8601_ReturnsCorrectValue()
    {
        var input = "2025-12-14T15:30:00+09:00";
        var result = TypedStringConverter.ToDateTimeOffset(input);

        Assert.Equal(2025, result.Year);
        Assert.Equal(12, result.Month);
        Assert.Equal(14, result.Day);
        Assert.Equal(15, result.Hour);
        Assert.Equal(30, result.Minute);
        Assert.Equal(TimeSpan.FromHours(9), result.Offset);
    }

    [Fact]
    public void ToDateTimeOffset_UTCFormat_ReturnsCorrectValue()
    {
        var input = "2025-12-14T15:30:00Z";
        var result = TypedStringConverter.ToDateTimeOffset(input);

        Assert.Equal(2025, result.Year);
        Assert.Equal(TimeSpan.Zero, result.Offset);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-date")]
    public void ToDateTimeOffset_InvalidValues_ReturnsDefault(string? input)
    {
        var defaultValue = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var result = TypedStringConverter.ToDateTimeOffset(input, defaultValue);
        Assert.Equal(defaultValue, result);
    }

    [Fact]
    public void FromDateTimeOffset_UsesISO8601Format()
    {
        var input = new DateTimeOffset(2025, 12, 14, 15, 30, 0, TimeSpan.FromHours(9));
        var result = TypedStringConverter.FromDateTimeOffset(input);

        Assert.StartsWith("2025-12-14T15:30:00", result);
        Assert.Contains("+09:00", result);
    }

    [Fact]
    public void DateTimeOffset_Roundtrip_PreservesValue()
    {
        var original = new DateTimeOffset(2025, 12, 14, 15, 30, 45, 123, TimeSpan.FromHours(-5));
        var str = TypedStringConverter.FromDateTimeOffset(original);
        var result = TypedStringConverter.ToDateTimeOffset(str);

        Assert.Equal(original, result);
    }

    // Enum conversion tests

    public enum TestEnum
    {
        ValueOne,
        ValueTwo,
        SomeOtherValue
    }

    [Theory]
    [InlineData("ValueOne", TestEnum.ValueOne)]
    [InlineData("valueone", TestEnum.ValueOne)]  // Case insensitive
    [InlineData("VALUEONE", TestEnum.ValueOne)]
    [InlineData("ValueTwo", TestEnum.ValueTwo)]
    [InlineData("SomeOtherValue", TestEnum.SomeOtherValue)]
    public void ToEnum_ValidValues_ReturnsCorrectEnum(string input, TestEnum expected)
    {
        var result = TypedStringConverter.ToEnum<TestEnum>(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, TestEnum.ValueTwo, TestEnum.ValueTwo)]
    [InlineData("", TestEnum.ValueTwo, TestEnum.ValueTwo)]
    [InlineData("InvalidValue", TestEnum.ValueTwo, TestEnum.ValueTwo)]
    public void ToEnum_InvalidValues_ReturnsDefault(string? input, TestEnum defaultValue, TestEnum expected)
    {
        var result = TypedStringConverter.ToEnum(input, defaultValue);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(TestEnum.ValueOne, "ValueOne")]
    [InlineData(TestEnum.ValueTwo, "ValueTwo")]
    [InlineData(TestEnum.SomeOtherValue, "SomeOtherValue")]
    public void FromEnum_ReturnsStringValue(TestEnum input, string expected)
    {
        var result = TypedStringConverter.FromEnum(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Enum_Roundtrip_PreservesValue()
    {
        var original = TestEnum.SomeOtherValue;
        var str = TypedStringConverter.FromEnum(original);
        var result = TypedStringConverter.ToEnum<TestEnum>(str);
        Assert.Equal(original, result);
    }

    // Uri conversion tests

    [Theory]
    [InlineData("https://example.com", "https://example.com/")]
    [InlineData("http://localhost:8080/path", "http://localhost:8080/path")]
    [InlineData("ftp://ftp.example.com", "ftp://ftp.example.com/")]
    [InlineData("file:///c:/temp/file.txt", "file:///c:/temp/file.txt")]
    public void ToUri_ValidAbsoluteUri_ReturnsCorrectUri(string input, string expectedString)
    {
        var result = TypedStringConverter.ToUri(input);

        Assert.NotNull(result);
        Assert.Equal(expectedString, result.ToString());
    }

    [Theory]
    [InlineData("relative/path")]
    [InlineData("/absolute/path")]
    public void ToUri_RelativeUri_ReturnsCorrectUri(string input)
    {
        var result = TypedStringConverter.ToUri(input);

        Assert.NotNull(result);
        Assert.Equal(input, result.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToUri_NullOrEmpty_ReturnsDefault(string? input)
    {
        var defaultValue = new Uri("https://default.com");
        var result = TypedStringConverter.ToUri(input, defaultValue);
        Assert.Equal(defaultValue, result);
    }

    [Fact]
    public void FromUri_ReturnsStringValue()
    {
        var input = new Uri("https://example.com/path?query=value");
        var result = TypedStringConverter.FromUri(input);

        Assert.Equal("https://example.com/path?query=value", result);
    }

    [Fact]
    public void Uri_Roundtrip_PreservesValue()
    {
        var original = new Uri("https://example.com:8080/path?query=value#fragment");
        var str = TypedStringConverter.FromUri(original);
        var result = TypedStringConverter.ToUri(str);

        Assert.Equal(original, result);
    }

    // Version conversion tests

    [Theory]
    [InlineData("1.2", 1, 2, -1, -1)]
    [InlineData("1.2.3", 1, 2, 3, -1)]
    [InlineData("1.2.3.4", 1, 2, 3, 4)]
    [InlineData("10.20.30.40", 10, 20, 30, 40)]
    public void ToVersion_ValidValues_ReturnsCorrectVersion(string input, int major, int minor, int build, int revision)
    {
        var result = TypedStringConverter.ToVersion(input);

        Assert.NotNull(result);
        Assert.Equal(major, result.Major);
        Assert.Equal(minor, result.Minor);
        if (build >= 0)
            Assert.Equal(build, result.Build);
        if (revision >= 0)
            Assert.Equal(revision, result.Revision);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-version")]
    [InlineData("1.2.3.4.5")]  // Too many parts
    public void ToVersion_InvalidValues_ReturnsDefault(string? input)
    {
        var defaultValue = new Version(1, 0, 0, 0);
        var result = TypedStringConverter.ToVersion(input, defaultValue);
        Assert.Equal(defaultValue, result);
    }

    [Theory]
    [InlineData(1, 2, -1, -1, "1.2")]
    [InlineData(1, 2, 3, -1, "1.2.3")]
    [InlineData(1, 2, 3, 4, "1.2.3.4")]
    public void FromVersion_ReturnsStringValue(int major, int minor, int build, int revision, string expected)
    {
        var version = build < 0
            ? new Version(major, minor)
            : revision < 0
                ? new Version(major, minor, build)
                : new Version(major, minor, build, revision);

        var result = TypedStringConverter.FromVersion(version);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Version_Roundtrip_PreservesValue()
    {
        var original = new Version(1, 2, 3, 4);
        var str = TypedStringConverter.FromVersion(original);
        var result = TypedStringConverter.ToVersion(str);

        Assert.Equal(original, result);
    }
}

