# Nekote.Text

Text processing utilities for parsing, escaping, and pattern matching.

## Current Segments

### Text Encoding
- **TextEncoding.cs** - Cached encoding instances for file operations
  - Provides Utf8NoBom (recommended default), Utf8WithBom, Utf16, Utf32
  - UTF-8 without BOM is the cross-platform standard (BOM breaks Unix tools, shebangs, parsers)
  - All file operations in this namespace accept optional encoding parameter with Utf8NoBom as default

### String Validation
- **StringValidator.cs** - Validates strings for use in structured text formats
  - Ensures keys and section names don't have leading/trailing whitespace (prevents ambiguity and attacks)
  - Validates keys don't contain format-breaking characters (colons, newlines, comment markers, section markers)
  - Security-focused: whitespace at boundaries can be used as attack vectors (homograph attacks, "key " vs "key")

### Typed String Conversion
- **TypedStringConverter.cs** - Culture-safe string↔type conversion (enforces InvariantCulture for portable config files)
  - Converts between nullable strings and Int32, Int64, Double, Decimal, Bool, DateTime, Guid, TimeSpan, DateTimeOffset, Enum<T>, Uri, Version
  - Rejects locale-specific formats (e.g., "1,5" German decimal) to ensure cross-platform compatibility

### Line and Paragraph Parsing
- **LineParser.cs** - Convert between text and line arrays, properly handling all line endings (\r\n, \n, \r)
  - Sequential scanning algorithm prevents string.Split issues with mixed line endings
- **ParagraphParser.cs** - Split text into paragraphs by blank lines
  - Preserves indentation and whitespace within paragraphs (does not trim)

### Text Escaping
- **EscapeMode.cs** - Enum defining escape strategies: NiniValue, CSV, URL, HTML
- **TextEscaper.cs** - Static escape/unescape methods for all four modes
  - Handles special characters, surrogate pairs (emoji), and culture-independent encoding

### Nini Key-Value Parsing
- **NiniKeyValueParser.cs** - Parse "key: value" format to Dictionary<string, string>
  - Supports # and // comments, uses TextEscaper for multi-line values
- **NiniKeyValueWriter.cs** - Write Dictionary<string, string> to "key: value" format

### Nini Sectioned Key-Value Files
- **NiniSectionMarkerStyle.cs** - Enum for section marker styles
- **NiniSection.cs** - Represents a parsed section
- **NiniSectionParser.cs** - Parse sections with [name] (IniBrackets) or @name (AtPrefix) markers
- **NiniFile.cs** - High-level API for sectioned key-value files with typed getters/setters
  - Line-based format: key:value pairs organized into sections, # and // comments, escaped values via TextEscaper
  - Not INI-compatible: different syntax (key:value not key=value), different comment markers, includes escaping

## Planned Segments

### Text Matching
- **TextMatcher.cs** - Pattern matching (contains, regex, wildcard)
