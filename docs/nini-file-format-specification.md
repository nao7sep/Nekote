# NINI File Format Specification

**Version:** 1.0
**Date:** December 14, 2025
**Author:** Yoshinao Inoguchi

## 1. Overview

NINI is a line-based configuration file format designed for clarity, portability, and security. The name is a playful combination of:
- **Not INI** - deliberately different from traditional INI format
- **New INI** - a modern take on simple configuration
- **Nekote INI** - part of the Nekote text processing library

NINI organizes key-value pairs into named sections with a focus on culture-safe parsing, explicit escaping, and protection against common security vulnerabilities (homograph attacks, injection attacks).

### 1.1 Key Differences from Traditional INI

| Feature | Traditional INI | NINI |
|---------|----------------|------|
| **Separator** | `key=value` | `key: value` (colon + space) |
| **Comments** | `;` (semicolon) | `#`, `//`, and `;` |
| **Section Markers** | `[Section]` only | `[Section]` or `@Section` |
| **Escaping** | Varies, often none | Explicit backslash escaping |
| **Multi-line Values** | Varies, often not supported | Supported via `\n` escape sequences |
| **Whitespace** | Often trimmed automatically | Validated for security |
| **Encoding** | Varies | UTF-8 without BOM (default) |
| **Culture** | Locale-dependent | Invariant culture for portability |

### 1.2 Design Goals

1. **Clarity**: Human-readable and unambiguous syntax
2. **Security**: Explicit validation to prevent whitespace-based attacks
3. **Portability**: Culture-independent parsing (InvariantCulture)
4. **Simplicity**: Minimal syntax rules, easy to implement
5. **Type-Safety**: Built-in support for typed value access

## 2. File Structure

A NINI file consists of **paragraphs** separated by blank lines. Each paragraph is either:
- **Preamble**: Key-value pairs without a section marker
- **Named Section**: A section marker followed by key-value pairs

### 2.1 Basic Example

```
# Preamble - keys without section marker
AppName: MyApplication
Version: 1.2.3

@Database
Host: localhost
Port: 5432
ConnectionString: Server=localhost;Port=5432;Database=mydb

@Logging
Level: Information
OutputPath: /var/log/app.log
```

### 2.2 Paragraph Structure

- Paragraphs are separated by **blank lines** (empty lines or lines containing only whitespace)
- Consecutive blank lines are treated as a single separator
- Leading and trailing blank lines in the file are ignored

## 3. Section Markers

Sections organize related key-value pairs. NINI supports two marker styles:

### 3.1 At-Prefix Style (Recommended)

```
@SectionName
```

- Section name starts immediately after `@`
- No trailing punctuation required
- Must start at column 0 (no leading whitespace)

### 3.2 INI Brackets Style

```
[SectionName]
```

- Section name is enclosed in square brackets
- Must start at column 0 (no leading whitespace)
- Compatible with traditional INI tools (for reading only)

### 3.3 Section Marker Rules

1. **Column 0 Only**: Section markers must start at the first character of the line
2. **No Leading/Trailing Whitespace**: Section names cannot have whitespace at boundaries
   - ❌ `@ SectionName` (space after @)
   - ❌ `@SectionName ` (trailing space)
   - ✅ `@SectionName`
3. **No Empty Names**: Empty section markers are not allowed
   - ❌ `@` or `[]`
4. **No Inline Comments**: Comments on the same line as section markers are not supported
   - ❌ `@Database # production config`
   - ✅ Use separate lines for comments
5. **Case-Insensitive**: Section names are compared using case-insensitive ordinal comparison
6. **Duplicate Sections**: Multiple paragraphs with the same section name are merged
   - Last value for a key wins

### 3.4 Preamble (No Section Marker)

Key-value pairs without a section marker belong to the **preamble** (unnamed section with empty string name `""`).

```
# This is in the preamble
GlobalSetting: value

@NamedSection
SectionSetting: value
```

## 4. Key-Value Pairs

### 4.1 Syntax

```
Key: Value
```

- **Key**: Identifier starting at column 0
- **Separator**: Colon `:` immediately after key (no space before colon)
- **Value**: Text after colon, leading/trailing whitespace trimmed

### 4.2 Key Rules

1. **Column 0 Only**: Keys must start at the first character of the line (no indentation)
2. **No Leading/Trailing Whitespace**: Keys cannot have whitespace at boundaries
3. **No Colons**: Keys cannot contain `:` character
4. **No Line Breaks**: Keys cannot contain `\n` or `\r`
5. **No Comment Markers**: Keys cannot start with `#`, `//`, `[`, or `@`
6. **Case-Insensitive**: Keys are compared using case-insensitive ordinal comparison
7. **No Duplicates**: Within a section, duplicate keys are not allowed

#### Valid Keys
```
Host: localhost
DatabasePort: 5432
API_URL: https://api.example.com
Connection_String_1: value
```

#### Invalid Keys
```
 Host: localhost           ❌ Leading space
Host : localhost           ❌ Space before colon
Host\nName: value          ❌ Contains line break
#Secret: value             ❌ Starts with comment marker
[Name]: value              ❌ Starts with bracket
@Property: value           ❌ Starts with @
```

### 4.3 Value Rules

1. **Whitespace Trimmed**: Leading and trailing whitespace in values is automatically removed
2. **Empty Values**: Empty values are allowed (represents empty string, not null)
3. **Escaping Required**: Special characters must be escaped (see section 5)
4. **No Null**: The format cannot distinguish between null and empty string
   - Both serialize as `Key: `
   - Null values are rejected by the API

#### Examples
```
EmptyValue:                          # Empty string
SpacedValue:     trimmed             # Leading/trailing spaces removed
EscapedNewline: First line\nSecond   # Multi-line value via escaping
```

## 5. Escaping

NINI uses backslash escaping for special characters in values.

### 5.1 Escape Sequences

| Sequence | Character | Description |
|----------|-----------|-------------|
| `\\` | `\` | Backslash (literal) |
| `\n` | LF (U+000A) | Line feed (newline) |
| `\r` | CR (U+000D) | Carriage return |
| `\t` | TAB (U+0009) | Horizontal tab |

### 5.2 Escaping Rules

1. **Values Only**: Only values are escaped, not keys or section names
2. **Automatic**: Escaping/unescaping is automatic when using the API
3. **No Unicode Escapes**: Unicode characters are stored directly in UTF-8
4. **Backslash Must Be Escaped**: Literal backslash must be written as `\\`

### 5.3 Examples

#### Writing Multi-line Values
```
Description: This is line one\nThis is line two\nThis is line three
Path: C:\\Users\\Documents\\file.txt
Message: He said: "Hello"     # Note: quotes don't need escaping
```

#### Raw Text Appearance in File
```
Description: This is line one\nThis is line two
Path: C:\\Users\\Documents\\file.txt
```

#### After Parsing (In-Memory)
```
Description → "This is line one
This is line two"

Path → "C:\Users\Documents\file.txt"
```

## 6. Comments

### 6.1 Comment Styles

NINI supports three comment styles for maximum compatibility:

```
# Hash-style comment (NINI native)
// Slash-style comment (NINI native)
; Semicolon-style comment (INI compatible)
```

### 6.2 Comment Rules

1. **Column 0 Only**: Comments must start at the first character of the line
2. **Full-Line Only**: Inline comments (after keys or values) are not supported
   - ❌ `Host: localhost # production`
   - ✅ Use separate lines for comments
3. **No Indentation**: Comments with leading whitespace are treated as syntax errors

### 6.3 Examples

```
# This is a comment
// This is also a comment
; This is an INI-compatible comment

@Section
# Comment within section
Key: Value

// Multiple comment lines
; can be placed together
# using any style
AnotherKey: AnotherValue
```

## 7. Encoding and Line Endings

### 7.1 Character Encoding

- **Default**: UTF-8 without BOM (Byte Order Mark)
- **Rationale**:
  - Cross-platform compatibility (BOM breaks Unix tools, shebangs, parsers)
  - Industry standard for text files
  - Full Unicode support including emoji and non-BMP characters
- **Alternative**: UTF-8 with BOM, UTF-16, UTF-32 (supported but not recommended)

### 7.2 Line Endings

NINI accepts all line ending styles:
- **Windows**: `\r\n` (CRLF)
- **Unix/Linux**: `\n` (LF)
- **Classic Mac**: `\r` (CR)
- **Mixed**: Files with mixed line endings are handled correctly

Line endings in the file are normalized during parsing and are **not preserved**. When saving, the library uses the newline sequence and marker style specified by the user.

## 8. Data Types

NINI supports typed value access through the API, with automatic conversion using `InvariantCulture` for portability.

### 8.1 Supported Types

| Type | Format | Example |
|------|--------|---------|
| **String** | Raw text | `Name: John Doe` |
| **Int32** | Decimal integer | `Port: 8080` |
| **Int64** | Decimal integer | `MaxSize: 9223372036854775807` |
| **Double** | Decimal with dot | `Pi: 3.14159` |
| **Decimal** | Decimal with dot | `Price: 19.99` |
| **Boolean** | true/false (case-insensitive) | `Enabled: true` |
| **DateTime** | ISO 8601 or round-trip | `Created: 2025-12-14T10:30:00Z` |
| **DateTimeOffset** | ISO 8601 with offset | `Timestamp: 2025-12-14T10:30:00+09:00` |
| **TimeSpan** | hh:mm:ss or d.hh:mm:ss | `Timeout: 00:05:00` |
| **Guid** | 32 hex digits with hyphens | `Id: 550e8400-e29b-41d4-a716-446655440000` |
| **Uri** | Absolute or relative URI | `Url: https://example.com/api` |
| **Version** | Major.Minor[.Build[.Revision]] | `AppVersion: 1.2.3.4` |
| **Enum** | Enum member name | `LogLevel: Information` |

### 8.2 Culture-Independent Parsing

**Critical**: All numeric and date parsing uses `InvariantCulture` to ensure cross-platform compatibility.

#### Why This Matters
```
# WRONG: Locale-dependent (Germany uses comma as decimal separator)
Price: 19,99     # Parsed as 1999 in InvariantCulture!

# CORRECT: Always use dot as decimal separator
Price: 19.99     # Parsed correctly everywhere
```

### 8.3 Type Conversion Examples

```
@Settings
Port: 8080                                  # Int32
MaxConnections: 1000                        # Int32
MaxFileSize: 9223372036854775807           # Int64
EnableLogging: true                         # Boolean
Timeout: 00:05:00                          # TimeSpan (5 minutes)
CreatedDate: 2025-12-14T10:30:00Z          # DateTime (UTC)
Price: 29.99                               # Decimal
ApiUrl: https://api.example.com            # Uri
AppVersion: 1.2.3                          # Version
SessionId: 550e8400-e29b-41d4-a716-446655440000  # Guid
LogLevel: Information                       # Enum
```

## 9. API Usage Examples

### 9.1 Reading a NINI File

```csharp
using Nekote.Text;

// Load file
var config = NiniFile.Load("config.nini");

// Read typed values with defaults
string host = config.GetString("Database", "Host", defaultValue: "localhost");
int port = config.GetInt32("Database", "Port", defaultValue: 5432);
bool enableSsl = config.GetBool("Database", "EnableSSL", defaultValue: false);

// Read with section indexer
var dbSection = config["Database"];
string connectionString = dbSection["ConnectionString"];
```

### 9.2 Creating and Writing a NINI File

```csharp
using Nekote.Text;

// Create new file
var config = new NiniFile(NiniSectionMarkerStyle.AtPrefix);

// Set values (creates sections automatically)
config.SetString("", "AppName", "MyApplication");
config.SetString("", "Version", "1.0.0");

config.SetString("Database", "Host", "localhost");
config.SetInt32("Database", "Port", 5432);
config.SetBool("Database", "EnableSSL", true);

config.SetEnum("Logging", "Level", LogLevel.Information);
config.SetString("Logging", "OutputPath", "/var/log/app.log");

// Save file
config.Save("config.nini");
```

### 9.3 Generated File Content

```
AppName: MyApplication
Version: 1.0.0

@Database
Host: localhost
Port: 5432
EnableSSL: True

@Logging
Level: Information
OutputPath: /var/log/app.log
```

### 9.4 Parsing and Modifying

```csharp
// Parse from string
string content = File.ReadAllText("config.nini");
var config = NiniFile.Parse(content);

// Check if section exists
if (config.HasSection("Database"))
{
    // Modify existing value
    config.SetString("Database", "Host", "db.example.com");

    // Remove a key
    config.RemoveValue("Database", "OldKey");
}

// Add new section
config.SetString("Cache", "Type", "Redis");
config.SetString("Cache", "Host", "cache.example.com");

// Remove entire section
config.RemoveSection("Logging");

// Save changes
config.Save("config.nini");
```

### 9.5 Iterating Sections and Keys

```csharp
// Get all section names
foreach (var sectionName in config.GetSectionNames())
{
    Console.WriteLine($"[{sectionName}]");

    // Get section dictionary
    if (config.TryGetSection(sectionName, out var section))
    {
        foreach (var (key, value) in section)
        {
            Console.WriteLine($"  {key}: {value}");
        }
    }
}
```

## 10. Security Considerations

### 10.1 Whitespace Validation

NINI strictly validates whitespace at boundaries to prevent security vulnerabilities:

**Homograph Attack Prevention**:
```
Key: value      # Valid
Key : value     # Invalid - trailing space in key could hide malicious content
 Key: value     # Invalid - leading space allows "invisible" keys
```

**Why This Matters**:
- Trailing whitespace can make "Key " and "Key" appear identical to humans
- Leading whitespace can be used to hide malicious configuration entries
- Attackers can use Unicode lookalike characters combined with whitespace

### 10.2 Format Injection Protection

**Key Validation** prevents injection attacks:
```
@Section: DROP TABLE users;  # Invalid - @ cannot start a key
Key\n@Admin: true            # Invalid - keys cannot contain line breaks
Key: [NewSection]            # Invalid - brackets cannot start a key
```

### 10.3 Null Safety

The format explicitly rejects null values:
- Cannot distinguish between null and empty string (both serialize as `Key: `)
- API throws `ArgumentNullException` when attempting to set null
- Use `string.Empty` explicitly for empty values

### 10.4 Best Practices

1. **Validate Input**: Always validate configuration values after parsing
2. **Use Typed Access**: Prefer `GetInt32()`, `GetBool()`, etc. over raw string access
3. **Default Values**: Always provide sensible defaults for optional settings
4. **Access Control**: Restrict write access to configuration files
5. **Audit Changes**: Log configuration changes in production systems

## 11. Implementation Notes

### 11.1 Parsing Strategy

NINI uses a two-phase parsing approach:

1. **Paragraph Splitting**: Text is split into paragraphs by blank lines
2. **Section Parsing**: Each paragraph is parsed as a potential section:
   - First line checked for section marker
   - Remaining lines parsed as key-value pairs

This design ensures that:
- Mixed line endings are handled correctly
- Blank lines reliably separate sections
- No ambiguity between content and structure

### 11.2 Case Sensitivity

- **Section Names**: Case-insensitive (ordinal comparison)
- **Keys**: Case-insensitive (ordinal comparison)
- **Values**: Case-sensitive (preserved exactly as written)

### 11.3 Duplicate Handling

**Duplicate Sections**: Merged together (last value wins)
```
@Database
Host: server1

@Database
Host: server2    # This wins
Port: 5432
```

**Duplicate Keys**: Not allowed within a section (throws `ArgumentException`)

### 11.4 Surrogate Pairs and Unicode

NINI is **surrogate-pair safe**:
- Emoji and other non-BMP characters are supported: 👍, 🚀, 𝕳𝖊𝖑𝖑𝖔
- Validation checks target ASCII characters only (`:`, `#`, `[`, `@`, etc.)
- These ASCII characters never appear in surrogate pairs
- `char.IsWhiteSpace()` is safe (all Unicode whitespace is in BMP)

## 12. Comparison with Other Formats

### 12.1 NINI vs INI

| Feature | INI | NINI |
|---------|-----|------|
| Separator | `=` | `:` |
| Comments | `;` | `#` and `//` |
| Escaping | None/varies | Explicit `\n`, `\\`, etc. |
| Multi-line | No/varies | Yes (via escaping) |
| Standardized | No | Yes |
| Type Safety | No | Yes (via API) |

### 12.2 NINI vs JSON

| Feature | JSON | NINI |
|---------|------|------|
| Human-readable | Moderate | High |
| Comments | No | Yes |
| Nested structures | Yes | No (flat sections only) |
| Arrays | Yes | No |
| Type safety | Yes | Yes (via API) |
| Edit-friendly | Moderate | High |

### 12.3 NINI vs YAML

| Feature | YAML | NINI |
|---------|------|------|
| Complexity | High | Low |
| Indentation-sensitive | Yes | No |
| Nested structures | Yes | No (flat sections only) |
| Multi-line strings | Yes | Yes (via escaping) |
| Learning curve | Steep | Minimal |

### 12.4 NINI vs TOML

| Feature | TOML | NINI |
|---------|------|------|
| Separator | `=` | `:` |
| Nested structures | Yes (tables) | No (flat sections) |
| Arrays | Yes | No |
| Type syntax | Explicit | Implicit (via API) |
| Complexity | Moderate | Low |

## 13. Grammar (EBNF-like)

```
File           = Paragraph { BlankLine Paragraph }
Paragraph      = [ SectionMarker NewLine ] KeyValueLine { NewLine KeyValueLine }
BlankLine      = { Whitespace } NewLine
SectionMarker  = AtPrefix | IniBrackets
AtPrefix       = "@" SectionName
IniBrackets    = "[" SectionName "]"
KeyValueLine   = Key ":" [ Whitespace ] Value
CommentLine    = ( "#" | "//" ) { AnyChar }
Key            = KeyChar { KeyChar }
Value          = { ValueChar }
SectionName    = NameChar { NameChar }

KeyChar        = Any Unicode except ':', '\n', '\r', whitespace at boundaries
ValueChar      = Any Unicode (escaped via \n, \\, \r, \t)
NameChar       = Any Unicode except whitespace at boundaries
NewLine        = "\n" | "\r\n" | "\r"
Whitespace     = " " | "\t"
```

## 14. Recommendations

### 14.1 When to Use NINI

✅ **Good Use Cases**:
- Application configuration files
- Simple key-value data storage
- User preferences and settings
- Deployment configuration
- Build configuration
- Flat data structures

❌ **Not Recommended For**:
- Complex nested data (use JSON or YAML)
- Data interchange between systems (use JSON)
- Large datasets (use database)
- Arrays or lists (use JSON)
- Machine-generated configuration (use JSON)

### 14.2 Style Guide

1. **Section Markers**: Use `@` prefix style (more modern, cleaner)
2. **File Extension**: `.nini` or `.conf`
3. **Encoding**: UTF-8 without BOM
4. **Line Endings**: LF (Unix-style) for cross-platform compatibility
5. **Comments**: Use `#` for consistency with shell scripts
6. **Key Naming**: Use PascalCase or snake_case consistently
7. **Blank Lines**: One blank line between sections
8. **Ordering**: Preamble first, then alphabetize sections

### 14.3 Example Configuration File

```
# Application Configuration
# Generated: 2025-12-14

AppName: MyApplication
Version: 1.2.3
Environment: Production

@Database
Host: db.example.com
Port: 5432
Database: myapp_prod
Username: app_user
# Password stored in secrets vault
ConnectionTimeout: 00:00:30
EnableSSL: true

@Cache
Provider: Redis
Host: cache.example.com
Port: 6379
TTL: 00:15:00

@Logging
Level: Information
OutputFormat: JSON
OutputPath: /var/log/myapp.log
MaxFileSizeMB: 100
RetentionDays: 30

@Features
EnableNewUI: true
EnableBetaFeatures: false
MaxConcurrentUsers: 1000
```

## 15. Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification |

## 16. References

- **Implementation**: Nekote.Text library (C#/.NET)
- **Repository**: https://github.com/nao7sep/Nekote
- **License**: GPL-3.0-or-later

---

**NINI** - Simple, secure, and straightforward configuration format.
