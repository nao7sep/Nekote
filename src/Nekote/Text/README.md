# Nekote.Text

Text processing utilities for parsing, escaping, and pattern matching.

## Current Segments

### Text Escaping (Complete)
- **EscapeMode.cs** - Enum defining escape strategies: KeyValue, CSV, URL, HTML
- **TextEscaper.cs** - Static escape/unescape methods for all four modes

## Planned Segments

### Text Parsing
- **KeyValueParser.cs** / **KeyValueWriter.cs** - Parse and write Key:Value format files
- **ParagraphParser.cs** - Split text into paragraphs by blank lines
- **SectionParser.cs** - Parse semantic sections with markers (e.g., @AI sections)

### Text Matching
- **TextMatcher.cs** - Pattern matching (contains, regex, wildcard)
