using System.Text;
using Nekote.Text;

namespace Nekote.Tests.Text;

public class LineProcessorTests
{
    #region CountLines Tests

    [Fact]
    public void CountLines_EmptyText_ReturnsZero()
    {
        Assert.Equal(0, LineProcessor.CountLines(""));
    }

    [Fact]
    public void CountLines_SingleLine_ReturnsOne()
    {
        Assert.Equal(1, LineProcessor.CountLines("Single line"));
    }

    [Theory]
    [InlineData("Line 1\nLine 2", 2)]
    [InlineData("Line 1\nLine 2\nLine 3", 3)]
    [InlineData("Line 1\r\nLine 2\r\nLine 3", 3)]
    [InlineData("Line 1\rLine 2\rLine 3", 3)]
    [InlineData("Line 1\n\nLine 3", 3)]
    public void CountLines_MultipleLines(string text, int expected)
    {
        Assert.Equal(expected, LineProcessor.CountLines(text));
    }

    [Fact]
    public void CountLines_TrailingLineBreak()
    {
        Assert.Equal(3, LineProcessor.CountLines("Line 1\nLine 2\n"));
    }

    #endregion

    #region IsBlank Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("  \t  ")]
    public void IsBlank_WhitespaceOnly_ReturnsTrue(string line)
    {
        Assert.True(LineProcessor.IsBlank(line));
    }

    [Theory]
    [InlineData("a")]
    [InlineData("Hello")]
    [InlineData("  Hello")]
    [InlineData("Hello  ")]
    [InlineData("  Hello  ")]
    [InlineData(" a ")]
    public void IsBlank_ContainsNonWhitespace_ReturnsFalse(string line)
    {
        Assert.False(LineProcessor.IsBlank(line));
    }

    #endregion

    #region GetLeadingWhitespace Tests

    [Theory]
    [InlineData("", "")]
    [InlineData("Hello", "")]
    [InlineData("  Hello", "  ")]
    [InlineData("\tHello", "\t")]
    [InlineData("   \tHello", "   \t")]
    [InlineData("   ", "   ")]
    public void GetLeadingWhitespace_ReturnsCorrectSpan(string line, string expected)
    {
        var result = LineProcessor.GetLeadingWhitespace(line);
        Assert.Equal(expected, result.ToString());
    }

    #endregion

    #region GetTrailingWhitespace Tests

    [Theory]
    [InlineData("", "")]
    [InlineData("Hello", "")]
    [InlineData("Hello  ", "  ")]
    [InlineData("Hello\t", "\t")]
    [InlineData("Hello   \t", "   \t")]
    [InlineData("   ", "   ")]
    public void GetTrailingWhitespace_ReturnsCorrectSpan(string line, string expected)
    {
        var result = LineProcessor.GetTrailingWhitespace(line);
        Assert.Equal(expected, result.ToString());
    }

    #endregion

    #region ProcessLine Tests

    [Fact]
    public void ProcessLine_EmptyLine_ReturnsEmpty()
    {
        var options = LineProcessingOptions.Default;
        var builder = new StringBuilder();
        
        var result = LineProcessor.ProcessLine("", options, builder);
        
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void ProcessLine_PreserveAll()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Preserve,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Preserve,
            InnerWhitespaceReplacement = ' ',
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Preserve,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Preserve,
            InnerBlankLineHandling = InnerBlankLineHandling.Preserve,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Preserve,
            NewLine = "\n"
        };
        var builder = new StringBuilder();
        
        var result = LineProcessor.ProcessLine("  Hello  World  ", options, builder);
        
        Assert.Equal("  Hello  World  ", result.ToString());
    }

    [Fact]
    public void ProcessLine_RemoveLeading()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Remove,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Preserve,
            InnerWhitespaceReplacement = ' ',
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Preserve,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Preserve,
            InnerBlankLineHandling = InnerBlankLineHandling.Preserve,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Preserve,
            NewLine = "\n"
        };
        var builder = new StringBuilder();
        
        var result = LineProcessor.ProcessLine("  Hello  World  ", options, builder);
        
        Assert.Equal("Hello  World  ", result.ToString());
    }

    [Fact]
    public void ProcessLine_RemoveTrailing()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Preserve,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Preserve,
            InnerWhitespaceReplacement = ' ',
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Preserve,
            InnerBlankLineHandling = InnerBlankLineHandling.Preserve,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Preserve,
            NewLine = "\n"
        };
        var builder = new StringBuilder();
        
        var result = LineProcessor.ProcessLine("  Hello  World  ", options, builder);
        
        Assert.Equal("  Hello  World", result.ToString());
    }

    [Fact]
    public void ProcessLine_CollapseInner()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Preserve,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Collapse,
            InnerWhitespaceReplacement = ' ',
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Preserve,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Preserve,
            InnerBlankLineHandling = InnerBlankLineHandling.Preserve,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Preserve,
            NewLine = "\n"
        };
        var builder = new StringBuilder();
        
        var result = LineProcessor.ProcessLine("  Hello   World  ", options, builder);
        
        Assert.Equal("  Hello World  ", result.ToString());
    }

    [Fact]
    public void ProcessLine_CollapseInnerWithCustomReplacement()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Remove,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Collapse,
            InnerWhitespaceReplacement = "_",
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Preserve,
            InnerBlankLineHandling = InnerBlankLineHandling.Preserve,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Preserve,
            NewLine = "\n"
        };
        var builder = new StringBuilder();
        
        var result = LineProcessor.ProcessLine("  Hello   World  ", options, builder);
        
        Assert.Equal("Hello_World", result.ToString());
    }

    [Fact]
    public void ProcessLine_RemoveInner()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Remove,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Remove,
            InnerWhitespaceReplacement = ' ',
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Preserve,
            InnerBlankLineHandling = InnerBlankLineHandling.Preserve,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Preserve,
            NewLine = "\n"
        };
        var builder = new StringBuilder();
        
        var result = LineProcessor.ProcessLine("  Hello   World  ", options, builder);
        
        Assert.Equal("HelloWorld", result.ToString());
    }

    [Fact]
    public void ProcessLine_AllWhitespace_RemoveLeading()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Remove,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Preserve,
            InnerWhitespaceReplacement = ' ',
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Preserve,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Preserve,
            InnerBlankLineHandling = InnerBlankLineHandling.Preserve,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Preserve,
            NewLine = "\n"
        };
        var builder = new StringBuilder();
        
        var result = LineProcessor.ProcessLine("     ", options, builder);
        
        Assert.True(result.IsEmpty);
    }

    #endregion

    #region SplitIntoSections Tests

    [Fact]
    public void SplitIntoSections_EmptyText()
    {
        var result = LineProcessor.SplitIntoSections("", out var leading, out var content, out var trailing);
        
        Assert.False(result);
        Assert.True(leading.IsEmpty);
        Assert.True(content.IsEmpty);
        Assert.True(trailing.IsEmpty);
    }

    [Fact]
    public void SplitIntoSections_OnlyWhitespace()
    {
        var result = LineProcessor.SplitIntoSections("   \n  \n  ", out var leading, out var content, out var trailing);
        
        Assert.False(result);
        Assert.Equal("   \n  \n  ", leading.ToString());
        Assert.True(content.IsEmpty);
        Assert.True(trailing.IsEmpty);
    }

    [Fact]
    public void SplitIntoSections_NoLeadingOrTrailing()
    {
        var text = "Line 1\nLine 2\nLine 3";
        var result = LineProcessor.SplitIntoSections(text, out var leading, out var content, out var trailing);
        
        Assert.True(result);
        Assert.True(leading.IsEmpty);
        Assert.Equal("Line 1\nLine 2\nLine 3", content.ToString());
        Assert.True(trailing.IsEmpty);
    }

    [Fact]
    public void SplitIntoSections_LeadingBlankLines()
    {
        var text = "\n\nLine 1\nLine 2";
        var result = LineProcessor.SplitIntoSections(text, out var leading, out var content, out var trailing);
        
        Assert.True(result);
        Assert.Equal("\n\n", leading.ToString());
        Assert.Equal("Line 1\nLine 2", content.ToString());
        Assert.True(trailing.IsEmpty);
    }

    [Fact]
    public void SplitIntoSections_TrailingBlankLines()
    {
        var text = "Line 1\nLine 2\n\n\n";
        var result = LineProcessor.SplitIntoSections(text, out var leading, out var content, out var trailing);
        
        Assert.True(result);
        Assert.True(leading.IsEmpty);
        Assert.Equal("Line 1\nLine 2\n", content.ToString());
        Assert.Equal("\n\n", trailing.ToString());
    }

    [Fact]
    public void SplitIntoSections_BothLeadingAndTrailing()
    {
        var text = "\n\nLine 1\nLine 2\n\n\n";
        var result = LineProcessor.SplitIntoSections(text, out var leading, out var content, out var trailing);
        
        Assert.True(result);
        Assert.Equal("\n\n", leading.ToString());
        Assert.Equal("Line 1\nLine 2\n", content.ToString());
        Assert.Equal("\n\n", trailing.ToString());
    }

    [Fact]
    public void SplitIntoSections_StartsImmediately()
    {
        var text = "Hello World\n\n";
        var result = LineProcessor.SplitIntoSections(text, out var leading, out var content, out var trailing);
        
        Assert.True(result);
        Assert.True(leading.IsEmpty);
        Assert.Equal("Hello World\n", content.ToString());
        Assert.Equal("\n", trailing.ToString());
    }

    #endregion

    #region Edge Cases - Unicode Whitespace

    [Theory]
    [InlineData("\u00A0")] // Non-breaking space
    [InlineData("\u2003")] // Em space
    [InlineData("\u3000")] // Ideographic space
    [InlineData("\u1680")] // Ogham space mark
    [InlineData("\u2000")] // En quad
    public void IsBlank_UnicodeWhitespace_ReturnsTrue(string whitespace)
    {
        Assert.True(LineProcessor.IsBlank(whitespace));
    }

    [Fact]
    public void GetLeadingWhitespace_UnicodeSpaces()
    {
        var line = "\u00A0\u2003Hello";
        var result = LineProcessor.GetLeadingWhitespace(line);
        Assert.Equal("\u00A0\u2003", result.ToString());
    }

    [Fact]
    public void GetTrailingWhitespace_UnicodeSpaces()
    {
        var line = "Hello\u00A0\u2003";
        var result = LineProcessor.GetTrailingWhitespace(line);
        Assert.Equal("\u00A0\u2003", result.ToString());
    }

    [Fact]
    public void ProcessLine_WithEmojis()
    {
        var options = LineProcessingOptions.Default;
        var builder = new StringBuilder();
        
        var result = LineProcessor.ProcessLine("  Hello 😀 World  ", options, builder);
        
        Assert.Equal("  Hello 😀 World", result.ToString());
    }

    [Fact]
    public void ProcessLine_CollapseInner_WithUnicodeSpaces()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Remove,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Collapse,
            InnerWhitespaceReplacement = ' ',
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Preserve,
            InnerBlankLineHandling = InnerBlankLineHandling.Preserve,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Preserve,
            NewLine = "\n"
        };
        var builder = new StringBuilder();
        
        var result = LineProcessor.ProcessLine("Hello\u00A0\u00A0\u2003World", options, builder);
        
        // Unicode whitespace should be collapsed to single space
        Assert.Equal("Hello World", result.ToString());
    }

    [Fact]
    public void ProcessLine_OnlyEmojis()
    {
        var options = LineProcessingOptions.Default;
        var builder = new StringBuilder();
        
        var result = LineProcessor.ProcessLine("😀😁😂", options, builder);
        
        Assert.Equal("😀😁😂", result.ToString());
    }

    [Fact]
    public void ProcessLine_ZeroWidthCharacters()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Remove,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Preserve,
            InnerWhitespaceReplacement = ' ',
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Preserve,
            InnerBlankLineHandling = InnerBlankLineHandling.Preserve,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Preserve,
            NewLine = "\n"
        };
        var builder = new StringBuilder();
        
        // Zero-width space is not whitespace according to char.IsWhiteSpace
        var result = LineProcessor.ProcessLine("Hello\u200BWorld", options, builder);
        
        Assert.Equal("Hello\u200BWorld", result.ToString());
    }

    [Fact]
    public void IsBlank_OnlyZeroWidthSpace_ReturnsFalse()
    {
        // Zero-width space (U+200B) is not considered whitespace by char.IsWhiteSpace
        Assert.False(LineProcessor.IsBlank("\u200B"));
    }

    [Fact]
    public void SplitIntoSections_WithEmojis()
    {
        var text = "\n😀 Line 1\n😁 Line 2\n\n";
        var result = LineProcessor.SplitIntoSections(text, out var leading, out var content, out var trailing);
        
        Assert.True(result);
        Assert.Equal("\n", leading.ToString());
        Assert.Contains("😀", content.ToString());
        Assert.Equal("\n", trailing.ToString());
    }

    [Fact]
    public void CountLines_VeryLongText()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < 10000; i++)
        {
            sb.Append("Line ").Append(i);
            if (i < 9999) sb.Append('\n'); // Don't add \n after the last line
        }
        var text = sb.ToString();
        
        Assert.Equal(10000, LineProcessor.CountLines(text));
    }

    #endregion

    #region Process Tests

    [Fact]
    public void Process_EmptyText_ReturnsEmpty()
    {
        var result = LineProcessor.Process("");
        Assert.Equal("", result);
    }

    [Fact]
    public void Process_Default_RemovesLeadingAndTrailingBlankLines()
    {
        var text = "\n\nLine 1\nLine 2\n\n\n";
        var result = LineProcessor.Process(text);
        
        Assert.Equal($"Line 1{Environment.NewLine}Line 2", result);
    }

    [Fact]
    public void Process_Default_CollapsesInnerBlankLines()
    {
        var text = "Line 1\n\n\nLine 2";
        var result = LineProcessor.Process(text);
        
        Assert.Equal($"Line 1{Environment.NewLine}{Environment.NewLine}Line 2", result);
    }

    [Fact]
    public void Process_Minimal_RemovesAllWhitespace()
    {
        var text = "  Line 1  \n  Line 2  ";
        var result = LineProcessor.Process(text, LineProcessingOptions.Minimal);
        
        Assert.Equal("Line1Line2", result);
    }

    [Fact]
    public void ToSingleLine_JoinsWithSpaces()
    {
        var text = "Line 1\nLine 2\nLine 3";
        var result = LineProcessor.ToSingleLine(text);
        
        Assert.Equal("Line 1 Line 2 Line 3", result);
    }

    [Fact]
    public void ToSingleLine_RemovesBlankLines()
    {
        var text = "\nLine 1\n\nLine 2\n";
        var result = LineProcessor.ToSingleLine(text);
        
        Assert.Equal("Line 1 Line 2", result);
    }

    #endregion
}
