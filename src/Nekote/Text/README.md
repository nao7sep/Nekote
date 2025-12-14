# Nekote.Text

Text processing utilities for parsing, escaping, and pattern matching.

## Current Segments

### Text Escaping
- **EscapeMode.cs** - Enum defining escape strategies: KeyValue, CSV, URL, HTML
- **TextEscaper.cs** - Static escape/unescape methods for all four modes
- Handles special characters, surrogate pairs (emoji), and culture-independent encoding

### Key-Value Parsing
- **KeyValueParser.cs** - Parse "key: value" format to Dictionary<string, string>
- **KeyValueWriter.cs** - Write Dictionary<string, string> to "key: value" format
- Supports # and // comments, uses TextEscaper for multi-line values

### Line and Paragraph Parsing
- **LineParser.cs** - Convert between text and line arrays, properly handling all line endings (\r\n, \n, \r)
- **ParagraphParser.cs** - Split text into paragraphs by blank lines
- Sequential scanning algorithm prevents string.Split issues with mixed line endings

### Typed String Conversion
- **TypedStringConverter.cs** - Culture-safe string↔type conversion (enforces InvariantCulture for portable config files)
- Converts between nullable strings and Int32, Int64, Double, Decimal, Bool, DateTime, Guid, TimeSpan, DateTimeOffset, Enum<T>, Uri, Version
- Rejects locale-specific formats (e.g., "1,5" German decimal) to ensure cross-platform compatibility

### Text Encoding
- **TextEncoding.cs** - Cached encoding instances for file operations
- Provides Utf8NoBom (recommended default), Utf8WithBom, Utf16, Utf32
- UTF-8 without BOM is the cross-platform standard (BOM breaks Unix tools, shebangs, parsers)
- All file operations in this namespace accept optional encoding parameter with Utf8NoBom as default

### Sectioned Key-Value Files
- **SectionParser.cs** - Parse sections with [name] (IniBrackets) or @name (AtPrefix) markers
- **SectionedKeyValueFile.cs** - High-level API for sectioned key-value files with typed getters/setters
- Line-based format: key:value pairs organized into sections, # and // comments, escaped values via TextEscaper
- Not INI-compatible: different syntax (key:value not key=value), different comment markers, includes escaping

## Planned Segments

### Text Matching
- **TextMatcher.cs** - Pattern matching (contains, regex, wildcard)
