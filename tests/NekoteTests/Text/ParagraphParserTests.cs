using Nekote.Text;

namespace Nekote.Tests.Text;

public class ParagraphParserTests
{
    [Fact]
    public void Parse_EmptyString_ReturnsEmptyArray()
    {
        var result = ParagraphParser.Parse("");
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_WhitespaceOnly_ReturnsEmptyArray()
    {
        var result = ParagraphParser.Parse("   \n\n   \n  ");
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_SingleParagraph_ReturnsSingleElement()
    {
        var input = "This is a single paragraph.";
        var result = ParagraphParser.Parse(input);

        Assert.Single(result);
        Assert.Equal("This is a single paragraph.", result[0]);
    }

    [Fact]
    public void Parse_SingleParagraphMultipleLines_JoinsLines()
    {
        var input = "Line 1\nLine 2\nLine 3";
        var result = ParagraphParser.Parse(input);

        Assert.Single(result);
        Assert.Equal($"Line 1{Environment.NewLine}Line 2{Environment.NewLine}Line 3", result[0]);
    }

    [Fact]
    public void Parse_TwoParagraphs_ReturnsTwoElements()
    {
        var input = "First paragraph.\n\nSecond paragraph.";
        var result = ParagraphParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("First paragraph.", result[0]);
        Assert.Equal("Second paragraph.", result[1]);
    }

    [Fact]
    public void Parse_MultipleParagraphs_SplitsCorrectly()
    {
        var input = "Paragraph 1\nLine 2\n\nParagraph 2\n\nParagraph 3\nLine 2\nLine 3";
        var result = ParagraphParser.Parse(input);

        Assert.Equal(3, result.Length);
        Assert.Equal($"Paragraph 1{Environment.NewLine}Line 2", result[0]);
        Assert.Equal("Paragraph 2", result[1]);
        Assert.Equal($"Paragraph 3{Environment.NewLine}Line 2{Environment.NewLine}Line 3", result[2]);
    }

    [Fact]
    public void Parse_ConsecutiveBlankLines_TreatedAsSingleSeparator()
    {
        var input = "Paragraph 1\n\n\n\nParagraph 2";
        var result = ParagraphParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("Paragraph 1", result[0]);
        Assert.Equal("Paragraph 2", result[1]);
    }

    [Fact]
    public void Parse_LeadingBlankLines_Ignored()
    {
        var input = "\n\n\nParagraph 1\n\nParagraph 2";
        var result = ParagraphParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("Paragraph 1", result[0]);
        Assert.Equal("Paragraph 2", result[1]);
    }

    [Fact]
    public void Parse_TrailingBlankLines_Ignored()
    {
        var input = "Paragraph 1\n\nParagraph 2\n\n\n";
        var result = ParagraphParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("Paragraph 1", result[0]);
        Assert.Equal("Paragraph 2", result[1]);
    }

    [Fact]
    public void Parse_WindowsLineEndings_WithinParagraph_DoesNotSplit()
    {
        var input = "Line 1\r\nLine 2";
        var result = ParagraphParser.Parse(input);

        Assert.Single(result);
        Assert.Equal($"Line 1{Environment.NewLine}Line 2", result[0]);
    }

    [Fact]
    public void Parse_WindowsLineEndings_HandlesCorrectly()
    {
        var input = "Paragraph 1\r\n\r\nParagraph 2";
        var result = ParagraphParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("Paragraph 1", result[0]);
        Assert.Equal("Paragraph 2", result[1]);
    }

    [Fact]
    public void Parse_MixedLineEndings_HandlesCorrectly()
    {
        var input = "Paragraph 1\n\r\nParagraph 2";
        var result = ParagraphParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("Paragraph 1", result[0]);
        Assert.Equal("Paragraph 2", result[1]);
    }

    [Fact]
    public void Parse_WhitespaceOnlyLines_TreatedAsBlank()
    {
        var input = "Paragraph 1\n   \nParagraph 2";
        var result = ParagraphParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("Paragraph 1", result[0]);
        Assert.Equal("Paragraph 2", result[1]);
    }

    [Fact]
    public void Parse_PreservesWhitespace()
    {
        var input = "  Paragraph 1  \n\n  Paragraph 2  ";
        var result = ParagraphParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal("  Paragraph 1  ", result[0]);
        Assert.Equal("  Paragraph 2  ", result[1]);
    }

    [Fact]
    public void Parse_ComplexDocument_ParsesCorrectly()
    {
        var input = "This is the first paragraph.\nIt has multiple lines.\n\nThis is the second paragraph.\n\nThis is the third paragraph.\nIt also has multiple lines.\nAnd even more lines.";
        var result = ParagraphParser.Parse(input);

        Assert.Equal(3, result.Length);
        Assert.Equal($"This is the first paragraph.{Environment.NewLine}It has multiple lines.", result[0]);
        Assert.Equal("This is the second paragraph.", result[1]);
        Assert.Equal($"This is the third paragraph.{Environment.NewLine}It also has multiple lines.{Environment.NewLine}And even more lines.", result[2]);
    }

    [Fact]
    public void Parse_MarkdownLikeText_ParsesCorrectly()
    {
        var input = @"# Heading

This is a paragraph under the heading.

## Subheading

Another paragraph here.";
        var result = ParagraphParser.Parse(input);

        Assert.Equal(4, result.Length);
        Assert.Equal("# Heading", result[0]);
        Assert.Equal("This is a paragraph under the heading.", result[1]);
        Assert.Equal("## Subheading", result[2]);
        Assert.Equal("Another paragraph here.", result[3]);
    }

    [Fact]
    public void Parse_CustomNewLine_Unix_UsesSpecifiedNewLine()
    {
        var input = "Line 1\nLine 2\n\nLine 3";
        var result = ParagraphParser.Parse(input, newLine: "\n");

        Assert.Equal(2, result.Length);
        Assert.Equal("Line 1\nLine 2", result[0]);
        Assert.Equal("Line 3", result[1]);
    }

    [Fact]
    public void Parse_CustomNewLine_Windows_UsesSpecifiedNewLine()
    {
        var input = "Line 1\nLine 2\n\nLine 3";
        var result = ParagraphParser.Parse(input, newLine: "\r\n");

        Assert.Equal(2, result.Length);
        Assert.Equal("Line 1\r\nLine 2", result[0]);
        Assert.Equal("Line 3", result[1]);
    }

    [Fact]
    public void Parse_CustomNewLine_Space_UsesSpecifiedNewLine()
    {
        var input = "Line 1\nLine 2\n\nLine 3";
        var result = ParagraphParser.Parse(input, newLine: " ");

        Assert.Equal(2, result.Length);
        Assert.Equal("Line 1 Line 2", result[0]);
        Assert.Equal("Line 3", result[1]);
    }

    [Fact]
    public void Parse_DefaultNewLine_UsesEnvironmentNewLine()
    {
        var input = "Line 1\nLine 2\n\nLine 3";
        var result = ParagraphParser.Parse(input);

        Assert.Equal(2, result.Length);
        Assert.Equal($"Line 1{Environment.NewLine}Line 2", result[0]);
        Assert.Equal("Line 3", result[1]);
    }
}
