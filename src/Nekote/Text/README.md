# Nekote.Text

Provides robust, culture-safe text processing utilities and a high-performance, specification-compliant parser for the NINI configuration format.

## Segments

### Foundational Utilities
- **TextEncoding.cs** - Cached encoding instances for optimized file operations.
  - Provides `Utf8NoBom` (recommended default), `Utf8WithBom`, `Utf16`, and `Utf32` to avoid repeated allocation.
- **TextEscaper.cs** - Centralized escaping logic for HTML, URL, CSV, and NINI values.
  - **Invariants**: Delegates to .NET BCL (`WebUtility`, `Uri`) for HTML and URL compliance; implements custom logic for CSV (RFC 4180) and NiniValue (backslash sequences).
- **EscapeMode.cs** - Defines the supported escaping strategies.
- **TypedStringConverter.cs** - Culture-independent translation between strings and .NET types.
  - **Invariants**: Strictly enforces `InvariantCulture` to prevent locale-dependent configuration parsing bugs (e.g., decimal separator issues).
- **StringValidator.cs** - Security-focused validation for format identifiers.
  - Rejects leading/trailing whitespace and invalid characters (separators, comment markers, section markers) to prevent ambiguity and format conflicts.

### Structural Parsing
- **LineParser.cs** - High-performance line splitting and joining.
  - Manually scans for `\r`, `\n`, and `\r\n` to handle mixed line-ending conventions within a single source correctly.
- **ParagraphParser.cs** - Text segmentation based on blank line boundaries.
  - Relationship: Uses `LineParser` to identify logical paragraphs while preserving internal indentation.

### NINI Key-Value Logic
- **NiniOptions.cs** - Configuration record for the NINI format.
  - **Invariants**: All properties are `required`. Includes `taskKiller` preset (lowercase name is intentional to match the official product name).
- **NiniKeyValueParser.cs** - Logic for extracting pairs from "key: value" lines.
  - **Invariants**: Enforces strict "no indentation" rules; keys must start at column 0.
- **NiniKeyValueWriter.cs** - Generator for "key: value" formatted text.
  - Relationship: Uses `TextEscaper` to ensure multi-line values are safely encoded.

### NINI Sectioned Files (Public API)
- **NiniSection.cs** - Data model for a named configuration segment.
- **NiniSectionMarkerStyle.cs** - Defines visual styles for section headers (`[Section]` vs `@Section`).
- **NiniSectionParser.cs** - Orchestrates paragraph-level parsing into sections.
  - Relationship: Delegates individual paragraph content parsing to `NiniKeyValueParser`.
- **NiniSectionWriter.cs** - Formats multiple sections into a cohesive document.
  - **Invariants**: Inserts `# (empty section)` comments for sections with no keys, enabling round-trip preservation (parse → write → parse yields identical structure).
- **NiniFile.cs** - The primary high-level interface for configuration management.
  - Relationship: Bridges `NiniSection` models with `TypedStringConverter` to provide the end-user API for typed data access.
