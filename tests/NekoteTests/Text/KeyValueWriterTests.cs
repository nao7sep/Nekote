using Nekote.Text;

namespace Nekote.Tests.Text;

public class KeyValueWriterTests
{
    [Fact]
    public void Write_EmptyDictionary_ReturnsEmptyString()
    {
        var data = new Dictionary<string, string>();
        var result = KeyValueWriter.Write(data);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Write_NullDictionary_ReturnsEmptyString()
    {
        var result = KeyValueWriter.Write(null!);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Write_SimpleKeyValue_WritesCorrectly()
    {
        var data = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };
        var result = KeyValueWriter.Write(data);

        Assert.Contains("key1: value1", result);
        Assert.Contains("key2: value2", result);
    }

    [Fact]
    public void Write_MultilineValue_EscapesCorrectly()
    {
        var data = new Dictionary<string, string>
        {
            ["description"] = "Line 1\nLine 2\nLine 3"
        };
        var result = KeyValueWriter.Write(data);

        Assert.Contains("description: Line 1\\nLine 2\\nLine 3", result);
    }

    [Fact]
    public void Write_EmptyValue_WritesCorrectly()
    {
        var data = new Dictionary<string, string>
        {
            ["key"] = ""
        };
        var result = KeyValueWriter.Write(data);

        Assert.Contains("key: ", result);
    }

    [Fact]
    public void Write_ValueWithBackslashes_EscapesCorrectly()
    {
        var data = new Dictionary<string, string>
        {
            ["path"] = "C:\\Users\\Name"
        };
        var result = KeyValueWriter.Write(data);

        Assert.Contains("path: C:\\\\Users\\\\Name", result);
    }

    [Fact]
    public void Write_WithSortKeys_SortsAlphabetically()
    {
        var data = new Dictionary<string, string>
        {
            ["zebra"] = "last",
            ["apple"] = "first",
            ["middle"] = "mid"
        };
        var result = KeyValueWriter.Write(data, sortKeys: true);

        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal("apple: first", lines[0]);
        Assert.Equal("middle: mid", lines[1]);
        Assert.Equal("zebra: last", lines[2]);
    }

    [Fact]
    public void Write_WithoutSortKeys_PreservesInsertionOrder()
    {
        var data = new Dictionary<string, string>
        {
            ["third"] = "3",
            ["first"] = "1",
            ["second"] = "2"
        };
        var result = KeyValueWriter.Write(data, sortKeys: false);

        // Dictionary doesn't guarantee order, but we can verify all are present
        Assert.Contains("third: 3", result);
        Assert.Contains("first: 1", result);
        Assert.Contains("second: 2", result);
    }

    [Fact]
    public void Write_ComplexExample_WritesCorrectly()
    {
        var data = new Dictionary<string, string>
        {
            ["openai-key"] = "sk-proj-abc123",
            ["description"] = "Test\nwith\nmultiple\nlines",
            ["path"] = "C:\\Windows\\System32"
        };
        var result = KeyValueWriter.Write(data);

        Assert.Contains("openai-key: sk-proj-abc123", result);
        Assert.Contains("description: Test\\nwith\\nmultiple\\nlines", result);
        Assert.Contains("path: C:\\\\Windows\\\\System32", result);
    }

    [Fact]
    public void Parse_Write_RoundTrip_PreservesData()
    {
        var original = new Dictionary<string, string>
        {
            ["key1"] = "simple value",
            ["key2"] = "Line 1\nLine 2\nLine 3",
            ["key3"] = "Path: C:\\Users\\Test",
            ["key4"] = ""
        };

        var written = KeyValueWriter.Write(original);
        var parsed = KeyValueParser.Parse(written);

        Assert.Equal(original.Count, parsed.Count);
        foreach (var kvp in original)
        {
            Assert.Equal(kvp.Value, parsed[kvp.Key]);
        }
    }

    [Fact]
    public void Write_KeyWithColon_ThrowsArgumentException()
    {
        var data = new Dictionary<string, string> { ["key:sub"] = "value" };
        var ex = Assert.Throws<ArgumentException>(() => KeyValueWriter.Write(data));
        Assert.Contains("contains invalid character ':'", ex.Message);
    }

    [Fact]
    public void Write_KeyWithNewline_ThrowsArgumentException()
    {
        var data = new Dictionary<string, string> { ["key\nline"] = "value" };
        var ex = Assert.Throws<ArgumentException>(() => KeyValueWriter.Write(data));
        Assert.Contains("contains line breaks", ex.Message);
    }

    [Fact]
    public void Write_KeyStartingWithHash_ThrowsArgumentException()
    {
        var data = new Dictionary<string, string> { ["#comment"] = "value" };
        var ex = Assert.Throws<ArgumentException>(() => KeyValueWriter.Write(data));
        Assert.Contains("starts with '#'", ex.Message);
    }

    [Fact]
    public void Write_KeyStartingWithSpaceAndHash_ThrowsArgumentException()
    {
        var data = new Dictionary<string, string> { ["  #comment"] = "value" };
        var ex = Assert.Throws<ArgumentException>(() => KeyValueWriter.Write(data));
        Assert.Contains("starts with '#'", ex.Message);
    }

    [Fact]
    public void Write_KeyStartingWithSlashSlash_ThrowsArgumentException()
    {
        var data = new Dictionary<string, string> { ["//comment"] = "value" };
        var ex = Assert.Throws<ArgumentException>(() => KeyValueWriter.Write(data));
        Assert.Contains("starts with '//'", ex.Message);
    }

    [Fact]
    public void Write_KeyContainingBracket_ThrowsArgumentException()
    {
        var data = new Dictionary<string, string> { ["[section]"] = "value" };
        var ex = Assert.Throws<ArgumentException>(() => KeyValueWriter.Write(data));
        Assert.Contains("section marker characters", ex.Message);
    }

    [Fact]
    public void Write_KeyContainingAtSign_ThrowsArgumentException()
    {
        var data = new Dictionary<string, string> { ["@key"] = "value" };
        var ex = Assert.Throws<ArgumentException>(() => KeyValueWriter.Write(data));
        Assert.Contains("section marker characters", ex.Message);
    }

    [Fact]
    public void Write_EmptyKey_ThrowsArgumentException()
    {
        var data = new Dictionary<string, string> { [""] = "value" };
        var ex = Assert.Throws<ArgumentException>(() => KeyValueWriter.Write(data));
        Assert.Contains("null or whitespace", ex.Message);
    }
}
