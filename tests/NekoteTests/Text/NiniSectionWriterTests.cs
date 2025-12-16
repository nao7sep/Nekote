using Nekote.Text;

namespace NekoteTests.Text;

public class NiniSectionWriterTests
{
    [Fact]
    public void Write_EmptySections_ReturnsEmptyString()
    {
        var sections = Array.Empty<NiniSection>();
        var result = NiniSectionWriter.Write(sections);
        Assert.Empty(result);
    }

    [Fact]
    public void Write_NullSections_ReturnsEmptyString()
    {
        var result = NiniSectionWriter.Write((IEnumerable<NiniSection>)null!);
        Assert.Empty(result);
    }

    [Fact]
    public void Write_PreambleOnly_WritesWithoutMarker()
    {
        var sections = new[]
        {
            new NiniSection
            {
                Name = "",
                Marker = NiniSectionMarkerStyle.None,
                KeyValues = new Dictionary<string, string>
                {
                    ["Key1"] = "Value1",
                    ["Key2"] = "Value2"
                }
            }
        };

        var result = NiniSectionWriter.Write(sections);

        Assert.Contains("Key1: Value1", result);
        Assert.Contains("Key2: Value2", result);
        Assert.DoesNotContain("@", result);
        Assert.DoesNotContain("[", result);
    }

    [Fact]
    public void Write_SingleNamedSection_WithAtPrefix()
    {
        var sections = new[]
        {
            new NiniSection
            {
                Name = "Database",
                Marker = NiniSectionMarkerStyle.AtPrefix,
                KeyValues = new Dictionary<string, string>
                {
                    ["Host"] = "localhost",
                    ["Port"] = "5432"
                }
            }
        };

        var result = NiniSectionWriter.Write(sections);

        Assert.Contains("@Database", result);
        Assert.Contains("Host: localhost", result);
        Assert.Contains("Port: 5432", result);
    }

    [Fact]
    public void Write_SingleNamedSection_WithIniBrackets()
    {
        var sections = new[]
        {
            new NiniSection
            {
                Name = "Database",
                Marker = NiniSectionMarkerStyle.IniBrackets,
                KeyValues = new Dictionary<string, string>
                {
                    ["Host"] = "localhost",
                    ["Port"] = "5432"
                }
            }
        };

        var result = NiniSectionWriter.Write(sections, NiniOptions.Default with { MarkerStyle = NiniSectionMarkerStyle.IniBrackets });

        Assert.Contains("[Database]", result);
        Assert.Contains("Host: localhost", result);
        Assert.Contains("Port: 5432", result);
    }

    [Fact]
    public void Write_PreambleAndNamedSection_WritesPreambleFirst()
    {
        var sections = new[]
        {
            new NiniSection
            {
                Name = "Database",
                Marker = NiniSectionMarkerStyle.AtPrefix,
                KeyValues = new Dictionary<string, string> { ["Host"] = "localhost" }
            },
            new NiniSection
            {
                Name = "",
                Marker = NiniSectionMarkerStyle.None,
                KeyValues = new Dictionary<string, string> { ["GlobalKey"] = "GlobalValue" }
            }
        };

        var result = NiniSectionWriter.Write(sections);

        var globalIndex = result.IndexOf("GlobalKey");
        var databaseIndex = result.IndexOf("@Database");

        Assert.True(globalIndex < databaseIndex, "Preamble should appear before named sections");
    }

    [Fact]
    public void Write_MultipleSections_SeparatesWithBlankLines()
    {
        var sections = new[]
        {
            new NiniSection
            {
                Name = "",
                Marker = NiniSectionMarkerStyle.None,
                KeyValues = new Dictionary<string, string> { ["Global"] = "Value" }
            },
            new NiniSection
            {
                Name = "Section1",
                Marker = NiniSectionMarkerStyle.AtPrefix,
                KeyValues = new Dictionary<string, string> { ["Key1"] = "Value1" }
            },
            new NiniSection
            {
                Name = "Section2",
                Marker = NiniSectionMarkerStyle.AtPrefix,
                KeyValues = new Dictionary<string, string> { ["Key2"] = "Value2" }
            }
        };

        var result = NiniSectionWriter.Write(sections);

        // Should have double newlines between paragraphs (CRLF on Windows)
        Assert.True(result.Contains("\n\n") || result.Contains("\r\n\r\n"), "Paragraphs should be separated by blank lines");
    }

    [Fact]
    public void Write_EmptyNamedSection_AddsComment()
    {
        var sections = new[]
        {
            new NiniSection
            {
                Name = "EmptySection",
                Marker = NiniSectionMarkerStyle.AtPrefix,
                KeyValues = new Dictionary<string, string>()
            }
        };

        var result = NiniSectionWriter.Write(sections);

        Assert.Contains("@EmptySection", result);
        Assert.Contains("# (empty section)", result);
    }

    [Fact]
    public void Write_WithSortSections_SortsAlphabetically()
    {
        var sections = new[]
        {
            new NiniSection
            {
                Name = "Zebra",
                Marker = NiniSectionMarkerStyle.AtPrefix,
                KeyValues = new Dictionary<string, string> { ["Key"] = "Value" }
            },
            new NiniSection
            {
                Name = "Apple",
                Marker = NiniSectionMarkerStyle.AtPrefix,
                KeyValues = new Dictionary<string, string> { ["Key"] = "Value" }
            },
            new NiniSection
            {
                Name = "Middle",
                Marker = NiniSectionMarkerStyle.AtPrefix,
                KeyValues = new Dictionary<string, string> { ["Key"] = "Value" }
            }
        };

        var result = NiniSectionWriter.Write(sections, NiniOptions.Default with { SortSections = true });

        var appleIndex = result.IndexOf("@Apple");
        var middleIndex = result.IndexOf("@Middle");
        var zebraIndex = result.IndexOf("@Zebra");

        Assert.True(appleIndex < middleIndex && middleIndex < zebraIndex, "Sections should be sorted alphabetically");
    }

    [Fact]
    public void Write_WithNamedSectionAndMarkerStyleNone_Throws()
    {
        var sections = new[]
        {
            new NiniSection
            {
                Name = "Database",
                Marker = NiniSectionMarkerStyle.AtPrefix,
                KeyValues = new Dictionary<string, string> { ["Host"] = "localhost" }
            }
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            NiniSectionWriter.Write(sections, NiniOptions.Default with { MarkerStyle = NiniSectionMarkerStyle.None }));

        Assert.Contains("Cannot write named section 'Database'", ex.Message);
        Assert.Contains("MarkerStyle.None", ex.Message);
    }

    [Fact]
    public void Write_FromDictionary_ConvertsCorrectly()
    {
        var dict = new Dictionary<string, Dictionary<string, string>>
        {
            [""] = new Dictionary<string, string> { ["Global"] = "Value" },
            ["Database"] = new Dictionary<string, string> { ["Host"] = "localhost" }
        };

        var result = NiniSectionWriter.Write(dict);

        Assert.Contains("Global: Value", result);
        Assert.Contains("@Database", result);
        Assert.Contains("Host: localhost", result);
    }

    [Fact]
    public void Write_FromEmptyDictionary_ReturnsEmptyString()
    {
        var dict = new Dictionary<string, Dictionary<string, string>>();
        var result = NiniSectionWriter.Write(dict);
        Assert.Empty(result);
    }

    [Fact]
    public void Write_FromNullDictionary_ReturnsEmptyString()
    {
        var result = NiniSectionWriter.Write((Dictionary<string, Dictionary<string, string>>)null!);
        Assert.Empty(result);
    }

    [Fact]
    public void Write_WithCustomSeparator_UsesCorrectSeparator()
    {
        var sections = new[]
        {
            new NiniSection
            {
                Name = "",
                Marker = NiniSectionMarkerStyle.None,
                KeyValues = new Dictionary<string, string> { ["Key"] = "Value" }
            }
        };

        var result = NiniSectionWriter.Write(sections, NiniOptions.taskKiller);

        Assert.Contains("Key:Value", result);
        Assert.DoesNotContain(": ", result);
    }

    [Fact]
    public void Write_WithSortKeys_SortsKeysAlphabetically()
    {
        var sections = new[]
        {
            new NiniSection
            {
                Name = "Section",
                Marker = NiniSectionMarkerStyle.AtPrefix,
                KeyValues = new Dictionary<string, string>
                {
                    ["Zebra"] = "last",
                    ["Apple"] = "first",
                    ["Middle"] = "mid"
                }
            }
        };

        var result = NiniSectionWriter.Write(sections, NiniOptions.Default with { SortKeys = true });

        var appleIndex = result.IndexOf("Apple:");
        var middleIndex = result.IndexOf("Middle:");
        var zebraIndex = result.IndexOf("Zebra:");

        Assert.True(appleIndex < middleIndex && middleIndex < zebraIndex, "Keys should be sorted alphabetically");
    }

    [Fact]
    public void Write_RoundTrip_PreservesData()
    {
        // Create original sections
        var original = new[]
        {
            new NiniSection
            {
                Name = "",
                Marker = NiniSectionMarkerStyle.None,
                KeyValues = new Dictionary<string, string> { ["GlobalKey"] = "GlobalValue" }
            },
            new NiniSection
            {
                Name = "Database",
                Marker = NiniSectionMarkerStyle.AtPrefix,
                KeyValues = new Dictionary<string, string>
                {
                    ["Host"] = "localhost",
                    ["Port"] = "5432"
                }
            }
        };

        // Write to string
        var text = NiniSectionWriter.Write(original);

        // Parse back
        var parsed = NiniSectionParser.Parse(text);

        // Verify
        Assert.Equal(2, parsed.Length);
        Assert.Equal("", parsed[0].Name);
        Assert.Equal("GlobalValue", parsed[0].KeyValues["GlobalKey"]);
        Assert.Equal("Database", parsed[1].Name);
        Assert.Equal("localhost", parsed[1].KeyValues["Host"]);
        Assert.Equal("5432", parsed[1].KeyValues["Port"]);
    }
}
