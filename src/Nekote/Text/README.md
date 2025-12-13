# Nekote.Text

Text processing utilities for parsing, escaping, and pattern matching.

## Files in this Namespace

### Text Escaping
- **EscapeMode.cs** - Defines escape strategies (KeyValue, Csv, Url, Html)
- **TextEscaper.cs** - Static methods for escaping and unescaping text

### Future Additions
- **KeyValueParser.cs** - Parse Key:Value format files (depends on TextEscaper)
- **KeyValueWriter.cs** - Write Key:Value format files
- **ParagraphParser.cs** - Split text into paragraphs by blank lines
- **SectionParser.cs** - Parse semantic sections with markers
- **TextMatcher.cs** - Pattern matching utilities (contains, regex, wildcard)

## Purpose

This namespace provides text processing utilities that are:
- Complex enough to benefit from well-tested implementations
- General enough to be useful across multiple applications
- Difficult to generate correctly with edge cases
