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
- **LineParser.cs** - Convert between text and line arrays, properly handling all line endings (\r\n, \n, \r)
- **ParagraphParser.cs** - Split text into paragraphs by blank lines
- **SectionParser.cs** - Parse INI-style sections with [name] or @name markers, supports comments (# and //)
- Useful for document processing, markdown, logs, configuration files, and any text with paragraph or section structure

## Planned Segments

### Text Matching
- **TextMatcher.cs** - Pattern matching (contains, regex, wildcard)
