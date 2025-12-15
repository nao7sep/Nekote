using Nekote.Text;

namespace Nekote.Tests.Text;

public class NiniSectionParserTests
{
    #region INI Brackets Style Tests

    [Fact]
    public void Parse_IniBrackets_EmptyString_ReturnsEmptyArray()
    {
        var result = NiniSectionParser.Parse("");
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_IniBrackets_NoSections_ReturnsPreamble()
    {
        var input = "key1: value1\nkey2: value2";
        var result = NiniSectionParser.Parse(input);

        Assert.Single(result);
        Assert.Equal(NiniSectionMarkerStyle.None, result[0].Marker);
        Assert.Equal("", result[0].Name);
        Assert.Equal(2, result[0].KeyValues.Count);
        Assert.Equal("value1", result[0].KeyValues["key1"]);
        Assert.Equal("value2", result[0].KeyValues["key2"]);
    }

    [Fact]
    public void Parse_IniBrackets_SingleSection_ParsesCorrectly()
    {
        var input = "[Section1]\nkey1: value1\nkey2: value2";
        var result = NiniSectionParser.Parse(input);

        Assert.Single(result);
        Assert.Equal(NiniSectionMarkerStyle.IniBrackets, result[0].Marker);
        Assert.Equal("Section1", result[0].Name);
        Assert.Equal(2, result[0].KeyValues.Count);
        Assert.Equal("value1", result[0].KeyValues["key1"]);
    }

    [Fact]
    public void Parse_IniBrackets_MultipleSections_ParsesCorrectly()
    {
        var input = @"[OpenAI]
key: sk-proj-abc
model: gpt-4

[Gemini]
key: AIza-xyz
model: gemini-pro";
        var result = NiniSectionParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("OpenAI", result[0].Name);
        Assert.Equal("sk-proj-abc", result[0].KeyValues["key"]);
        Assert.Equal("Gemini", result[1].Name);
        Assert.Equal("gemini-pro", result[1].KeyValues["model"]);
    }

    [Fact]
    public void Parse_IniBrackets_WithPreamble_ParsesCorrectly()
    {
        var input = @"preamble-key: preamble-value

[Section1]
key1: value1";
        var result = NiniSectionParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("", result[0].Name);
        Assert.Equal("preamble-value", result[0].KeyValues["preamble-key"]);
        Assert.Equal("Section1", result[1].Name);
        Assert.Equal("value1", result[1].KeyValues["key1"]);
    }

    [Fact]
    public void Parse_IniBrackets_EmptySection_PreservesSection()
    {
        var input = "[Section1]\n\n[Section2]\nkey: value";
        var result = NiniSectionParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("Section1", result[0].Name);
        Assert.Empty(result[0].KeyValues);
        Assert.Equal("Section2", result[1].Name);
        Assert.Single(result[1].KeyValues);
    }

    [Fact]
    public void Parse_IniBrackets_WithComments_SkipsComments()
    {
        var input = @"# This is a comment

[Section1]
// Another comment
key: value";
        var result = NiniSectionParser.Parse(input);

        // Comment-only preamble creates no NiniSection (no keys, no explicit marker)
        Assert.Single(result);
        Assert.Equal("Section1", result[0].Name);
        Assert.Single(result[0].KeyValues);
        Assert.Equal("value", result[0].KeyValues["key"]);
    }

    [Fact]
    public void Parse_IniBrackets_SectionNameWithInternalSpaces_ParsesCorrectly()
    {
        // Internal spaces are fine - only leading/trailing are problematic
        var input = "[NiniSection Name]\nkey: value";
        var result = NiniSectionParser.Parse(input);

        Assert.Single(result);
        Assert.Equal("NiniSection Name", result[0].Name);
    }

    [Fact]
    public void Parse_IniBrackets_PreambleAndSection_ParsesCorrectly()
    {
        var input = "key: value\n\n[Section1]\nkey1: value1";
        var result = NiniSectionParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("", result[0].Name);
        Assert.Equal("value", result[0].KeyValues["key"]);
        Assert.Equal("Section1", result[1].Name);
        Assert.Equal("value1", result[1].KeyValues["key1"]);
    }

    #endregion

    #region At-Prefix Style Tests

    [Fact]
    public void Parse_AtPrefix_EmptyString_ReturnsEmptyArray()
    {
        var result = NiniSectionParser.Parse("");
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_AtPrefix_SingleSection_ParsesCorrectly()
    {
        var input = "@Section1\nkey1: value1\nkey2: value2";
        var result = NiniSectionParser.Parse(input);

        Assert.Single(result);
        Assert.Equal(NiniSectionMarkerStyle.AtPrefix, result[0].Marker);
        Assert.Equal("Section1", result[0].Name);
        Assert.Equal(2, result[0].KeyValues.Count);
    }

    [Fact]
    public void Parse_AtPrefix_MultipleSections_ParsesCorrectly()
    {
        var input = @"@OpenAI
key: sk-proj-abc
model: gpt-4

@Gemini
key: AIza-xyz";
        var result = NiniSectionParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("OpenAI", result[0].Name);
        Assert.Equal("Gemini", result[1].Name);
    }

    [Fact]
    public void Parse_AtPrefix_WithPreamble_ParsesCorrectly()
    {
        var input = "intro: text\n\n@Section1\nkey: value";
        var result = NiniSectionParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("", result[0].Name);
        Assert.Equal("text", result[0].KeyValues["intro"]);
        Assert.Equal("Section1", result[1].Name);
    }

    [Fact]
    public void Parse_AtPrefix_SectionNameWithInternalSpaces_ParsesCorrectly()
    {
        // Internal spaces are fine - only leading/trailing are problematic
        var input = "@NiniSection Name\nkey: value";
        var result = NiniSectionParser.Parse(input);

        Assert.Single(result);
        Assert.Equal("NiniSection Name", result[0].Name);
    }

    [Fact]
    public void Parse_MixedMarkerStyles_ParsesCorrectly()
    {
        // Both [bracket] and @at-prefix markers can coexist
        var input = @"[IniSection]
key1: value1

@AtSection
key2: value2

key3: value3";
        var result = NiniSectionParser.Parse(input);

        Assert.Equal(3, result.Length);
        Assert.Equal(NiniSectionMarkerStyle.IniBrackets, result[0].Marker);
        Assert.Equal("IniSection", result[0].Name);
        Assert.Equal(NiniSectionMarkerStyle.AtPrefix, result[1].Marker);
        Assert.Equal("AtSection", result[1].Name);
        Assert.Equal(NiniSectionMarkerStyle.None, result[2].Marker);
        Assert.Equal("", result[2].Name);
    }

    #endregion

    #region GetSection Tests

    [Fact]
    public void GetSection_FindsExistingSection()
    {
        var input = "[Section1]\nkey1: value1\n\n[Section2]\nkey2: value2";
        var sections = NiniSectionParser.Parse(input);

        var NiniSection = NiniSectionParser.GetSection(sections, "Section2");

        Assert.NotNull(NiniSection);
        Assert.Equal("Section2", NiniSection.Name);
        Assert.Equal("value2", NiniSection.KeyValues["key2"]);
    }

    [Fact]
    public void GetSection_MissingSection_ReturnsNull()
    {
        var input = "[Section1]\nkey1: value1";
        var sections = NiniSectionParser.Parse(input);

        var NiniSection = NiniSectionParser.GetSection(sections, "NonExistent");

        Assert.Null(NiniSection);
    }

    [Fact]
    public void GetSection_Preamble_FindsByEmptyString()
    {
        var input = "key: value\n\n[Section1]\nkey1: value1";
        var sections = NiniSectionParser.Parse(input);

        var NiniSection = NiniSectionParser.GetSection(sections, "");

        Assert.NotNull(NiniSection);
        Assert.Equal("", NiniSection.Name);
        Assert.Equal("value", NiniSection.KeyValues["key"]);
    }

    [Fact]
    public void GetSection_FindsSectionCaseInsensitively()
    {
        var input = "[MySection]\nkey: value";
        var sections = NiniSectionParser.Parse(input);

        // Should find "MySection" even if we ask for "mysection"
        var section = NiniSectionParser.GetSection(sections, "mysection");

        Assert.NotNull(section);
        Assert.Equal("MySection", section.Name);
        Assert.Equal("value", section.KeyValues["key"]);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Parse_OnlyWhitespace_ReturnsEmptyArray()
    {
        var result = NiniSectionParser.Parse("   \n\n   ");
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_IniBrackets_MalformedMarker_ThrowsException()
    {
        var input = "[Unclosed\nkey: value";

        // [Unclosed is not a valid section marker, treated as content
        // NiniKeyValueParser will reject it as invalid key:value format
        Assert.Throws<ArgumentException>(() => NiniSectionParser.Parse(input));
    }

    [Fact]
    public void Parse_AtPrefix_AtSignOnly_ThrowsException()
    {
        var input = "@\nkey: value";

        // @ alone (too short) is not a valid section marker, treated as content
        // NiniKeyValueParser will reject it as invalid key:value format
        Assert.Throws<ArgumentException>(() => NiniSectionParser.Parse(input));
    }

    [Fact]
    public void Parse_ComplexRealWorldExample_ParsesCorrectly()
    {
        var input = @"# Global settings
version: 1.0

[OpenAI]
key: sk-proj-abc123
model: gpt-4
// This is the primary provider

[Gemini]
key: AIza-xyz789
model: gemini-pro

[Settings]
timeout: 30
retries: 3";
        var result = NiniSectionParser.Parse(input);

        Assert.Equal(4, result.Length);
        Assert.Equal("", result[0].Name);
        Assert.Equal("1.0", result[0].KeyValues["version"]);
        Assert.Equal("OpenAI", result[1].Name);
        Assert.Equal("gpt-4", result[1].KeyValues["model"]);
        Assert.Equal("Gemini", result[2].Name);
        Assert.Equal("Settings", result[3].Name);
        Assert.Equal("30", result[3].KeyValues["timeout"]);
    }

    [Fact]
    public void Parse_IniBrackets_EmptySectionName_Throws()
    {
        var input = "[]\nkey: value";
        var ex = Assert.Throws<ArgumentException>(() => NiniSectionParser.Parse(input));
        Assert.Contains("cannot be empty", ex.Message);
    }

    [Fact]
    public void Parse_IniBrackets_WhitespaceOnlySectionName_Throws()
    {
        var input = "[   ]\nkey: value";
        var ex = Assert.Throws<ArgumentException>(() => NiniSectionParser.Parse(input));
        Assert.Contains("cannot be empty", ex.Message);
    }

    [Fact]
    public void Parse_AtPrefix_EmptySectionName_Throws()
    {
        var input = "@\nkey: value";
        var ex = Assert.Throws<ArgumentException>(() => NiniSectionParser.Parse(input));
        Assert.Contains("cannot be empty", ex.Message);
    }

    [Fact]
    public void Parse_AtPrefix_WhitespaceOnlySectionName_Throws()
    {
        var input = "@   \nkey: value";
        var ex = Assert.Throws<ArgumentException>(() => NiniSectionParser.Parse(input));
        Assert.Contains("cannot be empty", ex.Message);
    }

    #endregion

    #region Mixed Marker Style Tests

    [Fact]
    public void Parse_MixedStyles_IniBracketsAndAtPrefix()
    {
        var input = @"[Section1]
key1: value1

@Section2
key2: value2";

        var result = NiniSectionParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal(NiniSectionMarkerStyle.IniBrackets, result[0].Marker);
        Assert.Equal("Section1", result[0].Name);
        Assert.Equal("value1", result[0].KeyValues["key1"]);

        Assert.Equal(NiniSectionMarkerStyle.AtPrefix, result[1].Marker);
        Assert.Equal("Section2", result[1].Name);
        Assert.Equal("value2", result[1].KeyValues["key2"]);
    }

    [Fact]
    public void Parse_MixedStyles_WithPreamble()
    {
        var input = @"preamble: value

[IniSection]
inikey: inivalue

@AtSection
atkey: atvalue";

        var result = NiniSectionParser.Parse(input);

        Assert.Equal(3, result.Length);
        Assert.Equal(NiniSectionMarkerStyle.None, result[0].Marker);
        Assert.Equal("", result[0].Name);
        Assert.Equal("value", result[0].KeyValues["preamble"]);

        Assert.Equal(NiniSectionMarkerStyle.IniBrackets, result[1].Marker);
        Assert.Equal("IniSection", result[1].Name);

        Assert.Equal(NiniSectionMarkerStyle.AtPrefix, result[2].Marker);
        Assert.Equal("AtSection", result[2].Name);
    }

    [Fact]
    public void Parse_MixedStyles_AlternatingPatterns()
    {
        var input = @"@First
key1: value1

[Second]
key2: value2

@Third
key3: value3

[Fourth]
key4: value4";

        var result = NiniSectionParser.Parse(input);

        Assert.Equal(4, result.Length);
        Assert.Equal(NiniSectionMarkerStyle.AtPrefix, result[0].Marker);
        Assert.Equal("First", result[0].Name);

        Assert.Equal(NiniSectionMarkerStyle.IniBrackets, result[1].Marker);
        Assert.Equal("Second", result[1].Name);

        Assert.Equal(NiniSectionMarkerStyle.AtPrefix, result[2].Marker);
        Assert.Equal("Third", result[2].Name);

        Assert.Equal(NiniSectionMarkerStyle.IniBrackets, result[3].Marker);
        Assert.Equal("Fourth", result[3].Name);
    }

    [Fact]
    public void Parse_MarkerProperty_ReflectsActualMarkerUsed()
    {
        var input = @"unmarked: value

[bracketed]
key1: value1

@prefixed
key2: value2";

        var result = NiniSectionParser.Parse(input);

        // Unmarked NiniSection has None marker
        Assert.Equal(NiniSectionMarkerStyle.None, result[0].Marker);

        // INI bracket NiniSection
        Assert.Equal(NiniSectionMarkerStyle.IniBrackets, result[1].Marker);

        // At-prefix NiniSection
        Assert.Equal(NiniSectionMarkerStyle.AtPrefix, result[2].Marker);
    }

    #endregion
}

