# Implementation Roadmap - Easiest First

**Goal:** Get to working code as fast as possible, then iterate
**Strategy:** Implement minimal viable product (MVP) first, add complexity later

---

## Phase 1: Foundation (Easiest) ⭐ START HERE

These have **zero external dependencies** and are pure C# classes.

| Step | Document | Time | Complexity | Why Easy? |
|------|----------|------|------------|-----------|
| 1 | `01-Domain-Models.md` | 30 min | ★☆☆☆☆ | Just POCOs, no logic |
| 2 | `02-Domain-Interfaces.md` | 15 min | ★★☆☆☆ | Just interface declarations |
| 3 | `03-Configuration.md` | 20 min | ★★☆☆☆ | Just config classes, no logic |
| 4 | `04-Json-Serialization.md` | 5 min | ★☆☆☆☆ | One static class |

**Total Time:** ~70 minutes
**Deliverable:** Complete Domain layer + Configuration classes

---

## Phase 2: First Working Integration (Moderate)

This is where we **actually call an AI API**.

| Step | Document | Time | Complexity | Notes |
|------|----------|------|------------|-------|
| 5 | `05-OpenAI-Implementation.md` | 2-3 hours | ★★★☆☆ | DTOs + Mapper + Repository + DI |

**Deliverable:** Working OpenAI Chat integration (console app that prints AI responses)

---

## Phase 3: Testing (Easy)

Now that we have working code, we add tests.

| Step | Task | Time | Complexity |
|------|------|------|------------|
| 6 | Unit tests for Mapper | 30 min | ★★☆☆☆ |
| 7 | Integration tests with mocked HTTP | 1 hour | ★★★☆☆ |

**Deliverable:** Tested, reliable OpenAI integration

---

## Phase 4: Second Provider (Easier Now)

Copy the OpenAI pattern for a second provider.

| Step | Task | Time | Complexity | Notes |
|------|------|------|------------|-------|
| 8 | Implement Gemini Chat | 1-2 hours | ★★☆☆☆ | Copy-paste from OpenAI, adjust DTOs |

**Deliverable:** Multi-provider support proven

---

## Phase 5: Advanced Features (Add When Needed)

These add complexity but aren't required for basic functionality.

| Feature | Time | Complexity | Priority |
|---------|------|------------|----------|
| Streaming responses | 2-3 hours | ★★★★☆ | Medium |
| Diagnostics/Tracking | 2-3 hours | ★★★☆☆ | Low (use ILogger for now) |
| Caching decorator | 1-2 hours | ★★★☆☆ | Medium |
| Embedding service | 1-2 hours | ★★☆☆☆ | High (for RAG) |
| Translation service | 1-2 hours | ★★☆☆☆ | Low |
| Retry policies | 2-3 hours | ★★★☆☆ | Low |
| Remaining providers (4) | 4-6 hours | ★★☆☆☆ | Low |

---

## Why This Order?

### Phase 1 First (Domain + Config)
✅ **No external APIs** - no API keys needed
✅ **No HTTP calls** - no network issues
✅ **Instant compilation** - see results immediately
✅ **Foundation for everything else** - other code depends on this

### Phase 2 Next (OpenAI)
✅ **Proves the architecture works** - real API integration
✅ **OpenAI has best docs** - easiest to implement
✅ **Most common provider** - highest value
✅ **Motivating milestone** - you can see it work!

### Phase 3 Testing
✅ **Prevents regression** - keep quality high
✅ **Documents behavior** - tests are documentation
✅ **Builds confidence** - know it works before adding complexity

### Phase 4 Second Provider
✅ **Validates architecture** - proves it's provider-agnostic
✅ **Easier than first** - pattern is established
✅ **Small incremental change** - low risk

### Phase 5 Only When Needed
✅ **Don't build what you don't need** - YAGNI principle
✅ **Real requirements drive design** - avoid over-engineering
✅ **Complexity is optional** - core functionality is done

---

## File Structure After Phase 2

```
/Nekote.Core
├─ /AI
│   ├─ /Domain
│   │   ├─ /Chat
│   │   │   ├─ IChatCompletionService.cs       ✅ Phase 1
│   │   │   ├─ ChatMessage.cs                  ✅ Phase 1
│   │   │   ├─ ChatResponse.cs                 ✅ Phase 1
│   │   │   ├─ ChatRole.cs                     ✅ Phase 1
│   │   │   ├─ ChatCompletionOptions.cs        ✅ Phase 1
│   │   │   └─ TokenUsage.cs                   ✅ Phase 1
│   │   ├─ /Embedding
│   │   │   ├─ ITextEmbeddingService.cs        ✅ Phase 1
│   │   │   └─ EmbeddingResult.cs              ✅ Phase 1
│   │   └─ /Translation
│   │       ├─ ITranslationService.cs          ✅ Phase 1
│   │       ├─ TranslationRequest.cs           ✅ Phase 1
│   │       └─ TranslationResult.cs            ✅ Phase 1
│   │
│   └─ /Infrastructure
│       ├─ JsonDefaults.cs                     ✅ Phase 1
│       │
│       ├─ /OpenAI
│       │   ├─ OpenAiConfiguration.cs          ✅ Phase 1
│       │   └─ /Chat
│       │       ├─ OpenAiChatRepository.cs     ✅ Phase 2
│       │       ├─ OpenAiChatMapper.cs         ✅ Phase 2
│       │       └─ /Dtos
│       │           ├─ OpenAiChatRequestDto.cs ✅ Phase 2
│       │           ├─ OpenAiChatResponseDto.cs✅ Phase 2
│       │           ├─ OpenAiMessageDto.cs     ✅ Phase 2
│       │           ├─ OpenAiChoiceDto.cs      ✅ Phase 2
│       │           └─ OpenAiUsageDto.cs       ✅ Phase 2
│       │
│       ├─ /Gemini
│       │   └─ GeminiConfiguration.cs          ✅ Phase 1
│       │
│       └─ /DependencyInjection
│           └─ OpenAiServiceCollectionExtensions.cs ✅ Phase 2
```

---

## Quick Start Commands

### After Phase 1 (Domain + Config)

```powershell
# Compile and verify
dotnet build src/Nekote.Core/Nekote.Core.csproj
```

### After Phase 2 (OpenAI Integration)

```powershell
# Create test console app
dotnet new console -n Nekote.Lab.Console
cd Nekote.Lab.Console
dotnet add reference ../Nekote.Core/Nekote.Core.csproj

# Add required packages
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Microsoft.Extensions.Configuration.Json

# Run test
dotnet run
```

---

## Success Criteria

### Phase 1 Complete When:
- [ ] All domain models compile
- [ ] All domain interfaces compile
- [ ] All configuration classes compile
- [ ] No compiler errors
- [ ] `dotnet build` succeeds

### Phase 2 Complete When:
- [ ] Console app successfully calls OpenAI API
- [ ] Response is printed to console
- [ ] Error handling works (test with invalid API key)
- [ ] Logs show request/response JSON
- [ ] No crashes

---

## Decision: What to Implement First?

**Recommendation:** Start with **Phase 1 (Foundation)** because:

1. **No blockers** - requires nothing except C# compiler
2. **Fast feedback** - see results in minutes
3. **Low risk** - can't break anything
4. **Builds momentum** - quick wins
5. **Foundation for Phase 2** - required for OpenAI implementation

**Next Question:** "Should we start coding Phase 1 now?"

---

## Questions Before Implementation

1. **Do you have an OpenAI API key?** (needed for Phase 2 testing)
2. **Should we use `Nekote.Lab.Console` for testing?** (already exists in your repo)
3. **Any changes to the Phase 1 design?** (models, interfaces, config)

---

*Ready to start implementing? Begin with Step 1: Domain Models (01-Domain-Models.md)*
