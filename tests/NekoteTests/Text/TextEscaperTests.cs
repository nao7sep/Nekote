using Nekote.Text;

namespace Nekote.Tests.Text;

public class TextEscaperTests
{
    #region NiniValue Mode Tests

    [Theory]
    [InlineData("", "")]
    [InlineData("simple text", "simple text")]
    [InlineData("Hello\nWorld", "Hello\\nWorld")]
    [InlineData("Line1\nLine2\nLine3", "Line1\\nLine2\\nLine3")]
    [InlineData("Path\\To\\File", "Path\\\\To\\\\File")]
    [InlineData("Mixed\\nContent", "Mixed\\\\nContent")]
    [InlineData("Tab\there", "Tab\\there")]
    [InlineData("Windows\r\nLine", "Windows\\r\\nLine")]
    public void Escape_NiniValue_ProducesCorrectOutput(string input, string expected)
    {
        var result = TextEscaper.Escape(input, EscapeMode.NiniValue);
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
    public void Unescape_NiniValue_ProducesCorrectOutput(string input, string expected)
    {
        var result = TextEscaper.Unescape(input, EscapeMode.NiniValue);
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
    public void Escape_Unescape_NiniValue_RoundTrip(string original)
    {
        var escaped = TextEscaper.Escape(original, EscapeMode.NiniValue);
        var unescaped = TextEscaper.Unescape(escaped, EscapeMode.NiniValue);
        Assert.Equal(original, unescaped);
    }

    [Fact]
    public void Unescape_NiniValue_UnknownEscapeSequence_KeepsBackslash()
    {
        var result = TextEscaper.Unescape("Hello\\xWorld", EscapeMode.NiniValue);
        Assert.Equal("Hello\\xWorld", result);
    }

    [Fact]
    public void Unescape_NiniValue_TrailingBackslash_KeepsBackslash()
    {
        var result = TextEscaper.Unescape("trailing\\", EscapeMode.NiniValue);
        Assert.Equal("trailing\\", result);
    }

    [Theory]
    [InlineData("  leading spaces")]
    [InlineData("trailing spaces  ")]
    [InlineData("  both  ")]
    public void Escape_Unescape_NiniValue_PreservesWhitespace(string original)
    {
        var escaped = TextEscaper.Escape(original, EscapeMode.NiniValue);
        var unescaped = TextEscaper.Unescape(escaped, EscapeMode.NiniValue);
        Assert.Equal(original, unescaped);
    }

    [Fact]
    public void Escape_NiniValue_MultilineTextWithEmptyLines()
    {
        var input = "Line 1\n\nLine 3\n\n\nLine 6";
        var expected = "Line 1\\n\\nLine 3\\n\\n\\nLine 6";
        var result = TextEscaper.Escape(input, EscapeMode.NiniValue);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Escape_NiniValue_ComplexMixedContent()
    {
        var input = "First line\n\tIndented with tab\nPath: C:\\Users\\Name\nEnd";
        var expected = "First line\\n\\tIndented with tab\\nPath: C:\\\\Users\\\\Name\\nEnd";
        var result = TextEscaper.Escape(input, EscapeMode.NiniValue);
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

    [Fact]
    public void Unescape_Url_LiteralSurrogatePair_HandlesCorrectly()
    {
        // "Hello 🌍 World" where 🌍 is literal (not percent encoded)
        // 🌍 is \uD83C\uDF4D (actually \uD83C\uDF0D for Earth Globe Europe-Africa, but any surrogate pair works)
        
        string input = "Hello 🌍 World";
        string expected = "Hello 🌍 World";
        
        string? result = TextEscaper.Unescape(input, EscapeMode.Url);
        
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Unescape_Url_MixedLiteralAndEscaped_HandlesCorrectly()
    {
        // "Hello 🌍 %20World"
        string input = "Hello 🌍 %20World";
        string expected = "Hello 🌍  World"; // Note double space (one from literal, one from %20)
        
        string? result = TextEscaper.Unescape(input, EscapeMode.Url);
        
        Assert.Equal(expected, result);
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
    public void Unescape_Html_NumericEntity_DecodesCorrectly()
    {
        // WebUtility supports both decimal and hex numeric entities.
        var input = "Test &#65; &#x41;";
        var result = TextEscaper.Unescape(input, EscapeMode.Html);
        Assert.Equal("Test A A", result);
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
    public void Escape_NullInput_ReturnsNull()
    {
        var result = TextEscaper.Escape(null, EscapeMode.NiniValue);
        Assert.Null(result);
    }

    [Fact]
    public void Unescape_NullInput_ReturnsNull()
    {
        var result = TextEscaper.Unescape(null, EscapeMode.NiniValue);
        Assert.Null(result);
    }

    [Fact]
    public void Escape_EmptyInput_ReturnsEmptyString()
    {
        var result = TextEscaper.Escape("", EscapeMode.NiniValue);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Unescape_EmptyInput_ReturnsEmptyString()
    {
        var result = TextEscaper.Unescape("", EscapeMode.NiniValue);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Unescape_Url_TrailingPercentOnly_TreatsAsLiteral()
    {
        var result = TextEscaper.Unescape("test%", EscapeMode.Url);
        Assert.Equal("test%", result);
    }

    [Fact]
    public void Unescape_Url_TrailingIncompleteSequence_TreatsAsLiteral()
    {
        var result = TextEscaper.Unescape("test%2", EscapeMode.Url);
        Assert.Equal("test%2", result);
    }

    [Fact]
    public void Escape_Unescape_Url_TrailingIncomplete_RoundTrip()
    {
        // If someone has literal "test%2" in their data, it should round-trip
        var original = "test%2";
        var escaped = TextEscaper.Escape(original, EscapeMode.Url);
        var unescaped = TextEscaper.Unescape(escaped, EscapeMode.Url);
        Assert.Equal(original, unescaped);
    }

    #endregion

    #region Surrogate Pair Tests

    private const string Emoji = "😀"; // U+1F600, UTF-16: \uD83D\uDE00
    private const string TextWithEmoji = "Hello " + Emoji + " World";

    [Fact]
    public void EscapeHtml_RoundTripsSurrogatePairs()
    {
        // WebUtility.HtmlEncode may encode emojis as literals or entities (e.g. &#128512;)
        // depending on the implementation/version. We verify that it round-trips correctly.
        var escaped = TextEscaper.Escape(TextWithEmoji, EscapeMode.Html);
        var unescaped = TextEscaper.Unescape(escaped, EscapeMode.Html);
        Assert.Equal(TextWithEmoji, unescaped);
    }

    [Fact]
    public void EscapeNiniValue_PreservesSurrogatePairs()
    {
        // Uses char iteration, escapes \ \n \r \t
        var escaped = TextEscaper.Escape(TextWithEmoji, EscapeMode.NiniValue);
        Assert.Equal(TextWithEmoji, escaped);
    }

    [Fact]
    public void EscapeCsv_PreservesSurrogatePairs()
    {
        // Uses char iteration, escapes " , \n \r
        var escaped = TextEscaper.Escape(TextWithEmoji, EscapeMode.Csv);
        Assert.Equal(TextWithEmoji, escaped);
        
        var quoted = "Hello, " + Emoji;
        var escapedQuoted = TextEscaper.Escape(quoted, EscapeMode.Csv);
        Assert.Equal("\"Hello, " + Emoji + "\"", escapedQuoted);
    }

    [Fact]
    public void EscapeUrl_HandlesSurrogatePairsCorrectly()
    {
        // Verify that the built-in Uri.EscapeDataString handles emojis correctly
        var escaped = TextEscaper.Escape(Emoji, EscapeMode.Url);
        // %F0%9F%98%80 is the UTF-8 percent-encoded sequence for U+1F600
        Assert.Equal("%F0%9F%98%80", escaped);
    }

    [Fact]
    public void UnescapeUrl_HandlesSurrogatePairs()
    {
        // Case 1: URL-encoded emoji
        var encoded = "Hello%20%F0%9F%98%80";
        var decoded = TextEscaper.Unescape(encoded, EscapeMode.Url);
        Assert.Equal("Hello " + Emoji, decoded);

        // Case 2: Raw emoji in URL string (loose input)
        // Uri.UnescapeDataString generally passes through non-encoded chars
        var raw = "Hello " + Emoji;
        var decodedRaw = TextEscaper.Unescape(raw, EscapeMode.Url);
        Assert.Equal("Hello " + Emoji, decodedRaw);
    }

    [Fact]
    public void TextEscaper_HandlesComplexGraphemeClusters_ZWJ()
    {
        // 👨‍👩‍👧‍👦 (Family: Man, Woman, Girl, Boy) - This is a single visual glyph 
        // but consists of 4 emojis joined by 3 Zero Width Joiners (U+200D).
        // It is a very long sequence of UTF-16 chars.
        const string FamilyEmoji = "👨‍👩‍👧‍👦";
        
        foreach (EscapeMode mode in Enum.GetValues<EscapeMode>())
        {
            var escaped = TextEscaper.Escape(FamilyEmoji, mode);
            var unescaped = TextEscaper.Unescape(escaped, mode);
            
            Assert.Equal(FamilyEmoji, unescaped);
        }
    }

    [Fact]
    public void EscapeUrl_MixedContent_RoundTripsCorrectly()
    {
        // Test mixed content: ASCII + Emoji + Reserved Chars + Spaces
        var input = "Test " + Emoji + " / Path & Query?";
        var escaped = TextEscaper.Escape(input, EscapeMode.Url);
        var unescaped = TextEscaper.Unescape(escaped, EscapeMode.Url);
        Assert.Equal(input, unescaped);
    }

    [Fact]
    public void EscapeUrl_LoneSurrogate_HandledByBCL()
    {
        // A lone high surrogate is invalid UTF-16.
        // The BCL's Uri.EscapeDataString behavior for invalid unicode is implementation-defined
        // but generally safe (throws or replaces). We test that it doesn't crash.
        string invalidInput = "\uD83D"; // Lone high surrogate
        
        try 
        {
            var result = TextEscaper.Escape(invalidInput, EscapeMode.Url);
            // If it succeeds, it should likely be the replacement char encoding or similar
            Assert.NotNull(result);
        }
        catch (Exception)
        {
            // Throwing is also an acceptable safe behavior for invalid input
        }
    }

    #endregion
}
