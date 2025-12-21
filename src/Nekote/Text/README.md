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
- **CharOrString.cs** - Unified representation for single-character or multi-character text values.
  - Discriminated union struct optimized for the common case of single characters while supporting strings.
  - Allocation-free (16 bytes), prevents default construction via constructors only accessible through `FromChar`/`FromString` or implicit operators.

### Structural Parsing
- **LineParser.cs** - High-performance line splitting and joining.
  - **Invariants**: Treats line endings as **terminators** (POSIX-style), not separators. A trailing line ending does *not* produce a final empty line.
  - Manually scans for `\r`, `\n`, and `\r\n` to handle mixed line-ending conventions within a single source correctly.
- **ParagraphParser.cs** - Text segmentation based on blank line boundaries.
  - Relationship: Uses `LineParser` to identify logical paragraphs while preserving internal indentation.

### Line Processing
- **LineProcessingOptions.cs** - Configuration record for line-level text processing operations.
  - Record type with `required` properties - all whitespace and blank line handling settings must be explicitly initialized.
  - Provides three presets: `Default` (normalize blank lines, remove trailing whitespace), `SingleLine` (flatten to single line with spaces), `Minimal` (aggressive removal, empty separator).
  - Configures behavior for leading/inner/trailing whitespace and leading/inner/trailing blank lines.
- **LeadingWhitespaceHandling.cs** - Enumeration for leading whitespace behavior (`Preserve`, `Remove`).
- **InnerWhitespaceHandling.cs** - Enumeration for consecutive inner whitespace behavior (`Preserve`, `Collapse`, `Remove`).
- **TrailingWhitespaceHandling.cs** - Enumeration for trailing whitespace behavior (`Preserve`, `Remove`).
- **LeadingBlankLineHandling.cs** - Enumeration for leading blank line behavior (`Preserve`, `Remove`).
- **InnerBlankLineHandling.cs** - Enumeration for consecutive inner blank line behavior (`Preserve`, `Collapse`, `Remove`).
- **TrailingBlankLineHandling.cs** - Enumeration for trailing blank line behavior (`Preserve`, `Remove`).
- **LineEnumerator.cs** - Allocation-free iteration over lines in text spans.
  - Ref struct that yields `ReadOnlySpan<char>` for each line without creating string allocations.
  - Relationship: Delegates line reading to `LineProcessor.TryReadLine`.
- **ProcessedLineEnumerator.cs** - Allocation-optimized iteration with whitespace and blank line processing.
  - Ref struct that applies `LineProcessingOptions` during enumeration using a three-phase model (leading blank lines, content, trailing blank lines).
  - Uses `StringBuilder` for lines requiring modification; returns slices of original input when possible for zero-allocation fast path.
  - **Invariants**: Tracks blank line sequences to implement collapse logic; resets state when transitioning between phases.
- **LineProcessor.cs** - Core line processing utilities and orchestration.
  - **Invariants**: Treats line endings as **terminators** (consistent with `LineParser`). `EnumerateLines` and `CountLines` exclude the final empty line if the text ends with a newline.
  - All whitespace detection uses `char.IsWhiteSpace()` including Unicode whitespace; zero-width characters (U+200B) are NOT considered whitespace per .NET contract.
  - Provides low-level primitives: `TryReadLine` (mixed line-ending support), `IsBlank` (whitespace detection), `GetLeadingWhitespace`, `GetTrailingWhitespace`, `CountLines`.
  - Provides high-level processing: `ProcessLine` (single-line whitespace handling), `SplitIntoSections` (separates leading/content/trailing regions), `Process` (full text processing with options).
  - Relationship: Central coordinator that is used by `LineEnumerator` and `ProcessedLineEnumerator`; delegates escaping to `TextEscaper` when needed.

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
