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
