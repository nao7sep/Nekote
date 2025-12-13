# Nekote.Text

Text processing utilities for parsing, escaping, and pattern matching.

## Current Segments

### Text Escaping (Complete)
- **EscapeMode.cs** - Enum defining escape strategies: KeyValue, CSV, URL, HTML
- **TextEscaper.cs** - Static escape/unescape methods for all four modes

### Key-Value Parsing (Complete)
- **KeyValueParser.cs** - Parse "key: value" format to Dictionary<string, string>
- **KeyValueWriter.cs** - Write Dictionary<string, string> to "key: value" format
- Simple INI-like configuration storage without sections, supports comments (#) and multi-line values

### Text Parsing (Complete)
- **ParagraphParser.cs** - Split text into paragraphs by blank lines
- Useful for document processing, markdown, logs, and any text with paragraph structure

## Planned Segments

### Text Parsing
- **SectionParser.cs** - Parse semantic sections with configurable markers (e.g., @AI, @TODO, custom prefixes)

### Text Matching
- **TextMatcher.cs** - Pattern matching (contains, regex, wildcard)
