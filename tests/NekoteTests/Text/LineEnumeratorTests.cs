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
}
