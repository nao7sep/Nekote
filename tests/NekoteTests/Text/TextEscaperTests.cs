using Nekote.Text;

namespace Nekote.Tests.Text;

public class TextEscaperTests
{
    #region KeyValue Mode Tests

    [Theory]
    [InlineData("", "")]
    [InlineData("simple text", "simple text")]
    [InlineData("Hello\nWorld", "Hello\\nWorld")]
    [InlineData("Line1\nLine2\nLine3", "Line1\\nLine2\\nLine3")]
    [InlineData("Path\\To\\File", "Path\\\\To\\\\File")]
    [InlineData("Mixed\\nContent", "Mixed\\\\nContent")]
    [InlineData("Tab\there", "Tab\\there")]
    [InlineData("Windows\r\nLine", "Windows\\r\\nLine")]
    public void Escape_KeyValue_ProducesCorrectOutput(string input, string expected)
    {
        var result = TextEscaper.Escape(input, EscapeMode.KeyValue);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("simple text", "simple text")]
    [InlineData("Hello\\nWorld", "Hello\nWorld")]
    [InlineData("Line1\\nLine2\\nLine3", "Line1\nLine2\nLine3")]
    [InlineData("Path\\\\To\\\\File", "Path\\To\\File")]
    [InlineData("Mixed\\\\nContent", "Mixed\\nContent")]
    [InlineData("Tab\\there", "Tab\there")]
    [InlineData("Windows\\r\\nLine", "Windows\r\nLine")]
    public void Unescape_KeyValue_ProducesCorrectOutput(string input, string expected)
    {
        var result = TextEscaper.Unescape(input, EscapeMode.KeyValue);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("simple text")]
    [InlineData("Hello\nWorld")]
    [InlineData("Line1\nLine2\nLine3")]
    [InlineData("Path\\To\\File")]
    [InlineData("Tab\there")]
    [InlineData("\n\n\n")]
    [InlineData("\\\\\\")]
    [InlineData("")]
    public void Escape_Unescape_KeyValue_RoundTrip(string original)
    {
        var escaped = TextEscaper.Escape(original, EscapeMode.KeyValue);
        var unescaped = TextEscaper.Unescape(escaped, EscapeMode.KeyValue);
        Assert.Equal(original, unescaped);
    }

    [Fact]
    public void Unescape_KeyValue_UnknownEscapeSequence_KeepsBackslash()
    {
        var result = TextEscaper.Unescape("Hello\\xWorld", EscapeMode.KeyValue);
        Assert.Equal("Hello\\xWorld", result);
    }

    [Fact]
    public void Unescape_KeyValue_TrailingBackslash_KeepsBackslash()
    {
        var result = TextEscaper.Unescape("trailing\\", EscapeMode.KeyValue);
        Assert.Equal("trailing\\", result);
    }

    [Theory]
    [InlineData("  leading spaces")]
    [InlineData("trailing spaces  ")]
    [InlineData("  both  ")]
    public void Escape_Unescape_KeyValue_PreservesWhitespace(string original)
    {
        var escaped = TextEscaper.Escape(original, EscapeMode.KeyValue);
        var unescaped = TextEscaper.Unescape(escaped, EscapeMode.KeyValue);
        Assert.Equal(original, unescaped);
    }

    [Fact]
    public void Escape_KeyValue_MultilineTextWithEmptyLines()
    {
        var input = "Line 1\n\nLine 3\n\n\nLine 6";
        var expected = "Line 1\\n\\nLine 3\\n\\n\\nLine 6";
        var result = TextEscaper.Escape(input, EscapeMode.KeyValue);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Escape_KeyValue_ComplexMixedContent()
    {
        var input = "First line\n\tIndented with tab\nPath: C:\\Users\\Name\nEnd";
        var expected = "First line\\n\\tIndented with tab\\nPath: C:\\\\Users\\\\Name\\nEnd";
        var result = TextEscaper.Escape(input, EscapeMode.KeyValue);
        Assert.Equal(expected, result);
    }

    #endregion

    #region CSV Mode Tests

    [Theory]
    [InlineData("", "")]
    [InlineData("simple", "simple")]
    [InlineData("no special chars", "no special chars")]
    [InlineData("has,comma", "\"has,comma\"")]
    [InlineData("has\"quote", "\"has\"\"quote\"")]
    [InlineData("has\nline break", "\"has\nline break\"")]
    [InlineData("has,comma and \"quote\"", "\"has,comma and \"\"quote\"\"\"")]
    [InlineData("multiple\"\"quotes", "\"multiple\"\"\"\"quotes\"")]
    public void Escape_Csv_ProducesCorrectOutput(string input, string expected)
    {
        var result = TextEscaper.Escape(input, EscapeMode.Csv);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("simple", "simple")]
    [InlineData("\"has,comma\"", "has,comma")]
    [InlineData("\"has\"\"quote\"", "has\"quote")]
    [InlineData("\"has\nline break\"", "has\nline break")]
    [InlineData("\"has,comma and \"\"quote\"\"\"", "has,comma and \"quote\"")]
    public void Unescape_Csv_ProducesCorrectOutput(string input, string expected)
    {
        var result = TextEscaper.Unescape(input, EscapeMode.Csv);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("simple")]
    [InlineData("has,comma")]
    [InlineData("has\"quote")]
    [InlineData("has\nline break")]
    [InlineData("has,comma and \"quote\"")]
    [InlineData("")]
    public void Escape_Unescape_Csv_RoundTrip(string original)
    {
        var escaped = TextEscaper.Escape(original, EscapeMode.Csv);
        var unescaped = TextEscaper.Unescape(escaped, EscapeMode.Csv);
        Assert.Equal(original, unescaped);
    }

    [Fact]
    public void Unescape_Csv_UnquotedText_ReturnsAsIs()
    {
        var result = TextEscaper.Unescape("not quoted", EscapeMode.Csv);
        Assert.Equal("not quoted", result);
    }

    #endregion

    #region URL Mode Tests

    [Theory]
    [InlineData("", "")]
    [InlineData("simple", "simple")]
    [InlineData("hello world", "hello%20world")]
    [InlineData("a+b=c", "a%2Bb%3Dc")]
    [InlineData("user@example.com", "user%40example.com")]
    [InlineData("path/to/file", "path%2Fto%2Ffile")]
    [InlineData("100%", "100%25")]
    [InlineData("hello\nworld", "hello%0Aworld")]
    [InlineData("tab\there", "tab%09here")]
    public void Escape_Url_ProducesCorrectOutput(string input, string expected)
    {
        var result = TextEscaper.Escape(input, EscapeMode.Url);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("simple", "simple")]
    [InlineData("hello%20world", "hello world")]
    [InlineData("a%2Bb%3Dc", "a+b=c")]
    [InlineData("user%40example.com", "user@example.com")]
    [InlineData("path%2Fto%2Ffile", "path/to/file")]
    [InlineData("100%25", "100%")]
    public void Unescape_Url_ProducesCorrectOutput(string input, string expected)
    {
        var result = TextEscaper.Unescape(input, EscapeMode.Url);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("simple text")]
    [InlineData("hello world")]
    [InlineData("a+b=c")]
    [InlineData("user@example.com")]
    [InlineData("100%")]
    [InlineData("")]
    public void Escape_Unescape_Url_RoundTrip(string original)
    {
        var escaped = TextEscaper.Escape(original, EscapeMode.Url);
        var unescaped = TextEscaper.Unescape(escaped, EscapeMode.Url);
        Assert.Equal(original, unescaped);
    }

    [Fact]
    public void Escape_Url_UnreservedChars_NotEncoded()
    {
        var input = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_.~";
        var result = TextEscaper.Escape(input, EscapeMode.Url);
        Assert.Equal(input, result);
    }

    [Fact]
    public void Unescape_Url_InvalidHex_TreatsPercentLiteral()
    {
        var result = TextEscaper.Unescape("test%ZZinvalid", EscapeMode.Url);
        Assert.Equal("test%ZZinvalid", result);
    }

    [Fact]
    public void Unescape_Url_IncompleteSequence_TreatsPercentLiteral()
    {
        var result = TextEscaper.Unescape("test%2", EscapeMode.Url);
        Assert.Equal("test%2", result);
    }

    [Fact]
    public void Escape_Url_Emoji_HandlesCorrectly()
    {
        var input = "Hello 🌍 World";
        var result = TextEscaper.Escape(input, EscapeMode.Url);
        Assert.Equal("Hello%20%F0%9F%8C%8D%20World", result);
    }

    [Fact]
    public void Unescape_Url_Emoji_HandlesCorrectly()
    {
        var input = "Hello%20%F0%9F%8C%8D%20World";
        var result = TextEscaper.Unescape(input, EscapeMode.Url);
        Assert.Equal("Hello 🌍 World", result);
    }

    [Fact]
    public void Escape_Url_NonAscii_HandlesCorrectly()
    {
        var input = "café";
        var result = TextEscaper.Escape(input, EscapeMode.Url);
        Assert.Equal("caf%C3%A9", result);
    }

    [Fact]
    public void Unescape_Url_NonAscii_HandlesCorrectly()
    {
        var input = "caf%C3%A9";
        var result = TextEscaper.Unescape(input, EscapeMode.Url);
        Assert.Equal("café", result);
    }

    [Fact]
    public void Unescape_Url_MixedEncodedAndNonAscii_HandlesCorrectly()
    {
        // This tests the critical bug fix: passing already-decoded non-ASCII chars
        var input = "café%20test";  // "café" is not encoded, but space is
        var result = TextEscaper.Unescape(input, EscapeMode.Url);
        Assert.Equal("café test", result);
    }

    [Fact]
    public void Escape_Unescape_Url_ComplexUnicode_RoundTrip()
    {
        var original = "Hello 🌍 café 日本語 test";
        var escaped = TextEscaper.Escape(original, EscapeMode.Url);
        var unescaped = TextEscaper.Unescape(escaped, EscapeMode.Url);
        Assert.Equal(original, unescaped);
    }

    #endregion

    #region HTML Mode Tests

    [Theory]
    [InlineData("", "")]
    [InlineData("simple text", "simple text")]
    [InlineData("hello & goodbye", "hello &amp; goodbye")]
    [InlineData("<div>content</div>", "&lt;div&gt;content&lt;/div&gt;")]
    [InlineData("\"quoted\"", "&quot;quoted&quot;")]
    [InlineData("it's here", "it&#39;s here")]
    [InlineData("<a href=\"link\">text</a>", "&lt;a href=&quot;link&quot;&gt;text&lt;/a&gt;")]
    [InlineData("&<>\"'", "&amp;&lt;&gt;&quot;&#39;")]
    public void Escape_Html_ProducesCorrectOutput(string input, string expected)
    {
        var result = TextEscaper.Escape(input, EscapeMode.Html);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("simple text", "simple text")]
    [InlineData("hello &amp; goodbye", "hello & goodbye")]
    [InlineData("&lt;div&gt;content&lt;/div&gt;", "<div>content</div>")]
    [InlineData("&quot;quoted&quot;", "\"quoted\"")]
    [InlineData("it&#39;s here", "it's here")]
    [InlineData("&lt;a href=&quot;link&quot;&gt;text&lt;/a&gt;", "<a href=\"link\">text</a>")]
    [InlineData("&amp;&lt;&gt;&quot;&#39;", "&<>\"'")]
    public void Unescape_Html_ProducesCorrectOutput(string input, string expected)
    {
        var result = TextEscaper.Unescape(input, EscapeMode.Html);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("simple text")]
    [InlineData("hello & goodbye")]
    [InlineData("<div>content</div>")]
    [InlineData("\"quoted\"")]
    [InlineData("it's here")]
    [InlineData("")]
    public void Escape_Unescape_Html_RoundTrip(string original)
    {
        var escaped = TextEscaper.Escape(original, EscapeMode.Html);
        var unescaped = TextEscaper.Unescape(escaped, EscapeMode.Html);
        Assert.Equal(original, unescaped);
    }

    [Fact]
    public void Unescape_Html_UnknownEntity_RemainsUnchanged()
    {
        var result = TextEscaper.Unescape("test &unknown; entity", EscapeMode.Html);
        Assert.Equal("test &unknown; entity", result);
    }

    [Fact]
    public void Unescape_Html_IncompleteEntity_RemainsUnchanged()
    {
        var result = TextEscaper.Unescape("test &amp", EscapeMode.Html);
        Assert.Equal("test &amp", result);
    }

    [Fact]
    public void Unescape_Html_Apos_HandlesCorrectly()
    {
        var result = TextEscaper.Unescape("it&apos;s", EscapeMode.Html);
        Assert.Equal("it's", result);
    }

    [Fact]
    public void Unescape_Html_Nbsp_HandlesCorrectly()
    {
        var result = TextEscaper.Unescape("hello&nbsp;world", EscapeMode.Html);
        Assert.Equal("hello\u00A0world", result);
    }

    [Fact]
    public void Unescape_Html_AllStandardEntities_HandlesCorrectly()
    {
        var input = "&amp;&lt;&gt;&quot;&#39;&apos;&nbsp;";
        var result = TextEscaper.Unescape(input, EscapeMode.Html);
        Assert.Equal("&<>\"''\u00A0", result);
    }

    #endregion

    #region General Tests

    [Fact]
    public void Escape_NullInput_ReturnsEmptyString()
    {
        var result = TextEscaper.Escape(null, EscapeMode.KeyValue);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Unescape_NullInput_ReturnsEmptyString()
    {
        var result = TextEscaper.Unescape(null, EscapeMode.KeyValue);
        Assert.Equal(string.Empty, result);
    }

    #endregion
}
