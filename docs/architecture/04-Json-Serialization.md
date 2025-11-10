# JSON Serialization Architecture

**Purpose:** Reusable JSON options for performance
**Layer:** Infrastructure
**Complexity:** ★☆☆☆☆ (Trivial - one static class)

---

## Overview

Static `JsonSerializerOptions` instances prevent repeated object creation and ensure consistency.

---

## JsonDefaults Class

```csharp
namespace Nekote.Core.AI.Infrastructure;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// JSON シリアライゼーションのデフォルトオプションを提供します。
/// </summary>
internal static class JsonDefaults
{
    /// <summary>
    /// 標準オプション (フォーマットなし、デシリアライズ用)。
    /// </summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// フォーマット付きオプション (インデント、診断データ記録用)。
    /// </summary>
    public static readonly JsonSerializerOptions FormattedOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };
}
```

---

## Usage Patterns

### Serialize Request (Formatted for Logs)

```csharp
var requestDto = OpenAiChatMapper.ToRequestDto(messages, options, modelName);
var json = JsonSerializer.Serialize(requestDto, JsonDefaults.FormattedOptions);

_logger.LogDebug("Sending request to OpenAI: {Json}", json);
```

### Deserialize Response (No Formatting)

```csharp
var responseJson = await response.Content.ReadAsStringAsync(cancellationToken)
    .ConfigureAwait(false);

var responseDto = JsonSerializer.Deserialize<OpenAiChatResponseDto>(
    responseJson,
    JsonDefaults.Options);
```

---

## Why Two Options?

1. **`Options`** (unformatted):
   - For **deserialization** (reading API responses)
   - For **production serialization** (sending to APIs)
   - Minimizes payload size

2. **`FormattedOptions`** (indented):
   - For **diagnostic logging** (readable JSON)
   - For **debugging** (troubleshooting API issues)
   - Makes logs human-readable

---

## Implementation Checklist

- [ ] Create `JsonDefaults.cs` in `/Infrastructure` root
- [ ] Make class `internal static`
- [ ] Define both `Options` and `FormattedOptions`
- [ ] XML comments in Japanese
- [ ] Verify works with all DTOs

---

**Estimated Time:** 5 minutes
**Dependencies:** None
**Next Step:** First Provider Implementation (05-OpenAI-Implementation.md)
