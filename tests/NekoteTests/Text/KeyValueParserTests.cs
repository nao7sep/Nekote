using Nekote.Text;

namespace Nekote.Tests.Text;

public class KeyValueParserTests
{
    [Fact]
    public void Parse_EmptyString_ReturnsEmptyDictionary()
    {
        var result = KeyValueParser.Parse("");
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_SimpleKeyValue_ParsesCorrectly()
    {
        var input = "key1: value1\nkey2: value2";
        var result = KeyValueParser.Parse(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void Parse_MultilineValue_UnescapesCorrectly()
    {
        var input = "description: Line 1\\nLine 2\\nLine 3";
        var result = KeyValueParser.Parse(input);

        Assert.Single(result);
        Assert.Equal("Line 1\nLine 2\nLine 3", result["description"]);
    }

    [Fact]
    public void Parse_EmptyValue_HandlesCorrectly()
    {
        var input = "key:";
        var result = KeyValueParser.Parse(input);

        Assert.Single(result);
        Assert.Equal("", result["key"]);
    }

    [Fact]
    public void Parse_ValueWithColon_ParsesCorrectly()
    {
        var input = "url: https://example.com";
        var result = KeyValueParser.Parse(input);

        Assert.Single(result);
        Assert.Equal("https://example.com", result["url"]);
    }

    [Fact]
    public void Parse_WhitespaceAroundKeyValue_TrimsBoth()
    {
        var input = "  key  :  value with spaces  ";
        var result = KeyValueParser.Parse(input);

        Assert.Single(result);
        Assert.Equal("value with spaces", result["key"]);
    }

    [Fact]
    public void Parse_EmptyLines_SkipsEmptyLines()
    {
        var input = "key1: value1\n\n\nkey2: value2";
        var result = KeyValueParser.Parse(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void Parse_CommentLines_SkipsComments()
    {
        var input = "# This is a comment\nkey1: value1\n# Another comment\nkey2: value2";
        var result = KeyValueParser.Parse(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void Parse_SlashCommentLines_SkipsComments()
    {
        var input = "// This is a comment\nkey1: value1\n// Another comment\nkey2: value2";
        var result = KeyValueParser.Parse(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void Parse_MixedComments_SkipsBothTypes()
    {
        var input = "# Hash comment\nkey1: value1\n// Slash comment\nkey2: value2";
        var result = KeyValueParser.Parse(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void Parse_MissingColon_ThrowsArgumentException()
    {
        var input = "invalid line without colon";
        var ex = Assert.Throws<ArgumentException>(() => KeyValueParser.Parse(input));
        Assert.Contains("Line 1", ex.Message);
        Assert.Contains("missing colon", ex.Message);
    }

    [Fact]
    public void Parse_EmptyKey_ThrowsArgumentException()
    {
        var input = ": value without key";
        var ex = Assert.Throws<ArgumentException>(() => KeyValueParser.Parse(input));
        Assert.Contains("Line 1", ex.Message);
        Assert.Contains("empty key", ex.Message);
    }

    [Fact]
    public void Parse_DuplicateKey_ThrowsArgumentException()
    {
        var input = "key: value1\nkey: value2";
        var ex = Assert.Throws<ArgumentException>(() => KeyValueParser.Parse(input));
        Assert.Contains("Line 2", ex.Message);
        Assert.Contains("duplicate key", ex.Message);
    }

    [Fact]
    public void Parse_WindowsLineEndings_ParsesCorrectly()
    {
        var input = "key1: value1\r\nkey2: value2\r\n";
        var result = KeyValueParser.Parse(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void Parse_EscapedBackslashes_UnescapesCorrectly()
    {
        var input = "path: C:\\\\Users\\\\Name";
        var result = KeyValueParser.Parse(input);

        Assert.Single(result);
        Assert.Equal("C:\\Users\\Name", result["path"]);
    }

    [Fact]
    public void Parse_ComplexExample_ParsesCorrectly()
    {
        var input = @"
# Configuration file
openai-key: sk-proj-abc123
gemini-key: AIza-xyz789

description: This is a test\nwith multiple lines\nand tabs:\there
path: C:\\Users\\Test\\Documents
";
        var result = KeyValueParser.Parse(input);

        Assert.Equal(4, result.Count);
        Assert.Equal("sk-proj-abc123", result["openai-key"]);
        Assert.Equal("AIza-xyz789", result["gemini-key"]);
        Assert.Equal("This is a test\nwith multiple lines\nand tabs:\there", result["description"]);
        Assert.Equal("C:\\Users\\Test\\Documents", result["path"]);
    }
}
