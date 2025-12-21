using Nekote.Text;

namespace Nekote.Tests.Text;

public class LineParserTests
{
    #region ToLines Tests

    [Fact]
    public void ToLines_EmptyString_ReturnsEmptyArray()
    {
        var result = LineParser.ToLines("");
        Assert.Empty(result);
    }

    [Fact]
    public void ToLines_Null_ReturnsEmptyArray()
    {
        var result = LineParser.ToLines(null);
        Assert.Empty(result);
    }

    [Fact]
    public void ToLines_SingleLine_ReturnsOneElement()
    {
        var result = LineParser.ToLines("single line");
        Assert.Single(result);
        Assert.Equal("single line", result[0]);
    }

    [Fact]
    public void ToLines_UnixNewlines_SplitsCorrectly()
    {
        var input = "line1\nline2\nline3";
        var result = LineParser.ToLines(input);

        Assert.Equal(3, result.Length);
        Assert.Equal("line1", result[0]);
        Assert.Equal("line2", result[1]);
        Assert.Equal("line3", result[2]);
    }

    [Fact]
    public void ToLines_WindowsNewlines_SplitsCorrectly()
    {
        var input = "line1\r\nline2\r\nline3";
        var result = LineParser.ToLines(input);

        Assert.Equal(3, result.Length);
        Assert.Equal("line1", result[0]);
        Assert.Equal("line2", result[1]);
        Assert.Equal("line3", result[2]);
    }

    [Fact]
    public void ToLines_OldMacNewlines_SplitsCorrectly()
    {
        var input = "line1\rline2\rline3";
        var result = LineParser.ToLines(input);

        Assert.Equal(3, result.Length);
        Assert.Equal("line1", result[0]);
        Assert.Equal("line2", result[1]);
        Assert.Equal("line3", result[2]);
    }

    [Fact]
    public void ToLines_MixedNewlines_SplitsCorrectly()
    {
        var input = "line1\nline2\r\nline3\rline4";
        var result = LineParser.ToLines(input);

        Assert.Equal(4, result.Length);
        Assert.Equal("line1", result[0]);
        Assert.Equal("line2", result[1]);
        Assert.Equal("line3", result[2]);
        Assert.Equal("line4", result[3]);
    }

    [Fact]
    public void ToLines_EmptyLines_PreservesEmptyLines()
    {
        var input = "line1\n\nline3\n\n\nline6";
        var result = LineParser.ToLines(input);

        Assert.Equal(6, result.Length);
        Assert.Equal("line1", result[0]);
        Assert.Equal("", result[1]);
        Assert.Equal("line3", result[2]);
        Assert.Equal("", result[3]);
        Assert.Equal("", result[4]);
        Assert.Equal("line6", result[5]);
    }

    [Fact]
    public void ToLines_NoTrailingNewline_HasFinalLine()
    {
        var input = "line1\nline2";
        var result = LineParser.ToLines(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("line1", result[0]);
        Assert.Equal("line2", result[1]);
    }

    [Fact]
    public void ToLines_OnlyNewlines_ReturnsEmptyLines()
    {
        var input = "\n\n\n";
        var result = LineParser.ToLines(input);

        // Terminator semantics: \n \n \n
        // 1. \n -> "" (Line 1)
        // 2. \n -> "" (Line 2)
        // 3. \n -> "" (Line 3)
        // End.
        Assert.Equal(3, result.Length);
        Assert.All(result, line => Assert.Equal("", line));
    }

    [Fact]
    public void ToLines_WindowsCRLF_TreatsAsSingleLineEnding()
    {
        // This is the critical test - \r\n should be ONE line ending, not two
        var input = "line1\r\nline2";
        var result = LineParser.ToLines(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("line1", result[0]);
        Assert.Equal("line2", result[1]);
    }

    #endregion

    #region FromLines Tests

    [Fact]
    public void FromLines_EmptyArray_ReturnsEmptyString()
    {
        var result = LineParser.FromLines(Array.Empty<string>());
        Assert.Equal("", result);
    }

    [Fact]
    public void FromLines_Null_ReturnsEmptyString()
    {
        var result = LineParser.FromLines(null);
        Assert.Equal("", result);
    }

    [Fact]
    public void FromLines_SingleLine_ReturnsLine()
    {
        var lines = new[] { "single line" };
        var result = LineParser.FromLines(lines);
        Assert.Equal("single line", result);
    }

    [Fact]
    public void FromLines_MultipleLines_JoinsWithEnvironmentNewLine()
    {
        var lines = new[] { "line1", "line2", "line3" };
        var result = LineParser.FromLines(lines);
        Assert.Equal($"line1{Environment.NewLine}line2{Environment.NewLine}line3", result);
    }

    [Fact]
    public void FromLines_CustomNewLine_Unix_UsesSpecified()
    {
        var lines = new[] { "line1", "line2", "line3" };
        var result = LineParser.FromLines(lines, "\n");
        Assert.Equal("line1\nline2\nline3", result);
    }

    [Fact]
    public void FromLines_CustomNewLine_Windows_UsesSpecified()
    {
        var lines = new[] { "line1", "line2", "line3" };
        var result = LineParser.FromLines(lines, "\r\n");
        Assert.Equal("line1\r\nline2\r\nline3", result);
    }

    [Fact]
    public void FromLines_EmptyLines_PreservesEmptyLines()
    {
        var lines = new[] { "line1", "", "line3" };
        var result = LineParser.FromLines(lines, "\n");
        Assert.Equal("line1\n\nline3", result);
    }
    [Fact]
    public void FromLines_IEnumerable_JoinsCorrectly()
    {
        // Test IEnumerable<string> overload with List<string>
        var lines = new List<string> { "line1", "line2", "line3" };
        var result = LineParser.FromLines(lines);
        Assert.Equal($"line1{Environment.NewLine}line2{Environment.NewLine}line3", result);
    }

    [Fact]
    public void FromLines_IEnumerableWithCustomNewline_JoinsCorrectly()
    {
        // Test IEnumerable<string> overload with LINQ query
        var lines = new[] { "a", "b", "c" }.Where(x => x != "skip");
        var result = LineParser.FromLines(lines, "|");
        Assert.Equal("a|b|c", result);
    }
    #endregion

    #region CountLines Tests

    [Fact]
    public void CountLines_EmptyString_ReturnsZero()
    {
        var result = LineParser.CountLines("");
        Assert.Equal(0, result);
    }

    [Fact]
    public void CountLines_Null_ReturnsZero()
    {
        var result = LineParser.CountLines(null);
        Assert.Equal(0, result);
    }

    [Fact]
    public void CountLines_SingleLine_ReturnsOne()
    {
        var result = LineParser.CountLines("single line");
        Assert.Equal(1, result);
    }

    [Fact]
    public void CountLines_UnixNewlines_CountsCorrectly()
    {
        var result = LineParser.CountLines("line1\nline2\nline3");
        Assert.Equal(3, result);
    }

    [Fact]
    public void CountLines_WindowsNewlines_CountsCorrectly()
    {
        var result = LineParser.CountLines("line1\r\nline2\r\nline3");
        Assert.Equal(3, result);
    }

    [Fact]
    public void CountLines_OldMacNewlines_CountsCorrectly()
    {
        var result = LineParser.CountLines("line1\rline2\rline3");
        Assert.Equal(3, result);
    }

    [Fact]
    public void CountLines_MixedNewlines_CountsCorrectly()
    {
        var result = LineParser.CountLines("line1\nline2\r\nline3\rline4");
        Assert.Equal(4, result);
    }

    [Fact]
    public void CountLines_EmptyLines_CountsAll()
    {
        var result = LineParser.CountLines("line1\n\nline3");
        Assert.Equal(3, result);
    }

    #endregion

    #region Round-Trip Tests

    [Theory]
    [InlineData("line1\nline2\nline3", "\n")]
    [InlineData("line1\r\nline2\r\nline3", "\r\n")]
    [InlineData("line1\rline2\rline3", "\r")]
    public void ToLines_FromLines_RoundTrip_PreservesText(string original, string newLine)
    {
        var lines = LineParser.ToLines(original);
        var result = LineParser.FromLines(lines, newLine);
        Assert.Equal(original, result);
    }

    [Fact]
    public void ToLines_FromLines_MixedNewlines_NormalizesToSingleType()
    {
        var input = "line1\nline2\r\nline3\rline4";
        var lines = LineParser.ToLines(input);
        var result = LineParser.FromLines(lines, "\n");

        Assert.Equal("line1\nline2\nline3\nline4", result);
    }

    #endregion

    #region Surrogate Pair Tests

    [Fact]
    public void LineParser_PreservesSurrogatePairs()
    {
        const string Emoji = "😀";
        const string TextWithEmoji = "Hello " + Emoji + " World";

        var lines = LineParser.ToLines(TextWithEmoji);
        Assert.Single(lines);
        Assert.Equal(TextWithEmoji, lines[0]);
        
        var multiLine = "Line1 " + Emoji + "\nLine2";
        var parsed = LineParser.ToLines(multiLine);
        Assert.Equal(2, parsed.Length);
        Assert.Equal("Line1 " + Emoji, parsed[0]);
    }

    [Fact]
    public void LineParser_PassesThroughLoneSurrogates()
    {
        // LineParser should not try to fix or validate Unicode, just split lines.
        // A lone surrogate \uD83D should be preserved.
        string invalidUtf16 = "Line1\uD83D\nLine2";
        
        var lines = LineParser.ToLines(invalidUtf16);
        
        Assert.Equal(2, lines.Length);
        Assert.Equal("Line1\uD83D", lines[0]);
        Assert.Equal("Line2", lines[1]);
    }

    #endregion
}
