using Nekote.Text;

namespace Nekote.Tests.Text;

public class NiniKeyValueParserTests
{
    [Fact]
    public void Parse_EmptyString_ReturnsEmptyDictionary()
    {
        var result = NiniKeyValueParser.Parse("");
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_SimpleKeyValue_ParsesCorrectly()
    {
        var input = "key1: value1\nkey2: value2";
        var result = NiniKeyValueParser.Parse(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void Parse_MultilineValue_UnescapesCorrectly()
    {
        var input = "description: Line 1\\nLine 2\\nLine 3";
        var result = NiniKeyValueParser.Parse(input);

        Assert.Single(result);
        Assert.Equal("Line 1\nLine 2\nLine 3", result["description"]);
    }

    [Fact]
    public void Parse_EmptyValue_HandlesCorrectly()
    {
        var input = "key:";
        var result = NiniKeyValueParser.Parse(input);

        Assert.Single(result);
        Assert.Equal("", result["key"]);
    }

    [Fact]
    public void Parse_ValueWithColon_ParsesCorrectly()
    {
        var input = "url: https://example.com";
        var result = NiniKeyValueParser.Parse(input);

        Assert.Single(result);
        Assert.Equal("https://example.com", result["url"]);
    }

    [Fact]
    public void Parse_ValueWithSpaces_ParsesCorrectly()
    {
        // Value can have spaces (value portion is trimmed)
        var input = "key:  value with spaces  ";
        var result = NiniKeyValueParser.Parse(input);

        Assert.Single(result);
        Assert.Equal("value with spaces", result["key"]);
    }

    [Fact]
    public void Parse_EmptyLines_SkipsEmptyLines()
    {
        var input = "key1: value1\n\n\nkey2: value2";
        var result = NiniKeyValueParser.Parse(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void Parse_CommentLines_SkipsComments()
    {
        var input = "# This is a comment\nkey1: value1\n# Another comment\nkey2: value2";
        var result = NiniKeyValueParser.Parse(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void Parse_SlashCommentLines_SkipsComments()
    {
        var input = "// This is a comment\nkey1: value1\n// Another comment\nkey2: value2";
        var result = NiniKeyValueParser.Parse(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void Parse_MixedComments_SkipsBothTypes()
    {
        var input = "# Hash comment\nkey1: value1\n// Slash comment\nkey2: value2";
        var result = NiniKeyValueParser.Parse(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void Parse_MissingColon_ThrowsArgumentException()
    {
        var input = "invalid line without colon";
        var ex = Assert.Throws<ArgumentException>(() => NiniKeyValueParser.Parse(input));
        Assert.Contains("Line 1", ex.Message);
        Assert.Contains("missing colon", ex.Message);
    }

    [Fact]
    public void Parse_EmptyKey_ThrowsArgumentException()
    {
        var input = ": value without key";
        var ex = Assert.Throws<ArgumentException>(() => NiniKeyValueParser.Parse(input));
        Assert.Contains("Line 1", ex.Message);
        Assert.Contains("empty key", ex.Message);
    }

    [Fact]
    public void Parse_DuplicateKey_ThrowsArgumentException()
    {
        var input = "key: value1\nkey: value2";
        var ex = Assert.Throws<ArgumentException>(() => NiniKeyValueParser.Parse(input));
        Assert.Contains("Line 2", ex.Message);
        Assert.Contains("duplicate key", ex.Message);
    }

    [Fact]
    public void Parse_WindowsLineEndings_ParsesCorrectly()
    {
        var input = "key1: value1\r\nkey2: value2\r\n";
        var result = NiniKeyValueParser.Parse(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void Parse_EscapedBackslashes_UnescapesCorrectly()
    {
        var input = "path: C:\\\\Users\\\\Name";
        var result = NiniKeyValueParser.Parse(input);

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
        var result = NiniKeyValueParser.Parse(input);

        Assert.Equal(4, result.Count);
        Assert.Equal("sk-proj-abc123", result["openai-key"]);
        Assert.Equal("AIza-xyz789", result["gemini-key"]);
        Assert.Equal("This is a test\nwith multiple lines\nand tabs:\there", result["description"]);
        Assert.Equal("C:\\Users\\Test\\Documents", result["path"]);
    }

    [Fact]
    public void Parse_KeyWithTrailingWhitespaceBeforeColon_Throws()
    {
        // Key extracted is "key " (trailing space before colon)
        var input = "key : value";
        var ex = Assert.Throws<ArgumentException>(() => NiniKeyValueParser.Parse(input));
        Assert.Contains("cannot end with whitespace", ex.Message);
    }

    [Fact]
    public void Parse_KeyWithLeadingWhitespace_Throws()
    {
        // Line starting with whitespace means key has leading whitespace
        var input = " key: value";
        var ex = Assert.Throws<ArgumentException>(() => NiniKeyValueParser.Parse(input));
        Assert.Contains("cannot start with whitespace", ex.Message);
    }

    [Fact]
    public void Parse_IndentedLine_Throws()
    {
        // Line-level indentation is NOT supported - keys must start at column 0
        var input = "    key: value";
        var ex = Assert.Throws<ArgumentException>(() => NiniKeyValueParser.Parse(input));
        Assert.Contains("cannot start with whitespace", ex.Message);
    }

    [Fact]
    public void Parse_IEnumerableLines_ParsesCorrectly()
    {
        // Test IEnumerable<string> overload with List<string>
        var lines = new List<string> { "key1: value1", "key2: value2" };
        var result = NiniKeyValueParser.Parse(lines);

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void Parse_IEnumerableLinesWithComments_SkipsComments()
    {
        // Test IEnumerable<string> overload with LINQ query
        var lines = new[] { "# comment", "key: value", "// comment2" }.Where(l => l != null);
        var result = NiniKeyValueParser.Parse(lines);

        Assert.Single(result);
        Assert.Equal("value", result["key"]);
    }

    [Fact]
    public void Parse_CaseInsensitiveKeys_TreatsSameKeyDifferentCase()
    {
        // Keys are case-insensitive, so "Key" and "key" are the same
        var input = "Key: value1\nkey: value2";
        var ex = Assert.Throws<ArgumentException>(() => NiniKeyValueParser.Parse(input));
        Assert.Contains("duplicate key", ex.Message);
    }

    [Fact]
    public void Parse_CaseInsensitiveKeys_Lookup()
    {
        // Can lookup keys with different casing
        var input = "Host: localhost\nPort: 5432";
        var result = NiniKeyValueParser.Parse(input);

        Assert.Equal("localhost", result["Host"]);
        Assert.Equal("localhost", result["host"]);
        Assert.Equal("localhost", result["HOST"]);
        Assert.Equal("5432", result["Port"]);
        Assert.Equal("5432", result["port"]);
    }

    [Fact]
    public void Parse_CaseInsensitiveKeys_MixedCase()
    {
        // Last occurrence wins when keys differ only in case
        var input = "dataBase: value1";
        var result = NiniKeyValueParser.Parse(input);

        // Can be accessed with any casing
        Assert.Equal("value1", result["dataBase"]);
        Assert.Equal("value1", result["database"]);
        Assert.Equal("value1", result["DATABASE"]);
        Assert.Equal("value1", result["DataBase"]);
    }
}



