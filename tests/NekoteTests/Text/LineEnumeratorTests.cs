using Nekote.Text;

namespace Nekote.Tests.Text;

public class LineEnumeratorTests
{
    [Fact]
    public void EmptyText_NoLines()
    {
        var text = "";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Empty(lines);
    }

    [Fact]
    public void SingleLine_NoLineBreak()
    {
        var text = "Hello World";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Single(lines);
        Assert.Equal("Hello World", lines[0]);
    }

    [Fact]
    public void MultipleLines_UnixLineEndings()
    {
        var text = "Line 1\nLine 2\nLine 3";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(3, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("Line 2", lines[1]);
        Assert.Equal("Line 3", lines[2]);
    }

    [Fact]
    public void MultipleLines_WindowsLineEndings()
    {
        var text = "Line 1\r\nLine 2\r\nLine 3";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(3, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("Line 2", lines[1]);
        Assert.Equal("Line 3", lines[2]);
    }

    [Fact]
    public void MultipleLines_MacLineEndings()
    {
        var text = "Line 1\rLine 2\rLine 3";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(3, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("Line 2", lines[1]);
        Assert.Equal("Line 3", lines[2]);
    }

    [Fact]
    public void MixedLineEndings()
    {
        var text = "Line 1\nLine 2\r\nLine 3\rLine 4";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(4, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("Line 2", lines[1]);
        Assert.Equal("Line 3", lines[2]);
        Assert.Equal("Line 4", lines[3]);
    }

    [Fact]
    public void EmptyLines()
    {
        var text = "Line 1\n\nLine 3";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(3, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("", lines[1]);
        Assert.Equal("Line 3", lines[2]);
    }

    [Fact]
    public void TrailingLineBreak()
    {
        var text = "Line 1\nLine 2\n";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(2, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("Line 2", lines[1]);
    }

    [Fact]
    public void ConsecutiveLineBreaks()
    {
        var text = "Line 1\n\n\nLine 4";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(4, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("", lines[1]);
        Assert.Equal("", lines[2]);
        Assert.Equal("Line 4", lines[3]);
    }

    #region Edge Cases - Unicode and Extremes

    [Fact]
    public void Lines_WithEmojis()
    {
        var text = "Hello 😀\nWorld 🌍\nTest 🎉";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(3, lines.Count);
        Assert.Equal("Hello 😀", lines[0]);
        Assert.Equal("World 🌍", lines[1]);
        Assert.Equal("Test 🎉", lines[2]);
    }

    [Fact]
    public void Lines_WithZeroWidthCharacters()
    {
        var text = "Line\u200B1\nLine\u200B2"; // Zero-width space
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(2, lines.Count);
        Assert.Contains("\u200B", lines[0]);
        Assert.Contains("\u200B", lines[1]);
    }

    [Fact]
    public void Lines_WithCombiningCharacters()
    {
        var text = "Cafe\u0301\nNaive\u0308"; // e and i with combining diacritics
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(2, lines.Count);
        Assert.Equal("Cafe\u0301", lines[0]);
        Assert.Equal("Naive\u0308", lines[1]);
    }

    [Fact]
    public void VeryLongLine()
    {
        var longLine = new string('x', 100000);
        var text = longLine + "\nShort";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(2, lines.Count);
        Assert.Equal(100000, lines[0].Length);
        Assert.Equal("Short", lines[1]);
    }

    [Fact]
    public void Line_OnlyEmojis()
    {
        var text = "😀😁😂\n🌍🌎🌏";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(2, lines.Count);
        Assert.Equal("😀😁😂", lines[0]);
        Assert.Equal("🌍🌎🌏", lines[1]);
    }

    [Fact]
    public void Line_WithUnicodeLineSeparator()
    {
        // Note: LineProcessor uses IndexOfAny('\r', '\n'), so Unicode line separator (U+2028) is NOT treated as line break
        var text = "Line1\u2028Line2";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateLines(text))
        {
            lines.Add(line.ToString());
        }
        
        // U+2028 is not recognized as a line break by this implementation
        Assert.Single(lines);
        Assert.Equal("Line1\u2028Line2", lines[0]);
    }

    #endregion
}
