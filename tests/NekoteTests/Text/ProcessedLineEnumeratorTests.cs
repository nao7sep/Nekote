using System.Text;
using Nekote.Text;

namespace Nekote.Tests.Text;

public class ProcessedLineEnumeratorTests
{
    [Fact]
    public void EmptyText_NoLines()
    {
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateProcessedLines(""))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Empty(lines);
    }

    [Fact]
    public void Default_RemovesLeadingBlankLines()
    {
        var text = "\n\nLine 1\nLine 2";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateProcessedLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(2, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("Line 2", lines[1]);
    }

    [Fact]
    public void Default_RemovesTrailingBlankLines()
    {
        var text = "Line 1\nLine 2\n\n\n";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateProcessedLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(2, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("Line 2", lines[1]);
    }

    [Fact]
    public void Default_CollapsesInnerBlankLines()
    {
        var text = "Line 1\n\n\nLine 2";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateProcessedLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(3, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("", lines[1]);
        Assert.Equal("Line 2", lines[2]);
    }

    [Fact]
    public void Default_RemovesTrailingWhitespace()
    {
        var text = "Line 1  \nLine 2\t";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateProcessedLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(2, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("Line 2", lines[1]);
    }

    [Fact]
    public void PreserveLeading_KeepsLeadingBlankLines()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Preserve,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Preserve,
            InnerWhitespaceReplacement = ' ',
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Preserve,
            InnerBlankLineHandling = InnerBlankLineHandling.Preserve,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Remove,
            NewLine = "\n"
        };
        
        var text = "\n\nLine 1";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateProcessedLines(text, options))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(3, lines.Count);
        Assert.Equal("", lines[0]);
        Assert.Equal("", lines[1]);
        Assert.Equal("Line 1", lines[2]);
    }

    [Fact]
    public void PreserveTrailing_KeepsTrailingBlankLines()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Preserve,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Preserve,
            InnerWhitespaceReplacement = ' ',
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Remove,
            InnerBlankLineHandling = InnerBlankLineHandling.Preserve,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Preserve,
            NewLine = "\n"
        };
        
        var text = "Line 1\n\n\n";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateProcessedLines(text, options))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(3, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("", lines[1]);
        Assert.Equal("", lines[2]);
    }

    [Fact]
    public void RemoveInnerBlankLines()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Preserve,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Preserve,
            InnerWhitespaceReplacement = ' ',
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Remove,
            InnerBlankLineHandling = InnerBlankLineHandling.Remove,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Remove,
            NewLine = "\n"
        };
        
        var text = "Line 1\n\n\nLine 2";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateProcessedLines(text, options))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(2, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("Line 2", lines[1]);
    }

    [Fact]
    public void CollapseInnerWhitespace()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Remove,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Collapse,
            InnerWhitespaceReplacement = ' ',
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Remove,
            InnerBlankLineHandling = InnerBlankLineHandling.Remove,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Remove,
            NewLine = "\n"
        };
        
        var text = "  Line   1  \n  Line   2  ";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateProcessedLines(text, options))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(2, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("Line 2", lines[1]);
    }

    [Fact]
    public void OnlyWhitespaceText_RemoveLeading()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Remove,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Preserve,
            InnerWhitespaceReplacement = ' ',
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Remove,
            InnerBlankLineHandling = InnerBlankLineHandling.Remove,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Remove,
            NewLine = "\n"
        };
        
        var text = "   \n   \n   ";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateProcessedLines(text, options))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Empty(lines);
    }

    [Fact]
    public void MixedBlankAndVisible_CollapseInner()
    {
        var text = "Line 1\n\nLine 2\n\n\nLine 3\n\nLine 4";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateProcessedLines(text))
        {
            lines.Add(line.ToString());
        }
        
        // Default collapses consecutive blank lines to one
        Assert.Equal(7, lines.Count);
        Assert.Equal("Line 1", lines[0]);
        Assert.Equal("", lines[1]);
        Assert.Equal("Line 2", lines[2]);
        Assert.Equal("", lines[3]);
        Assert.Equal("Line 3", lines[4]);
        Assert.Equal("", lines[5]);
        Assert.Equal("Line 4", lines[6]);
    }

    [Fact]
    public void SingleLine_NoProcessingNeeded()
    {
        var text = "Hello World";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateProcessedLines(text))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Single(lines);
        Assert.Equal("Hello World", lines[0]);
    }

    [Fact]
    public void PreserveLeadingWhitespace()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Preserve,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Preserve,
            InnerWhitespaceReplacement = ' ',
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Remove,
            InnerBlankLineHandling = InnerBlankLineHandling.Remove,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Remove,
            NewLine = "\n"
        };
        
        var text = "  Line 1\n    Line 2";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateProcessedLines(text, options))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Equal(2, lines.Count);
        Assert.Equal("  Line 1", lines[0]);
        Assert.Equal("    Line 2", lines[1]);
    }

    [Fact]
    public void CustomInnerWhitespaceReplacement()
    {
        var options = new LineProcessingOptions
        {
            LeadingWhitespaceHandling = LeadingWhitespaceHandling.Remove,
            InnerWhitespaceHandling = InnerWhitespaceHandling.Collapse,
            InnerWhitespaceReplacement = "---",
            TrailingWhitespaceHandling = TrailingWhitespaceHandling.Remove,
            LeadingBlankLineHandling = LeadingBlankLineHandling.Remove,
            InnerBlankLineHandling = InnerBlankLineHandling.Remove,
            TrailingBlankLineHandling = TrailingBlankLineHandling.Remove,
            NewLine = "\n"
        };
        
        var text = "Hello   World";
        var lines = new List<string>();
        
        foreach (var line in LineProcessor.EnumerateProcessedLines(text, options))
        {
            lines.Add(line.ToString());
        }
        
        Assert.Single(lines);
        Assert.Equal("Hello---World", lines[0]);
    }
}
