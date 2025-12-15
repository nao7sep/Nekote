using Nekote.Text;

namespace NekoteTests.Text;

public class NiniFileTests
{
    // Parse and Load tests

    [Fact]
    public void Parse_SimpleConfig_ReturnsCorrectSections()
    {
        var input = @"[Database]
Host: localhost
Port: 5432

[Features]
AutoSave: true
Timeout: 30.5";

        var ini = NiniFile.Parse(input);

        Assert.True(ini.HasSection("Database"));
        Assert.True(ini.HasSection("Features"));
        Assert.Equal("localhost", ini.GetString("Database", "Host"));
        Assert.Equal(5432, ini.GetInt32("Database", "Port"));
        Assert.True(ini.GetBool("Features", "AutoSave"));
        Assert.Equal(30.5, ini.GetDouble("Features", "Timeout"), precision: 1);
    }

    [Fact]
    public void Parse_WithPreamble_IncludesEmptySection()
    {
        var input = @"# Preamble comment
GlobalKey: GlobalValue

[Section1]
Key1: Value1";

        var ini = NiniFile.Parse(input);

        Assert.True(ini.HasSection(""));  // Preamble
        Assert.True(ini.HasSection("Section1"));
        Assert.Equal("GlobalValue", ini.GetString("", "GlobalKey"));
        Assert.Equal("Value1", ini.GetString("Section1", "Key1"));
    }

    [Fact]
    public void Parse_AtPrefixStyle_ParsesCorrectly()
    {
        var input = @"@Database
Host: localhost
Port: 5432";

        var ini = NiniFile.Parse(input, NiniSectionMarkerStyle.AtPrefix);

        Assert.True(ini.HasSection("Database"));
        Assert.Equal("localhost", ini.GetString("Database", "Host"));
    }

    // ToString and Save tests

    [Fact]
    public void ToString_ProducesValidFormat()
    {
        var ini = new NiniFile();
        ini.SetValue("Database", "Host", "localhost");
        ini.SetInt32("Database", "Port", 5432);
        ini.SetBool("Features", "AutoSave", true);

        var result = ini.ToString();

        Assert.Contains("@Database", result);
        Assert.Contains("Host: localhost", result);
        Assert.Contains("Port: 5432", result);
        Assert.Contains("@Features", result);
        Assert.Contains("AutoSave: true", result);
    }

    [Fact]
    public void ToString_AtPrefixStyle_UsesAtMarkers()
    {
        var ini = new NiniFile(NiniSectionMarkerStyle.AtPrefix);
        ini.SetValue("Database", "Host", "localhost");

        var result = ini.ToString();

        Assert.Contains("@Database", result);
        Assert.DoesNotContain("[Database]", result);
    }

    [Fact]
    public void Roundtrip_PreservesData()
    {
        var original = new NiniFile();
        original.SetValue("Section1", "Key1", "Value1");
        original.SetInt32("Section1", "IntKey", 123);
        original.SetDouble("Section2", "DoubleKey", 3.14);
        original.SetBool("Section2", "BoolKey", true);

        var text = original.ToString();
        var restored = NiniFile.Parse(text);

        Assert.Equal("Value1", restored.GetString("Section1", "Key1"));
        Assert.Equal(123, restored.GetInt32("Section1", "IntKey"));
        Assert.Equal(3.14, restored.GetDouble("Section2", "DoubleKey"), precision: 2);
        Assert.True(restored.GetBool("Section2", "BoolKey"));
    }

    [Fact]
    public void ToString_CustomNewLine_UsesSpecifiedNewLine()
    {
        var ini = new NiniFile(NiniSectionMarkerStyle.AtPrefix);
        ini.SetValue("Section1", "Key1", "Value1");
        ini.SetValue("Section2", "Key2", "Value2");

        var newLine = "NEWLINE"; // Use a visible marker to be sure
        var result = ini.ToString(newLine);

        Assert.Contains($"@Section1{newLine}", result);
        Assert.Contains($"Key1: Value1{newLine}", result);
        // Sections separated by double newline
        Assert.Contains($"{newLine}{newLine}@Section2", result);
    }

    [Fact]
    public void ToString_EmptySection_AppendsComment()
    {
        var ini = NiniFile.Parse("@EmptySection");
        var result = ini.ToString();

        Assert.Contains("@EmptySection", result);
        Assert.Contains("# (empty section)", result);
    }

    // Section access tests

    [Fact]
    public void Indexer_ExistingSection_ReturnsKeyValues()
    {
        var ini = new NiniFile();
        ini.SetValue("Section1", "Key1", "Value1");

        var section = ini["Section1"];

        Assert.Equal("Value1", section["Key1"]);
    }

    [Fact]
    public void Indexer_NonExistingSection_ThrowsException()
    {
        var ini = new NiniFile();

        Assert.Throws<KeyNotFoundException>(() => ini["NonExisting"]);
    }

    [Fact]
    public void TryGetSection_ExistingSection_ReturnsTrue()
    {
        var ini = new NiniFile();
        ini.SetValue("Section1", "Key1", "Value1");

        var found = ini.TryGetSection("Section1", out var section);

        Assert.True(found);
        Assert.NotNull(section);
        Assert.Equal("Value1", section["Key1"]);
    }

    [Fact]
    public void TryGetSection_NonExistingSection_ReturnsFalse()
    {
        var ini = new NiniFile();

        var found = ini.TryGetSection("NonExisting", out var section);

        Assert.False(found);
        Assert.Null(section);
    }

    [Fact]
    public void GetSectionNames_ReturnsAllSections()
    {
        var ini = new NiniFile();
        ini.SetValue("Section1", "Key", "Value");
        ini.SetValue("Section2", "Key", "Value");

        var names = ini.GetSectionNames().ToList();

        Assert.Equal(2, names.Count);
        Assert.Contains("Section1", names);
        Assert.Contains("Section2", names);
    }

    [Fact]
    public void RemoveSection_RemovesSection()
    {
        var ini = new NiniFile();
        ini.SetValue("Section1", "Key", "Value");

        ini.RemoveSection("Section1");

        Assert.False(ini.HasSection("Section1"));
    }

    // String value access tests

    [Fact]
    public void GetString_ExistingKey_ReturnsValue()
    {
        var ini = new NiniFile();
        ini.SetValue("Section1", "Key1", "Value1");

        var result = ini.GetString("Section1", "Key1");

        Assert.Equal("Value1", result);
    }

    [Fact]
    public void GetString_NonExistingKey_ReturnsDefault()
    {
        var ini = new NiniFile();

        var result = ini.GetString("Section1", "Key1", "DefaultValue");

        Assert.Equal("DefaultValue", result);
    }

    [Fact]
    public void GetString_NonExistingSection_ReturnsDefault()
    {
        var ini = new NiniFile();

        var result = ini.GetString("NonExisting", "Key1", "DefaultValue");

        Assert.Equal("DefaultValue", result);
    }

    [Fact]
    public void RemoveValue_RemovesKey()
    {
        var ini = new NiniFile();
        ini.SetValue("Section1", "Key1", "Value1");

        ini.RemoveValue("Section1", "Key1");

        Assert.Null(ini.GetString("Section1", "Key1"));
    }

    // Int32 typed access tests

    [Fact]
    public void GetInt32_ValidValue_ReturnsCorrectInt()
    {
        var ini = new NiniFile();
        ini.SetInt32("Section1", "Port", 5432);

        var result = ini.GetInt32("Section1", "Port");

        Assert.Equal(5432, result);
    }

    [Fact]
    public void GetInt32_InvalidValue_ReturnsDefault()
    {
        var ini = new NiniFile();
        ini.SetValue("Section1", "Port", "NotANumber");

        var result = ini.GetInt32("Section1", "Port", defaultValue: 9999);

        Assert.Equal(9999, result);
    }

    [Fact]
    public void GetInt32_NonExistingKey_ReturnsDefault()
    {
        var ini = new NiniFile();

        var result = ini.GetInt32("Section1", "Port", defaultValue: 8080);

        Assert.Equal(8080, result);
    }

    // Double typed access tests (culture-critical)

    [Fact]
    public void GetDouble_ValidValue_ReturnsCorrectDouble()
    {
        var ini = new NiniFile();
        ini.SetDouble("Section1", "Timeout", 30.5);

        var result = ini.GetDouble("Section1", "Timeout");

        Assert.Equal(30.5, result, precision: 1);
    }

    [Fact]
    public void GetDouble_UsesInvariantCulture_DecimalPoint()
    {
        var ini = NiniFile.Parse("[Section1]\nTimeout: 30.5");

        var result = ini.GetDouble("Section1", "Timeout");

        Assert.Equal(30.5, result, precision: 1);
    }

    [Fact]
    public void SetDouble_UsesInvariantCulture_DecimalPoint()
    {
        var ini = new NiniFile();
        ini.SetDouble("Section1", "Timeout", 30.5);

        var text = ini.ToString();

        Assert.Contains("30.5", text);  // Decimal point, not comma
        Assert.DoesNotContain("30,5", text);  // German format not used
    }

    // Boolean typed access tests

    [Fact]
    public void GetBool_ValidValue_ReturnsCorrectBool()
    {
        var ini = new NiniFile();
        ini.SetBool("Section1", "Enabled", true);

        var result = ini.GetBool("Section1", "Enabled");

        Assert.True(result);
    }

    [Fact]
    public void GetBool_InvalidValue_ReturnsDefault()
    {
        var ini = new NiniFile();
        ini.SetValue("Section1", "Enabled", "NotABool");

        var result = ini.GetBool("Section1", "Enabled", defaultValue: true);

        Assert.True(result);
    }

    // DateTime typed access tests

    [Fact]
    public void GetDateTime_ValidValue_ReturnsCorrectDateTime()
    {
        var expected = new DateTime(2025, 12, 14, 15, 30, 0, DateTimeKind.Utc);
        var ini = new NiniFile();
        ini.SetDateTime("Section1", "LastSync", expected);

        var result = ini.GetDateTime("Section1", "LastSync");

        Assert.Equal(expected.Year, result.Year);
        Assert.Equal(expected.Month, result.Month);
        Assert.Equal(expected.Day, result.Day);
        Assert.Equal(expected.Hour, result.Hour);
        Assert.Equal(expected.Minute, result.Minute);
    }

    [Fact]
    public void SetDateTime_UsesISO8601Format()
    {
        var ini = new NiniFile();
        var dateTime = new DateTime(2025, 12, 14, 15, 30, 0, DateTimeKind.Utc);
        ini.SetDateTime("Section1", "LastSync", dateTime);

        var text = ini.ToString();

        Assert.Contains("2025-12-14", text);  // ISO 8601 date format
    }

    // Guid typed access tests

    [Fact]
    public void GetGuid_ValidValue_ReturnsCorrectGuid()
    {
        var expected = Guid.NewGuid();
        var ini = new NiniFile();
        ini.SetGuid("Section1", "Id", expected);

        var result = ini.GetGuid("Section1", "Id");

        Assert.Equal(expected, result);
    }

    // File I/O tests

    [Fact]
    public void SaveAndLoad_PreservesData()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var original = new NiniFile();
            original.SetValue("Section1", "Key1", "Value1");
            original.SetInt32("Section1", "IntKey", 123);

            original.Save(tempFile);
            var loaded = NiniFile.Load(tempFile);

            Assert.Equal("Value1", loaded.GetString("Section1", "Key1"));
            Assert.Equal(123, loaded.GetInt32("Section1", "IntKey"));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // Complex scenario tests

    [Fact]
    public void ComplexScenario_MultipleTypesAndSections()
    {
        var ini = new NiniFile();

        // Database section
        ini.SetValue("Database", "Host", "localhost");
        ini.SetInt32("Database", "Port", 5432);
        ini.SetValue("Database", "Name", "mydb");

        // Features section
        ini.SetBool("Features", "AutoSave", true);
        ini.SetDouble("Features", "SaveInterval", 5.5);
        ini.SetInt32("Features", "MaxBackups", 10);

        // Metadata section
        ini.SetDateTime("Metadata", "Created", new DateTime(2025, 12, 14, 0, 0, 0, DateTimeKind.Utc));
        ini.SetGuid("Metadata", "Id", Guid.Parse("12345678-1234-1234-1234-123456789abc"));

        // Verify all values
        Assert.Equal("localhost", ini.GetString("Database", "Host"));
        Assert.Equal(5432, ini.GetInt32("Database", "Port"));
        Assert.Equal("mydb", ini.GetString("Database", "Name"));

        Assert.True(ini.GetBool("Features", "AutoSave"));
        Assert.Equal(5.5, ini.GetDouble("Features", "SaveInterval"), precision: 1);
        Assert.Equal(10, ini.GetInt32("Features", "MaxBackups"));

        Assert.Equal(2025, ini.GetDateTime("Metadata", "Created").Year);
        Assert.Equal(Guid.Parse("12345678-1234-1234-1234-123456789abc"), ini.GetGuid("Metadata", "Id"));
    }

    [Fact]
    public void SetValue_NullValue_ThrowsArgumentNullException()
    {
        var ini = new NiniFile();
        var ex = Assert.Throws<ArgumentNullException>(() => ini.SetValue("Section", "Key", null!));
        Assert.Contains("cannot be null", ex.Message);
        Assert.Contains("Use string.Empty", ex.Message);
    }

    [Fact]
    public void SetValue_EmptyValue_Works()
    {
        var ini = new NiniFile();
        ini.SetValue("Section", "Key", string.Empty);
        Assert.Equal(string.Empty, ini.GetString("Section", "Key"));
    }

    // Case-insensitivity tests

    [Fact]
    public void Parse_CaseInsensitiveSections_MergesDifferentCase()
    {
        var input = @"[Database]
Host: localhost

[database]
Port: 5432

[DATABASE]
Name: mydb";

        var ini = NiniFile.Parse(input);

        // All three should be treated as same section
        Assert.True(ini.HasSection("Database"));
        Assert.True(ini.HasSection("database"));
        Assert.True(ini.HasSection("DATABASE"));

        // All keys should be in the same section
        Assert.Equal("localhost", ini.GetString("Database", "Host"));
        Assert.Equal(5432, ini.GetInt32("database", "Port"));
        Assert.Equal("mydb", ini.GetString("DATABASE", "Name"));
    }

    [Fact]
    public void Parse_CaseInsensitiveKeys_ThrowsDuplicate()
    {
        // Keys that differ only in case are duplicates
        var input = @"[Database]
Host: first
host: second
HOST: third";

        var ex = Assert.Throws<ArgumentException>(() => NiniFile.Parse(input));
        Assert.Contains("duplicate key", ex.Message);
    }

    [Fact]
    public void GetString_CaseInsensitiveSection_AnyCase()
    {
        var ini = new NiniFile();
        ini.SetValue("Database", "Host", "localhost");

        // Can access with any casing
        Assert.Equal("localhost", ini.GetString("database", "Host"));
        Assert.Equal("localhost", ini.GetString("DATABASE", "Host"));
        Assert.Equal("localhost", ini.GetString("DataBase", "Host"));
    }

    [Fact]
    public void GetString_CaseInsensitiveKey_AnyCase()
    {
        var ini = new NiniFile();
        ini.SetValue("Database", "Host", "localhost");

        // Can access with any key casing
        Assert.Equal("localhost", ini.GetString("Database", "host"));
        Assert.Equal("localhost", ini.GetString("Database", "HOST"));
        Assert.Equal("localhost", ini.GetString("Database", "HoSt"));
    }

    [Fact]
    public void HasSection_CaseInsensitive()
    {
        var ini = new NiniFile();
        ini.SetValue("Database", "Host", "localhost");

        Assert.True(ini.HasSection("Database"));
        Assert.True(ini.HasSection("database"));
        Assert.True(ini.HasSection("DATABASE"));
        Assert.True(ini.HasSection("DaTaBaSe"));
    }

    [Fact]
    public void SetValue_CaseInsensitiveSectionAndKey_UpdatesSameEntry()
    {
        var ini = new NiniFile();
        ini.SetValue("Database", "Host", "localhost");
        ini.SetValue("database", "HOST", "remotehost");

        // Should have updated the same entry
        Assert.Equal("remotehost", ini.GetString("Database", "Host"));
        Assert.Equal("remotehost", ini.GetString("database", "host"));
    }

    [Fact]
    public void RemoveSection_CaseInsensitive()
    {
        var ini = new NiniFile();
        ini.SetValue("Database", "Host", "localhost");

        ini.RemoveSection("database");  // Different case

        Assert.False(ini.HasSection("Database"));
        Assert.False(ini.HasSection("database"));
    }

    [Fact]
    public void RemoveValue_CaseInsensitive()
    {
        var ini = new NiniFile();
        ini.SetValue("Database", "Host", "localhost");

        ini.RemoveValue("database", "HOST");  // Both different case

        Assert.Null(ini.GetString("Database", "Host"));
    }

    // Mixed marker style tests

    [Fact]
    public void Parse_MixedMarkerStyles_ParsesBoth()
    {
        var input = @"[IniBracketSection]
Key1: Value1

@AtPrefixSection
Key2: Value2";

        var ini = NiniFile.Parse(input);

        Assert.True(ini.HasSection("IniBracketSection"));
        Assert.True(ini.HasSection("AtPrefixSection"));
        Assert.Equal("Value1", ini.GetString("IniBracketSection", "Key1"));
        Assert.Equal("Value2", ini.GetString("AtPrefixSection", "Key2"));
    }

    [Fact]
    public void Parse_MixedMarkersWithPreamble_ParsesAll()
    {
        var input = @"GlobalKey: GlobalValue

[Section1]
Key1: Value1

@Section2
Key2: Value2";

        var ini = NiniFile.Parse(input);

        Assert.Equal("GlobalValue", ini.GetString("", "GlobalKey"));
        Assert.Equal("Value1", ini.GetString("Section1", "Key1"));
        Assert.Equal("Value2", ini.GetString("Section2", "Key2"));
    }

    [Fact]
    public void Parse_AutoDetectsBothStyles_RoundTripsWithOutputStyle()
    {
        var input = @"[Database]
Host: localhost

@Features
AutoSave: true";

        var ini = NiniFile.Parse(input, outputMarkerStyle: NiniSectionMarkerStyle.AtPrefix);
        var output = ini.ToString();

        // Output should use @ style for both (output marker style)
        Assert.Contains("@Database", output);
        Assert.Contains("@Features", output);
        Assert.DoesNotContain("[Database]", output);
        Assert.DoesNotContain("[Features]", output);
    }
}

