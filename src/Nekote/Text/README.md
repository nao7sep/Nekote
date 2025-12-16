# Nekote.Text

Text processing utilities for parsing, escaping, and pattern matching.

## Current Segments

### Text Encoding
- **TextEncoding.cs** - Cached encoding instances for optimized file operations.
  - Provides `Utf8NoBom` (recommended default), `Utf8WithBom`, `Utf16`, and `Utf32` instances.
  - Standardizes file operations to use UTF-8 without BOM by default for cross-platform compatibility.

### String Validation
- **StringValidator.cs** - Security-focused validation for strings in structured text formats.
  - Enforces strict rules for keys and section names to prevent ambiguity and homograph attacks.
  - Rejects dangerous characters (colons, newlines) and boundary whitespace that could obscure content.

### Typed String Conversion
- **TypedStringConverter.cs** - Robust, culture-independent conversion between strings and .NET types.
  - Handles `Int32`, `Int64`, `Double`, `Decimal`, `Bool`, `DateTime`, `Guid`, `TimeSpan`, `DateTimeOffset`, `Enum<T>`, `Uri`, and `Version`.
  - Enforces `InvariantCulture` to ensure configuration files are portable across different system locales (e.g., always uses `.` for decimals).

### Line and Paragraph Parsing
- **LineParser.cs** - High-performance line splitting and joining.
  - Uses sequential scanning to handle mixed line endings (`\r\n`, `\n`, `\r`) correctly within the same file.
  - Avoids common pitfalls of `string.Split` which can produce incorrect empty entries.
- **ParagraphParser.cs** - Text segmentation based on blank lines.
  - Splits text into paragraphs separated by one or more blank lines.
  - Preserves indentation and internal structure of paragraphs, making it suitable for content-heavy formats.

### Text Escaping
- **EscapeMode.cs** - Enumeration of supported escaping strategies (NiniValue, CSV, URL, HTML).
- **TextEscaper.cs** - Centralized escaping logic for multiple formats.
  - Handles special characters, surrogate pairs, and format-specific escape sequences (e.g. `\n`, `\"`).

### Nini Key-Value Parsing
- **NiniOptions.cs** - Configuration record for NINI format parsing and writing.
  - Defines parsing settings (separator, string comparers), output formatting (marker style, sorting), and file I/O options (encoding).
  - All properties use `required` modifier - no inline defaults. Values must be explicitly set during initialization.
  - Provides three predefined instances: `Default` (`: ` separator, @ markers), `TaskKiller` (`:` no space, no sections), `TraditionalIni` (`=` separator, [brackets]).
- **NiniKeyValueParser.cs** - Parser for the "key: value" line format.
  - Extracts key-value pairs while handling comments (`#`, `//`, `;`) and ignoring blank lines.
  - Unescapes values using `TextEscaper` to support multi-line content.
  - Uses `NiniOptions` for separator character and comparer configuration.
- **NiniKeyValueWriter.cs** - Generator for the "key: value" line format.
  - Serializes dictionaries to text, automatically escaping special characters.
  - Uses `NiniOptions` for output separator, sorting, and newline configuration.

### Nini Sectioned Key-Value Files
- **NiniSectionMarkerStyle.cs** - Definition of supported section markers (`[Section]`, `@Section`, and `None`).
- **NiniSection.cs** - Data structure representing a single named section and its key-value pairs.
- **NiniSectionParser.cs** - Parser for sectioned text content.
  - Identifies section boundaries using markers and delegates content parsing to `NiniKeyValueParser`.
  - Supports mixed marker styles within the same file.
- **NiniSectionWriter.cs** - Writer for sectioned text content.
  - Formats sections with markers and delegates key-value writing to `NiniKeyValueWriter`.
  - Handles preamble (unnamed sections), marker style selection, and section sorting.
  - Validates that `MarkerStyle.None` has no named sections.
- **NiniFile.cs** - The primary high-level API for working with NINI configuration files.
  - Provides a complete interface for loading, saving, querying, and modifying configuration data.
  - Offers strongly-typed accessors (`GetInt32`, `GetBool`, etc.) backed by `TypedStringConverter`.
  - Uses `NiniSectionParser` and `NiniSectionWriter` for I/O operations.
  - `Save()` and `ToString()` accept optional `outputOptions` parameter to override format without changing the instance's configuration.

---

## Considered but Not Implemented

This section documents features that were analyzed but deliberately excluded from Nekote.Text to maintain focus on general-purpose text processing utilities.

### TextMatcher - Pattern Matching Wrapper

**Rejected:** A wrapper for pattern matching (contains, regex, wildcard) that operates on isolated strings.

**Reasoning:**
- Provides no value over built-in .NET methods (`String.Contains`, `String.StartsWith`, `Regex.IsMatch`)
- Real-world use cases (email filtering, rule engines, content classification) require field-aware matching where patterns are applied to specific properties (e.g., "From starts with X" AND "Subject contains Y")
- Proper solution requires boolean composition (AND/OR/NOT), priority handling, and action execution - this belongs in a comprehensive rule engine system (future `Nekote.Rules` namespace), not as isolated text utilities
- Creating a thin wrapper would produce an API that doesn't solve actual problems and forces awkward workarounds

**Conclusion:** When rule-based filtering is needed, implement a proper rule engine with conditions, composition, and actions rather than string-only pattern matching.

### SectionParser - Custom Semantic Markers

**Rejected:** A parser for text with application-specific semantic markers (e.g., `@ AI-generated content @` in taskKiller).

**Reasoning:**
- The `@ ... @` marker pattern was designed as a workaround for embedding AI responses in plaintext notes for a specific application
- Not a general-purpose pattern - no identified use cases beyond the original application's constraints
- `NiniSectionParser` already handles standard sectioned formats (`[Section]` and `@Section` markers) for configuration files
- Application-specific text parsing logic belongs in application code, not in a general-purpose library

**Conclusion:** Nekote.Text is complete for general-purpose text processing. Domain-specific parsing belongs in application codebases.
