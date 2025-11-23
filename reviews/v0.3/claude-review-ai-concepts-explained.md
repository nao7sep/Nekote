# AI/LLM Concepts Guide for Nekote DTOs

**Date:** 2025-11-23
**Purpose:** Comprehensive explanation of all AI/LLM-specific concepts used in OpenAI and Gemini DTOs

---

## Table of Contents

1. [Introduction](#introduction)
2. [Sampling Parameters](#sampling-parameters)
3. [Tokens & Tokenization](#tokens--tokenization)
4. [Log Probabilities (Logprobs)](#log-probabilities-logprobs)
5. [Function Calling & Tool Use](#function-calling--tool-use)
6. [Response Format & Output Control](#response-format--output-control)
7. [Embeddings](#embeddings)
8. [Streaming](#streaming)
9. [Grounding & Citations](#grounding--citations)
10. [Safety & Moderation](#safety--moderation)
11. [Multi-Modal AI](#multi-modal-ai)
12. [Advanced Features](#advanced-features)

---

## Introduction

This document explains ALL AI/LLM-specific concepts found in the Nekote repository's OpenAI and Gemini DTOs. These concepts are used when interacting with Large Language Models (LLMs) via their APIs.

**Target Audience:** Developers who want to understand what each DTO property means and how it affects AI model behavior.

**Note:** This covers both **OpenAI** (GPT models) and **Google Gemini** APIs.

---

## 1. Sampling Parameters

Sampling parameters control **how the AI model selects the next token** when generating text. They affect randomness, creativity, and consistency.

### 1.1 Temperature

**What it is:** Controls the randomness of the model's output.

**Range:**
- OpenAI: 0.0 to 2.0
- Gemini: 0.0 to 2.0 (typical)

**How it works:**
- **Low temperature (0.0 - 0.3):** Deterministic, focused, predictable
  - Model picks the most likely tokens
  - Best for factual answers, code generation, data extraction
  - Example: "The capital of France is Paris" (always consistent)

- **Medium temperature (0.5 - 0.8):** Balanced creativity
  - Good mix of coherence and variety
  - Best for general conversation, content writing
  - Example: "Paris is known for its..." (varied but sensible)

- **High temperature (1.0 - 2.0):** Creative, random, unpredictable
  - Model explores less likely tokens
  - Best for creative writing, brainstorming
  - Example: "Paris reminds me of dancing umbrellas..." (creative but risky)

**DTO Properties:**
```csharp
// OpenAI
OpenAiChatRequestDto.Temperature (double?)

// Gemini
GeminiChatGenerationConfigDto.Temperature (double?)
```

**Example:**
```csharp
var request = new OpenAiChatRequestDto
{
    Temperature = 0.0  // Deterministic responses
};
```

**When to use:**
- **0.0:** Math problems, code generation, factual Q&A
- **0.7:** General chat, content creation
- **1.5+:** Creative stories, poetry, unusual ideas

---

### 1.2 Top-P (Nucleus Sampling)

**What it is:** Alternative to temperature for controlling randomness. Limits token selection to a cumulative probability threshold.

**Range:** 0.0 to 1.0

**How it works:**
1. Model ranks all possible next tokens by probability
2. Keeps only the tokens whose cumulative probability ≤ top_p
3. Samples from this reduced set

**Example:**
```
All tokens:          Probability:
"the"                40%
"a"                  30%
"an"                 15%
"that"               10%
"this"               5%

top_p = 0.8:
  Keep "the" (40%) + "a" (30%) + "an" (15%) = 85%
  Only sample from these 3 tokens
```

**Top-P vs Temperature:**
- **Temperature:** Flattens/sharpens entire probability distribution
- **Top-P:** Cuts off low-probability tokens

**DTO Properties:**
```csharp
// OpenAI
OpenAiChatRequestDto.TopP (double?)

// Gemini
GeminiChatGenerationConfigDto.TopP (double?)
```

**Best Practice:**
- **Use Temperature OR Top-P**, not both
- Top-P = 1.0 means "consider all tokens" (no filtering)
- Top-P = 0.1 means "only top 10% most likely tokens"

---

### 1.3 Top-K (Gemini Only)

**What it is:** Limits token selection to the top K most likely tokens.

**Range:** Integer (e.g., 10, 40, 100)

**How it works:**
1. Model ranks all possible next tokens by probability
2. Keeps only the top K tokens
3. Re-normalizes probabilities and samples

**Example:**
```
Top-K = 3:
  All tokens: ["the", "a", "an", "that", "this", ...]
  Keep only: ["the", "a", "an"]
  Sample from these 3
```

**Difference from Top-P:**
- **Top-K:** Fixed number of tokens (e.g., always 40 tokens)
- **Top-P:** Variable number (depends on probability distribution)

**DTO Properties:**
```csharp
// Gemini only
GeminiChatGenerationConfigDto.TopK (int?)
```

**When to use:**
- Lower Top-K (1-10): Very focused, deterministic
- Higher Top-K (40-100): More variety

---

### 1.4 Frequency Penalty

**What it is:** Reduces the likelihood of repeating tokens that have already appeared.

**Range:** -2.0 to 2.0

**How it works:**
- Positive values: Penalize repeated tokens → **less repetition**
- Negative values: Encourage repeated tokens → **more repetition**
- The penalty is proportional to how many times a token has appeared

**Example:**
```
Input: "The cat sat on the mat. The cat..."
Frequency Penalty = 1.0:
  Model is penalized for using "cat" again
  → Likely output: "The cat sat on the mat. The feline..."

Frequency Penalty = 0.0:
  No penalty
  → Might output: "The cat sat on the mat. The cat sat..."
```

**DTO Properties:**
```csharp
// OpenAI
OpenAiChatRequestDto.FrequencyPenalty (double?)

// Gemini
GeminiChatGenerationConfigDto.FrequencyPenalty (double?)
```

**Use cases:**
- **Positive (0.5 - 1.0):** Prevent repetitive text, encourage variety
- **Zero:** Default behavior
- **Negative:** Rare, used for specific repetition needs

---

### 1.5 Presence Penalty

**What it is:** Encourages the model to talk about **new topics** by penalizing tokens that have appeared at all.

**Range:** -2.0 to 2.0

**Difference from Frequency Penalty:**
- **Frequency Penalty:** Penalizes based on **how many times** a token appeared
- **Presence Penalty:** Penalizes based on **whether** a token appeared (binary)

**Example:**
```
Input: "Let's discuss Python. Python is..."
Presence Penalty = 1.0:
  "Python" appeared once → penalized equally regardless of count
  → Model encouraged to discuss OTHER topics
  → Might output: "...also consider JavaScript and Rust"

Frequency Penalty = 1.0:
  "Python" appeared twice → penalty increases with count
  → Model discouraged from repeating "Python" specifically
```

**DTO Properties:**
```csharp
// OpenAI
OpenAiChatRequestDto.PresencePenalty (double?)

// Gemini
GeminiChatGenerationConfigDto.PresencePenalty (double?)
```

**Use cases:**
- **Positive (0.5 - 1.0):** Encourage diverse topics in long conversations
- **Zero:** Default behavior

---

### 1.6 Seed (Deterministic Sampling)

**What it is:** A random seed for reproducible outputs.

**Type:** Integer

**How it works:**
- Same seed + same input + same parameters = same output (usually)
- Different seeds = different outputs
- Enables **deterministic testing** and debugging

**Important Notes:**
- **Not 100% guaranteed:** Minor API changes can affect reproducibility
- **OpenAI:** Deprecated (but still works)
- **Gemini:** Supported

**DTO Properties:**
```csharp
// OpenAI (deprecated)
OpenAiChatRequestDto.Seed (int?)

// Gemini
GeminiChatGenerationConfigDto.Seed (int?)
```

**Example:**
```csharp
// Test 1: seed = 12345
var response1 = await CallApi(seed: 12345);
// Output: "Paris is the capital of France."

// Test 2: seed = 12345 (same seed)
var response2 = await CallApi(seed: 12345);
// Output: "Paris is the capital of France." (same!)

// Test 3: seed = 67890 (different seed)
var response3 = await CallApi(seed: 67890);
// Output: "The capital of France is Paris." (different wording)
```

**Use cases:**
- Unit testing AI applications
- Debugging unexpected outputs
- Reproducible experiments

---

### Sampling Parameters Summary Table

| Parameter | Range | Effect | Best For |
|-----------|-------|--------|----------|
| **Temperature** | 0.0 - 2.0 | Controls randomness | Adjusting creativity level |
| **Top-P** | 0.0 - 1.0 | Filters low-probability tokens | Alternative to temperature |
| **Top-K** | Integer | Limits to top K tokens | Gemini-specific control |
| **Frequency Penalty** | -2.0 to 2.0 | Reduces token repetition | Avoiding repetitive text |
| **Presence Penalty** | -2.0 to 2.0 | Encourages new topics | Encouraging topic diversity |
| **Seed** | Integer | Enables reproducibility | Testing and debugging |

**Golden Rules:**
1. **Use Temperature OR Top-P**, not both
2. Start with Temperature = 0.7 for general use
3. Use Temperature = 0.0 for deterministic tasks
4. Frequency/Presence penalties usually between 0.0 and 1.0
5. Seed is optional; use for testing only

---

## 2. Tokens & Tokenization

**Tokens** are the fundamental units that LLMs process. Understanding tokens is crucial for cost management, context limits, and performance optimization.

### 2.1 What is a Token?

**Definition:** A token is a chunk of text that the model processes as a single unit.

**Key Points:**
- Tokens ≠ Words
- Tokens ≠ Characters
- 1 token ≈ 4 characters (English average)
- 1 token ≈ ¾ of a word (English average)

**Tokenization Examples:**

```
Text: "Hello, world!"
Tokens: ["Hello", ",", " world", "!"]  // 4 tokens

Text: "ChatGPT"
Tokens: ["Chat", "G", "PT"]  // 3 tokens

Text: "artificial intelligence"
Tokens: ["art", "ificial", " intelligence"]  // 3 tokens

Text: "人工知能" (Japanese)
Tokens: ["人", "工", "知", "能"]  // 4 tokens (1 char per token)

Text: "🎉🎊"
Tokens: ["🎉", "🎊"]  // 2 tokens (emoji = 1 token each)
```

**Why Tokens Matter:**
1. **API Costs:** Charged per token (input + output)
2. **Context Limits:** Models have max token limits (e.g., 128K tokens)
3. **Performance:** More tokens = longer processing time

---

### 2.2 Token Limits

**What it is:** Maximum number of tokens the model can process in a single request.

**Types:**

#### A) Max Completion Tokens (Output)
Maximum tokens the model can generate in its response.

**DTO Properties:**
```csharp
// OpenAI (current)
OpenAiChatRequestDto.MaxCompletionTokens (int?)

// OpenAI (deprecated)
OpenAiChatRequestDto.MaxTokens (int?)

// Gemini
GeminiChatGenerationConfigDto.MaxOutputTokens (int?)
```

**Example:**
```csharp
var request = new OpenAiChatRequestDto
{
    MaxCompletionTokens = 500  // Limit response to ~375 words
};
```

**Use Cases:**
- **Short answers:** MaxCompletionTokens = 100
- **Medium responses:** MaxCompletionTokens = 500
- **Long articles:** MaxCompletionTokens = 2000

#### B) Context Window (Total)
Maximum combined tokens for input + output.

**Common Limits:**
- GPT-3.5-Turbo: 16K tokens
- GPT-4: 8K tokens (base), 32K/128K (extended)
- GPT-4o: 128K tokens
- Gemini 1.5 Pro: 2M tokens
- Gemini 1.5 Flash: 1M tokens

**Formula:**
```
Available Output Tokens = Context Window - Input Tokens
```

**Example:**
```
Model: GPT-4 (8K context)
Input prompt: 6,000 tokens
Available for output: 8,000 - 6,000 = 2,000 tokens
```

---

### 2.3 Token Counts (Usage Tracking)

**What it is:** The API returns token counts after each request for billing and monitoring.

**OpenAI Token Counts:**

```csharp
public class OpenAiChatUsageDto
{
    // Input tokens
    public int PromptTokens { get; set; }

    // Output tokens
    public int CompletionTokens { get; set; }

    // Total tokens (prompt + completion)
    public int TotalTokens { get; set; }

    // Detailed breakdowns
    public OpenAiChatCompletionTokensDetailsDto? CompletionTokensDetails { get; set; }
    public OpenAiChatPromptTokensDetailsDto? PromptTokensDetails { get; set; }
}
```

**Gemini Token Counts:**

```csharp
public class GeminiChatUsageMetadataDto
{
    // Input tokens
    public int? PromptTokenCount { get; set; }

    // Cached tokens (reused from previous request)
    public int? CachedContentTokenCount { get; set; }

    // Output tokens
    public int? CandidatesTokenCount { get; set; }

    // Tokens used for tool/function prompts
    public int? ToolUsePromptTokenCount { get; set; }

    // Tokens used for model's internal "thinking"
    public int? ThoughtsTokenCount { get; set; }

    // Total tokens
    public int? TotalTokenCount { get; set; }

    // Per-modality breakdowns
    public List<GeminiChatModalityTokenCountDto>? PromptTokensDetails { get; set; }
    public List<GeminiChatModalityTokenCountDto>? CacheTokensDetails { get; set; }
    public List<GeminiChatModalityTokenCountDto>? CandidatesTokensDetails { get; set; }
}
```

**Example Usage:**
```csharp
var response = await CallOpenAiApi(request);

Console.WriteLine($"Input tokens: {response.Usage.PromptTokens}");
Console.WriteLine($"Output tokens: {response.Usage.CompletionTokens}");
Console.WriteLine($"Total tokens: {response.Usage.TotalTokens}");

// Calculate cost (GPT-4 example: $0.03 per 1K input, $0.06 per 1K output)
var inputCost = (response.Usage.PromptTokens / 1000.0) * 0.03;
var outputCost = (response.Usage.CompletionTokens / 1000.0) * 0.06;
var totalCost = inputCost + outputCost;

Console.WriteLine($"Estimated cost: ${totalCost:F4}");
```

---

### 2.4 Token Details (Advanced Breakdown)

#### OpenAI Completion Token Details

```csharp
public class OpenAiChatCompletionTokensDetailsDto
{
    // Tokens from accepted predictions (speculative execution)
    public int? AcceptedPredictionTokens { get; set; }

    // Tokens from rejected predictions
    public int? RejectedPredictionTokens { get; set; }

    // Tokens used for internal reasoning (o1 models)
    public int? ReasoningTokens { get; set; }

    // Tokens used for audio output
    public int? AudioTokens { get; set; }
}
```

**Reasoning Tokens (o1 Models):**
- o1-preview and o1-mini models use "chain of thought" reasoning
- These hidden reasoning tokens are NOT visible in the output
- Charged separately

**Example:**
```
User: "Solve: 2x + 5 = 13"

Hidden reasoning tokens (not shown to user):
  "Let's solve this step by step.
   First, subtract 5 from both sides: 2x = 8
   Then divide by 2: x = 4"
   [50 reasoning tokens]

Visible output:
  "x = 4"
   [5 completion tokens]

Total charged: 50 + 5 = 55 tokens
```

#### OpenAI Prompt Token Details

```csharp
public class OpenAiChatPromptTokensDetailsDto
{
    // Tokens retrieved from cache (not charged)
    public int? CachedTokens { get; set; }

    // Tokens from audio input
    public int? AudioTokens { get; set; }
}
```

---

### 2.5 Logit Bias (Token Probability Manipulation)

**What it is:** Manually adjusts the probability of specific tokens being generated.

**Range:** -100 to 100

**How it works:**
1. Identify token IDs you want to bias
2. Positive bias: Increase likelihood
3. Negative bias: Decrease likelihood
4. -100 = ban the token completely

**DTO Property:**
```csharp
// OpenAI
OpenAiChatRequestDto.LogitBias (Dictionary<string, int>?)
// Key: token ID (as string)
// Value: bias (-100 to 100)
```

**Example: Banning Profanity**
```csharp
var request = new OpenAiChatRequestDto
{
    LogitBias = new Dictionary<string, int>
    {
        ["1212"] = -100,  // Ban token 1212 (example: profanity)
        ["3456"] = -100   // Ban token 3456
    }
};
```

**Example: Encouraging Specific Words**
```csharp
var request = new OpenAiChatRequestDto
{
    LogitBias = new Dictionary<string, int>
    {
        ["4321"] = 50,  // Strongly encourage token 4321 (e.g., "Python")
        ["8765"] = 30   // Moderately encourage token 8765 (e.g., "code")
    }
};
```

**Use Cases:**
- Ban specific words/tokens
- Encourage technical terminology
- Bias towards certain formats (e.g., JSON)
- Content filtering

**Important Notes:**
- Requires knowing token IDs (use OpenAI's tokenizer tool)
- Very powerful but difficult to use correctly
- Overuse can make output unnatural

---

### 2.6 Encoding Formats (Embeddings)

**What it is:** How embedding vectors are encoded in API responses.

**DTO Property:**
```csharp
// OpenAI
OpenAiEmbeddingRequestDto.EncodingFormat (string?)
// Values: "float" or "base64"
```

**Options:**

#### A) Float (Default)
Embedding returned as array of floating-point numbers.

```json
{
  "embedding": [0.0012, -0.0034, 0.0056, ...]
}
```

**Pros:** Easy to use, human-readable
**Cons:** Larger JSON payload

#### B) Base64
Embedding returned as Base64-encoded binary data.

```json
{
  "embedding": "AAECAwQFBgcICQ..."
}
```

**Pros:** Smaller payload (~50% reduction)
**Cons:** Requires decoding

**Example:**
```csharp
var request = new OpenAiEmbeddingRequestDto
{
    Input = "Hello, world!",
    EncodingFormat = "base64"  // Smaller response
};

var response = await CallEmbeddingApi(request);
var base64 = response.Data[0].Embedding; // Base64 string
var floats = DecodeBase64ToFloats(base64); // Decode to float[]
```

---

### Token Best Practices

1. **Monitor Token Usage:**
   ```csharp
   if (response.Usage.TotalTokens > 100000)
   {
       Console.WriteLine("Warning: High token usage!");
   }
   ```

2. **Set Max Tokens:**
   Always set `MaxCompletionTokens` to prevent runaway costs.

3. **Estimate Tokens Before Calling API:**
   ```csharp
   // Rough estimate: 4 characters ≈ 1 token
   int estimatedTokens = text.Length / 4;
   ```

4. **Use Caching (Gemini):**
   Cached tokens are cheaper or free.

5. **Trim Long Inputs:**
   ```csharp
   if (estimatedTokens > maxContextWindow - maxOutput)
   {
       // Truncate or summarize input
   }
   ```

---

### Token Cost Examples (2024 Pricing)

| Model | Input (per 1K tokens) | Output (per 1K tokens) |
|-------|----------------------|------------------------|
| GPT-3.5-Turbo | $0.0005 | $0.0015 |
| GPT-4 | $0.03 | $0.06 |
| GPT-4o | $0.0025 | $0.01 |
| GPT-4o-mini | $0.00015 | $0.0006 |
| Gemini 1.5 Flash | Free tier, then $0.000075 | $0.0003 |
| Gemini 1.5 Pro | $0.00125 | $0.005 |

**Example Calculation:**
```
Request:
- Input: 1,500 tokens
- Output: 500 tokens
- Model: GPT-4

Cost:
- Input: (1,500 / 1,000) × $0.03 = $0.045
- Output: (500 / 1,000) × $0.06 = $0.030
- Total: $0.075 per request
```

---

## 3. Log Probabilities (Logprobs)

**Logprobs** (logarithmic probabilities) provide insight into **how confident** the model is about each token it generates. This is one of the most powerful but least understood features.

### 3.1 What are Logprobs?

**Definition:** The natural logarithm of the probability that the model assigned to each generated token.

**Mathematical Background:**
```
Probability: P(token) = 0.8 (80% confident)
Logprob: log(P(token)) = log(0.8) = -0.223

Probability: P(token) = 0.1 (10% confident)
Logprob: log(P(token)) = log(0.1) = -2.303
```

**Key Properties:**
- **Range:** -∞ to 0 (always negative or zero)
- **Higher is better:** -0.1 is more confident than -2.0
- **0 = 100% confident:** log(1.0) = 0
- **Very negative = uncertain:** log(0.001) = -6.91

**Why Log Scale?**
1. Probabilities multiply (0.8 × 0.7 × 0.9 = tiny number)
2. Logprobs add (log(0.8) + log(0.7) + log(0.9) = manageable)
3. Avoids floating-point underflow
4. Easier to work with computationally

---

### 3.2 Enabling Logprobs

**DTO Properties:**
```csharp
// OpenAI
OpenAiChatRequestDto.Logprobs = true;  // Enable logprobs
OpenAiChatRequestDto.TopLogprobs = 5;  // Return top 5 alternatives per token

// Gemini
GeminiChatGenerationConfigDto.ResponseLogprobs = true;  // Enable
GeminiChatGenerationConfigDto.Logprobs = 5;  // Top 5 alternatives
```

**Example Request:**
```csharp
var request = new OpenAiChatRequestDto
{
    Model = "gpt-4",
    Messages = new List<OpenAiChatMessageBaseDto>
    {
        new OpenAiChatMessageUserDto
        {
            Content = new OpenAiChatMessageContentStringDto
            {
                StringContent = "What is the capital of France?"
            }
        }
    },
    Logprobs = true,      // Enable logprobs
    TopLogprobs = 3       // Show top 3 alternatives per token
};
```

---

### 3.3 Logprobs Response Structure

#### OpenAI Logprobs

```csharp
public class OpenAiChatLogprobsDto
{
    // Logprobs for each content token
    public List<OpenAiChatLogprobContentDto>? Content { get; set; }

    // Logprobs for refusal tokens (if model refused)
    public List<OpenAiChatLogprobContentDto>? Refusal { get; set; }
}

public class OpenAiChatLogprobContentDto
{
    // The token that was actually generated
    public string? Token { get; set; }

    // The logprob of this token
    public double Logprob { get; set; }

    // Byte representation of token
    public List<int>? Bytes { get; set; }

    // Top alternative tokens (if requested)
    public List<OpenAiChatTopLogprobDto>? TopLogprobs { get; set; }
}

public class OpenAiChatTopLogprobDto
{
    // Alternative token
    public string? Token { get; set; }

    // Its logprob
    public double Logprob { get; set; }

    // Its byte representation
    public List<int>? Bytes { get; set; }
}
```

#### Gemini Logprobs

```csharp
public class GeminiChatLogprobsResultDto
{
    // Top candidates for each generation step
    public List<List<GeminiChatLogprobsCandidateDto>>? TopCandidates { get; set; }

    // The actual chosen candidates
    public List<GeminiChatLogprobsCandidateDto>? ChosenCandidates { get; set; }

    // Sum of all token logprobs (overall confidence)
    public double? LogProbabilitySum { get; set; }
}

public class GeminiChatLogprobsCandidateDto
{
    // The token
    public string? Token { get; set; }

    // Token ID
    public int? TokenId { get; set; }

    // Logprob value
    public double? LogProbability { get; set; }
}
```

---

### 3.4 Example: Reading Logprobs

**Question:** "What is the capital of France?"

**Response with Logprobs:**

```json
{
  "choices": [
    {
      "message": {
        "content": "The capital of France is Paris."
      },
      "logprobs": {
        "content": [
          {
            "token": "The",
            "logprob": -0.0001,  // Very confident
            "top_logprobs": [
              {"token": "The", "logprob": -0.0001},
              {"token": "France", "logprob": -8.5},
              {"token": "It", "logprob": -9.2}
            ]
          },
          {
            "token": " capital",
            "logprob": -0.0002,  // Very confident
            "top_logprobs": [
              {"token": " capital", "logprob": -0.0002},
              {"token": " answer", "logprob": -7.8},
              {"token": " city", "logprob": -8.1}
            ]
          },
          {
            "token": " of",
            "logprob": -0.00001,  // Extremely confident
            "top_logprobs": [...]
          },
          {
            "token": " France",
            "logprob": -0.00005,  // Extremely confident
            "top_logprobs": [...]
          },
          {
            "token": " is",
            "logprob": -0.0001,  // Very confident
            "top_logprobs": [...]
          },
          {
            "token": " Paris",
            "logprob": -0.0000001,  // Virtually certain
            "top_logprobs": [
              {"token": " Paris", "logprob": -0.0000001},
              {"token": " Lyon", "logprob": -15.2},
              {"token": " Marseille", "logprob": -16.8}
            ]
          },
          {
            "token": ".",
            "logprob": -0.001,  // Very confident
            "top_logprobs": [...]
          }
        ]
      }
    }
  ]
}
```

**Interpretation:**
- **"Paris":** logprob = -0.0000001 ≈ 99.99999% confident
- **"Lyon":** logprob = -15.2 ≈ 0.0002% confident
- Model is **extremely certain** that Paris is correct

---

### 3.5 Use Cases for Logprobs

#### A) Confidence Scoring

Detect when the model is **uncertain**:

```csharp
bool IsConfident(OpenAiChatLogprobContentDto logprob)
{
    // Threshold: -1.0
    // Values closer to 0 = more confident
    return logprob.Logprob > -1.0;
}

// Usage
var response = await CallApi(request);
foreach (var logprob in response.Choices[0].Logprobs.Content)
{
    if (!IsConfident(logprob))
    {
        Console.WriteLine($"Low confidence token: {logprob.Token} ({logprob.Logprob})");
        // Maybe ask user for clarification
    }
}
```

**Example:**
```
User: "What is the capital of Elbonia?" (fictional country)
Response: "The capital of Elbonia is Borovia."

Logprobs:
  "Borovia": -8.5 (LOW CONFIDENCE)

Action: Flag as uncertain, show disclaimer
```

#### B) Hallucination Detection

Detect when model is **making things up**:

```csharp
double CalculateAverageConfidence(List<OpenAiChatLogprobContentDto> logprobs)
{
    return logprobs.Average(lp => lp.Logprob);
}

var avgLogprob = CalculateAverageConfidence(response.Choices[0].Logprobs.Content);

if (avgLogprob < -2.0)
{
    Console.WriteLine("WARNING: Model seems uncertain. Possible hallucination.");
}
```

#### C) Multiple Choice Questions

Evaluate probabilities of different answers:

```csharp
// Question: "Is Paris in France? (Yes/No)"
// Check logprobs for "Yes" vs "No" tokens

var request = new OpenAiChatRequestDto
{
    Messages = [...],
    Logprobs = true,
    TopLogprobs = 10,  // Get more alternatives
    MaxCompletionTokens = 1  // Only generate 1 token
};

var response = await CallApi(request);
var firstToken = response.Choices[0].Logprobs.Content[0];

// Find probabilities
var yesLogprob = firstToken.TopLogprobs.FirstOrDefault(t => t.Token == "Yes")?.Logprob ?? double.NegativeInfinity;
var noLogprob = firstToken.TopLogprobs.FirstOrDefault(t => t.Token == "No")?.Logprob ?? double.NegativeInfinity;

Console.WriteLine($"P(Yes) ≈ {Math.Exp(yesLogprob):P2}");
Console.WriteLine($"P(No) ≈ {Math.Exp(noLogprob):P2}");
```

#### D) Fact Verification

Check if model "knows" a fact vs. guessing:

```csharp
bool IsFactuallyGrounded(double logprob)
{
    // Very high confidence (-0.01 or better) suggests factual knowledge
    // Low confidence suggests guessing
    return logprob > -0.1;
}
```

#### E) Quality Assessment

Calculate overall response quality:

```csharp
public class ResponseQuality
{
    public double AverageConfidence { get; set; }
    public int LowConfidenceTokens { get; set; }
    public double MinConfidence { get; set; }
    public string QualityRating { get; set; }
}

ResponseQuality AssessQuality(List<OpenAiChatLogprobContentDto> logprobs)
{
    var avg = logprobs.Average(lp => lp.Logprob);
    var min = logprobs.Min(lp => lp.Logprob);
    var lowCount = logprobs.Count(lp => lp.Logprob < -1.0);

    string rating;
    if (avg > -0.5 && min > -2.0)
        rating = "High Quality (Confident)";
    else if (avg > -2.0)
        rating = "Medium Quality";
    else
        rating = "Low Quality (Uncertain)";

    return new ResponseQuality
    {
        AverageConfidence = avg,
        LowConfidenceTokens = lowCount,
        MinConfidence = min,
        QualityRating = rating
    };
}
```

---

### 3.6 Converting Logprobs to Probabilities

**Formula:**
```
Probability = e^(logprob)
```

**C# Implementation:**
```csharp
double LogprobToProbability(double logprob)
{
    return Math.Exp(logprob);
}

// Examples:
LogprobToProbability(-0.001) ≈ 0.999 (99.9%)
LogprobToProbability(-1.0) ≈ 0.368 (36.8%)
LogprobToProbability(-5.0) ≈ 0.007 (0.7%)
```

**Probability to Percentage:**
```csharp
string FormatConfidence(double logprob)
{
    var probability = Math.Exp(logprob);
    return $"{probability:P2}";  // e.g., "99.50%"
}
```

---

### 3.7 Logprobs Best Practices

1. **Don't Over-Rely on Absolute Values:**
   - Logprobs are relative, not absolute guarantees
   - -0.1 doesn't mean "definitely correct"

2. **Use Thresholds Carefully:**
   ```csharp
   // Good thresholds (empirical):
   if (logprob > -0.5)  // Very confident
   if (logprob > -1.0)  // Confident
   if (logprob > -2.0)  // Somewhat confident
   if (logprob < -5.0)  // Very uncertain
   ```

3. **Compare Alternatives:**
   ```csharp
   var topToken = logprob.TopLogprobs[0];
   var secondToken = logprob.TopLogprobs[1];
   var gap = topToken.Logprob - secondToken.Logprob;

   if (gap > 5.0)  // Large gap = model is decisive
       Console.WriteLine("Model is very sure");
   ```

4. **Aggregate for Overall Confidence:**
   ```csharp
   // Don't judge by single token; look at average
   var avgLogprob = logprobs.Average(lp => lp.Logprob);
   ```

5. **Performance Cost:**
   - Logprobs add overhead to API response
   - Only enable when needed
   - `TopLogprobs=20` is much slower than `TopLogprobs=3`

---

### 3.8 Gemini-Specific: Log Probability Sum

Gemini provides a **cumulative logprob sum** for the entire response:

```csharp
// Gemini
var logprobSum = response.Candidates[0].LogprobsResult.LogProbabilitySum;

// More negative = less confident overall
if (logprobSum < -50.0)
{
    Console.WriteLine("Low overall confidence");
}
```

**Interpretation:**
```
Short response (10 tokens): Sum = -5.0 → Average = -0.5 per token (good)
Long response (100 tokens): Sum = -50.0 → Average = -0.5 per token (good)
Long response (100 tokens): Sum = -200.0 → Average = -2.0 per token (poor)
```

---

### Logprobs Summary

| Concept | Meaning | Range | Good Value |
|---------|---------|-------|------------|
| **Logprob** | Log of probability | -∞ to 0 | > -1.0 |
| **Token** | Generated text unit | - | - |
| **Top Logprobs** | Alternative tokens | Up to 20 | Check gap |
| **Probability** | e^(logprob) | 0.0 to 1.0 | > 0.5 |

**When to Use Logprobs:**
- ✅ Fact-checking AI outputs
- ✅ Detecting hallucinations
- ✅ Building confidence scores
- ✅ Multiple-choice questions
- ✅ Quality assessment
- ❌ Not needed for simple chat
- ❌ Adds API cost/latency

---

## 4. Function Calling & Tool Use

**Function calling** (also called "tool use") allows the AI model to **call external functions/APIs** to perform actions or retrieve information. This is one of the most powerful features for building AI agents.

### 4.1 What is Function Calling?

**Problem:** LLMs can't:
- Access real-time data (weather, stock prices, etc.)
- Perform calculations accurately
- Interact with databases or external APIs
- Take actions (send emails, create calendar events, etc.)

**Solution:** Function calling lets the model:
1. **Recognize** when it needs external help
2. **Generate** a function call with appropriate arguments
3. **Return control** to your application
4. **Receive** the function result
5. **Continue** the conversation with new information

**Flow:**
```
User: "What's the weather in Tokyo?"
   ↓
Model: "I need to call get_weather(city='Tokyo')"
   ↓
Your Code: Calls actual weather API
   ↓
Your Code: Returns {"temperature": 22, "condition": "sunny"}
   ↓
Model: "The weather in Tokyo is 22°C and sunny."
```

---

### 4.2 Defining Functions

#### OpenAI Function Definition

```csharp
public class OpenAiChatFunctionDto
{
    // Function name (alphanumeric + underscores)
    public string? Name { get; set; }

    // Human-readable description (helps model decide when to call)
    public string? Description { get; set; }

    // JSON Schema for parameters
    public JsonElement? Parameters { get; set; }
}
```

**Example:**
```csharp
var getWeatherFunction = new OpenAiChatFunctionDto
{
    Name = "get_weather",
    Description = "Get the current weather in a given location",
    Parameters = JsonSerializer.SerializeToElement(new
    {
        type = "object",
        properties = new
        {
            location = new
            {
                type = "string",
                description = "The city and state, e.g. San Francisco, CA"
            },
            unit = new
            {
                type = "string",
                @enum = new[] { "celsius", "fahrenheit" },
                description = "The temperature unit"
            }
        },
        required = new[] { "location" }
    })
};
```

#### Gemini Function Declaration

```csharp
public class GeminiChatFunctionDeclarationDto
{
    // Function name
    public string? Name { get; set; }

    // Description
    public string? Description { get; set; }

    // Behavior description (optional)
    public string? Behavior { get; set; }

    // Parameters (object properties)
    public object? Parameters { get; set; }

    // Or JSON Schema format
    public JsonElement? ParametersJsonSchema { get; set; }

    // Response schema (optional)
    public object? Response { get; set; }
    public JsonElement? ResponseJsonSchema { get; set; }
}
```

---

### 4.3 Function Call Modes

#### OpenAI Tool Choice

```csharp
// OpenAI
OpenAiChatRequestDto.ToolChoice
```

**Options:**

1. **"none"** - Model won't call any functions
   ```csharp
   ToolChoice = new OpenAiChatToolChoiceStringDto { StringChoice = "none" }
   ```

2. **"auto"** (default) - Model decides
   ```csharp
   ToolChoice = new OpenAiChatToolChoiceStringDto { StringChoice = "auto" }
   ```

3. **"required"** - Model MUST call at least one function
   ```csharp
   ToolChoice = new OpenAiChatToolChoiceStringDto { StringChoice = "required" }
   ```

4. **Specific function** - Force a particular function
   ```csharp
   ToolChoice = new OpenAiChatToolChoiceFunctionDto
   {
       Type = "function",
       Function = new OpenAiChatToolChoiceFunctionDetailDto
       {
           Name = "get_weather"
       }
   }
   ```

#### Gemini Function Calling Config

```csharp
public class GeminiChatFunctionCallingConfigDto
{
    // Mode: "AUTO", "ANY", "NONE"
    public string? Mode { get; set; }

    // Allowed function names (if Mode = "ANY")
    public List<string>? AllowedFunctionNames { get; set; }
}
```

**Modes:**
- **"AUTO":** Model decides (like OpenAI "auto")
- **"ANY":** Model must call one of the allowed functions
- **"NONE":** No function calls (like OpenAI "none")

---

### 4.4 Complete Function Calling Example

**Step 1: Define Functions**

```csharp
var tools = new List<OpenAiChatToolBaseDto>
{
    new OpenAiChatToolFunctionDto
    {
        Type = "function",
        Function = new OpenAiChatToolFunctionDefinitionDto
        {
            Name = "get_current_weather",
            Description = "Get the current weather in a given location",
            Parameters = JsonSerializer.SerializeToElement(new
            {
                type = "object",
                properties = new
                {
                    location = new
                    {
                        type = "string",
                        description = "City and state, e.g. San Francisco, CA"
                    },
                    unit = new
                    {
                        type = "string",
                        @enum = new[] { "celsius", "fahrenheit" }
                    }
                },
                required = new[] { "location" }
            })
        }
    },
    new OpenAiChatToolFunctionDto
    {
        Type = "function",
        Function = new OpenAiChatToolFunctionDefinitionDto
        {
            Name = "get_stock_price",
            Description = "Get the current stock price for a given ticker symbol",
            Parameters = JsonSerializer.SerializeToElement(new
            {
                type = "object",
                properties = new
                {
                    symbol = new
                    {
                        type = "string",
                        description = "Stock ticker symbol, e.g. AAPL for Apple"
                    }
                },
                required = new[] { "symbol" }
            })
        }
    }
};
```

**Step 2: Send Request**

```csharp
var request = new OpenAiChatRequestDto
{
    Model = "gpt-4",
    Messages = new List<OpenAiChatMessageBaseDto>
    {
        new OpenAiChatMessageUserDto
        {
            Role = "user",
            Content = new OpenAiChatMessageContentStringDto
            {
                StringContent = "What's the weather in Tokyo and the price of Apple stock?"
            }
        }
    },
    Tools = tools,
    ToolChoice = new OpenAiChatToolChoiceStringDto { StringChoice = "auto" }
};

var response = await CallOpenAiApi(request);
```

**Step 3: Handle Function Calls**

```csharp
var choice = response.Choices[0];

if (choice.FinishReason == "tool_calls")
{
    var toolCalls = choice.Message.ToolCalls;

    foreach (var toolCall in toolCalls)
    {
        var functionName = toolCall.Function.Name;
        var arguments = toolCall.Function.Arguments; // JSON string

        // Parse arguments
        var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);

        string functionResult;

        if (functionName == "get_current_weather")
        {
            var location = args["location"].GetString();
            var unit = args.ContainsKey("unit") ? args["unit"].GetString() : "celsius";

            // Call your actual weather API
            functionResult = GetWeather(location, unit);
            // Returns: "{\"temperature\": 22, \"condition\": \"sunny\"}"
        }
        else if (functionName == "get_stock_price")
        {
            var symbol = args["symbol"].GetString();

            // Call your actual stock API
            functionResult = GetStockPrice(symbol);
            // Returns: "{\"symbol\": \"AAPL\", \"price\": 175.50}"
        }
        else
        {
            functionResult = "{\"error\": \"Unknown function\"}";
        }

        // Add function result to conversation
        messages.Add(new OpenAiChatMessageToolDto
        {
            Role = "tool",
            Content = new OpenAiChatMessageContentStringDto
            {
                StringContent = functionResult
            },
            ToolCallId = toolCall.Id
        });
    }

    // Send updated conversation back to model
    var followUpRequest = new OpenAiChatRequestDto
    {
        Model = "gpt-4",
        Messages = messages,  // Includes function results
        Tools = tools
    };

    var finalResponse = await CallOpenAiApi(followUpRequest);

    // Now model can synthesize answer:
    // "The weather in Tokyo is 22°C and sunny. Apple stock is trading at $175.50."
}
```

---

### 4.5 Parallel Tool Calls

**What it is:** Model can call **multiple functions simultaneously** in one response.

**DTO Property:**
```csharp
// OpenAI
OpenAiChatRequestDto.ParallelToolCalls = true;  // Default: true
```

**Example:**
```
User: "What's the weather in Tokyo, Paris, and New York?"

Without parallel calls:
  → Call get_weather("Tokyo")
  → Wait for result
  → Call get_weather("Paris")
  → Wait for result
  → Call get_weather("New York")
  → Wait for result

With parallel calls:
  → Call get_weather("Tokyo"), get_weather("Paris"), get_weather("New York")
  → All at once!
```

**Response Structure:**
```csharp
response.Choices[0].Message.ToolCalls = new List<OpenAiChatToolCallDto>
{
    new() { Id = "call_1", Function = new() { Name = "get_weather", Arguments = "{\"location\": \"Tokyo\"}" } },
    new() { Id = "call_2", Function = new() { Name = "get_weather", Arguments = "{\"location\": \"Paris\"}" } },
    new() { Id = "call_3", Function = new() { Name = "get_weather", Arguments = "{\"location\": \"New York\"}" } }
};
```

**Benefits:**
- Faster execution (parallel processing)
- More efficient API usage

---

### 4.6 Strict Schema Compliance (OpenAI)

**What it is:** Ensures function calls **always match** your JSON schema exactly.

**DTO Property:**
```csharp
public class OpenAiChatJsonSchemaDto
{
    public string? Name { get; set; }
    public bool? Strict { get; set; }  // ← Strict mode
    public JsonElement? Schema { get; set; }
}
```

**Example:**
```csharp
var function = new OpenAiChatToolFunctionDefinitionDto
{
    Name = "get_delivery_date",
    Description = "Get the delivery date for a customer's order",
    Parameters = JsonSerializer.SerializeToElement(new
    {
        type = "object",
        properties = new
        {
            order_id = new { type = "string" }
        },
        required = new[] { "order_id" },
        additionalProperties = false  // Strict: no extra fields allowed
    }),
    Strict = true  // Enable strict mode
};
```

**Without Strict Mode:**
```json
// Model might generate:
{
  "order_id": "12345",
  "extra_field": "something"  ← Not in schema!
}
```

**With Strict Mode:**
```json
// Model MUST generate exactly:
{
  "order_id": "12345"
}
// No extra fields allowed
```

**Benefits:**
- Guaranteed schema compliance
- Easier parsing
- Fewer errors

---

### 4.7 Function Call vs Tool Call (Terminology)

**Historical Context:**
- **Old (deprecated):** `functions` and `function_call`
- **Current:** `tools` and `tool_choice`

**Why the change?**
- "Tools" is more general (functions, code execution, web search, etc.)
- "Functions" implied only custom user-defined functions

**DTO Migration:**
```csharp
// OLD (deprecated)
OpenAiChatRequestDto.Functions
OpenAiChatRequestDto.FunctionCall

// NEW (current)
OpenAiChatRequestDto.Tools
OpenAiChatRequestDto.ToolChoice
```

**Still Supported:** Old fields work but show `[Obsolete]` warnings.

---

### 4.8 Custom Tools (OpenAI Advanced)

**What it is:** Define custom tool behaviors beyond simple functions.

**Types:**

#### A) Custom Grammar Tool
Define a grammar for structured output.

```csharp
new OpenAiChatToolCustomDto
{
    Type = "custom",
    Custom = new OpenAiChatToolCustomGrammarDto
    {
        Format = new OpenAiChatToolCustomFormatGrammarDto
        {
            Grammar = "/* BNF grammar here */"
        }
    }
}
```

#### B) Custom Text Tool
Define tool using natural language.

```csharp
new OpenAiChatToolCustomDto
{
    Type = "custom",
    Custom = new OpenAiChatToolCustomFormatTextDto
    {
        FormatCase = OpenAiChatToolCustomFormatTextDto.FormatOneofCase.TextFormat,
        TextFormat = "Output must be valid XML"
    }
}
```

**Use Cases:**
- Enforce specific output formats
- Domain-specific languages
- Complex structured data

---

### 4.9 Gemini-Specific: Code Execution Tool

**What it is:** Gemini can **write and execute Python code** automatically.

**DTO:**
```csharp
public class GeminiChatCodeExecutionDto
{
    // No configuration needed - just enable it
}
```

**Usage:**
```csharp
var request = new GeminiChatRequestDto
{
    Tools = new List<GeminiChatToolDto>
    {
        new GeminiChatToolDto
        {
            CodeExecution = new GeminiChatCodeExecutionDto()
        }
    }
};
```

**What happens:**
```
User: "Calculate the 50th Fibonacci number"
   ↓
Gemini writes code:
   def fib(n):
       if n <= 1: return n
       return fib(n-1) + fib(n-2)
   print(fib(50))
   ↓
Gemini executes code
   ↓
Result: 12586269025
   ↓
Gemini: "The 50th Fibonacci number is 12,586,269,025"
```

**Returned in Response:**
```csharp
response.Candidates[0].Content.Parts = new List<GeminiChatPartDto>
{
    new()
    {
        ExecutableCode = new GeminiChatExecutableCodeDto
        {
            Language = "python",
            Code = "def fib(n): ..."
        }
    },
    new()
    {
        CodeExecutionResult = new GeminiChatCodeExecutionResultDto
        {
            Outcome = "success",
            Output = "12586269025"
        }
    }
};
```

---

### 4.10 Function Calling Best Practices

1. **Clear Descriptions:**
   ```csharp
   // ✅ Good
   Description = "Get current weather in a city. Returns temperature and conditions."

   // ❌ Bad
   Description = "Weather"
   ```

2. **Specific Parameter Descriptions:**
   ```csharp
   properties = new
   {
       location = new
       {
           type = "string",
           description = "City and state/country, e.g. 'Tokyo, Japan' or 'New York, NY'"
           // ✅ Examples help model format correctly
       }
   }
   ```

3. **Mark Required Parameters:**
   ```csharp
   required = new[] { "location", "date" }
   ```

4. **Validate Function Arguments:**
   ```csharp
   try
   {
       var args = JsonSerializer.Deserialize<WeatherArgs>(arguments);
       if (string.IsNullOrEmpty(args.Location))
           throw new ArgumentException("Location required");
   }
   catch (Exception ex)
   {
       return "{\"error\": \"Invalid arguments\"}";
   }
   ```

5. **Handle Errors Gracefully:**
   ```csharp
   string CallFunction(string name, string args)
   {
       try
       {
           // Call actual function
           return ExecuteFunction(name, args);
       }
       catch (Exception ex)
       {
           // Return error as JSON so model can handle it
           return JsonSerializer.Serialize(new { error = ex.Message });
       }
   }
   ```

6. **Return Structured Data:**
   ```csharp
   // ✅ Good - structured JSON
   return "{\"temperature\": 22, \"unit\": \"celsius\", \"condition\": \"sunny\"}";

   // ❌ Bad - plain text (harder for model to parse)
   return "It's 22 degrees and sunny";
   ```

---

### Function Calling Summary

| Concept | Purpose | Example |
|---------|---------|---------|
| **Tools** | List of available functions | Weather, stocks, calendar |
| **Tool Choice** | Control when functions called | "auto", "required", specific |
| **Tool Calls** | Model's function call requests | get_weather("Tokyo") |
| **Tool Response** | Your function's result | {"temp": 22, "condition": "sunny"} |
| **Parallel Calls** | Multiple functions at once | 3 weather calls simultaneously |
| **Strict Mode** | Enforce schema compliance | No extra fields allowed |

**When to Use Function Calling:**
- ✅ Real-time data needs (weather, stocks, news)
- ✅ External API integration (databases, search)
- ✅ Calculations (complex math, data analysis)
- ✅ Actions (send email, create event, update record)
- ✅ Building AI agents and assistants
- ❌ Simple text generation (unnecessary overhead)

---

## 5. Response Format & Output Control

**Response format** controls **how the model structures its output**. You can request plain text, JSON, or specific JSON schemas.

### 5.1 Response Format Types

#### OpenAI Response Format

```csharp
public class OpenAiChatResponseFormatBaseDto
{
    public string? Type { get; set; }  // "text", "json_object", "json_schema"
}
```

**Three Types:**

**A) Text (Default)**
```csharp
ResponseFormat = new OpenAiChatResponseFormatTextDto
{
    Type = "text"
}
```
- Normal conversational text
- No structure enforced

**B) JSON Object**
```csharp
ResponseFormat = new OpenAiChatResponseFormatJsonObjectDto
{
    Type = "json_object"
}
```
- Response MUST be valid JSON
- No schema enforcement (any structure)
- Must prompt "respond in JSON" in messages

**Example:**
```
User: "List 3 colors in JSON format"
Response: {"colors": ["red", "blue", "green"]}
```

**C) JSON Schema**
```csharp
ResponseFormat = new OpenAiChatResponseFormatJsonSchemaDto
{
    Type = "json_schema",
    JsonSchema = new OpenAiChatJsonSchemaDto
    {
        Name = "color_list",
        Strict = true,
        Schema = JsonSerializer.SerializeToElement(new
        {
            type = "object",
            properties = new
            {
                colors = new
                {
                    type = "array",
                    items = new { type = "string" }
                }
            },
            required = new[] { "colors" },
            additionalProperties = false
        })
    }
}
```
- Response MUST match exact schema
- Strictest control
- Best for parsing/automation

---

#### Gemini Response Format

```csharp
public class GeminiChatGenerationConfigDto
{
    // MIME type for response
    public string? ResponseMimeType { get; set; }  // "text/plain", "application/json"

    // JSON schema for structured output
    public JsonElement? ResponseSchema { get; set; }
}
```

**Examples:**

**Text Response:**
```csharp
GenerationConfig = new GeminiChatGenerationConfigDto
{
    ResponseMimeType = "text/plain"
}
```

**JSON Response:**
```csharp
GenerationConfig = new GeminiChatGenerationConfigDto
{
    ResponseMimeType = "application/json",
    ResponseSchema = JsonSerializer.SerializeToElement(new
    {
        type = "object",
        properties = new
        {
            colors = new
            {
                type = "array",
                items = new { type = "string" }
            }
        }
    })
}
```

---

### 5.2 Complete JSON Schema Example

**Scenario:** Extract structured product information.

**Schema:**
```csharp
var productSchema = new
{
    type = "object",
    properties = new
    {
        name = new
        {
            type = "string",
            description = "Product name"
        },
        price = new
        {
            type = "number",
            description = "Price in USD"
        },
        inStock = new
        {
            type = "boolean",
            description = "Whether product is in stock"
        },
        categories = new
        {
            type = "array",
            items = new { type = "string" },
            description = "Product categories"
        },
        specs = new
        {
            type = "object",
            properties = new
            {
                weight = new { type = "number" },
                dimensions = new
                {
                    type = "object",
                    properties = new
                    {
                        length = new { type = "number" },
                        width = new { type = "number" },
                        height = new { type = "number" }
                    },
                    required = new[] { "length", "width", "height" }
                }
            }
        }
    },
    required = new[] { "name", "price", "inStock" },
    additionalProperties = false
};
```

**Request:**
```csharp
var request = new OpenAiChatRequestDto
{
    Model = "gpt-4",
    Messages = new List<OpenAiChatMessageBaseDto>
    {
        new OpenAiChatMessageUserDto
        {
            Role = "user",
            Content = new OpenAiChatMessageContentStringDto
            {
                StringContent = @"
Extract product info:
Product: iPhone 15 Pro
Price: $999
Available: Yes
Categories: Electronics, Smartphones, Apple
Weight: 187g
Size: 146.6 x 70.6 x 8.25 mm"
            }
        }
    },
    ResponseFormat = new OpenAiChatResponseFormatJsonSchemaDto
    {
        Type = "json_schema",
        JsonSchema = new OpenAiChatJsonSchemaDto
        {
            Name = "product_info",
            Strict = true,
            Schema = JsonSerializer.SerializeToElement(productSchema)
        }
    }
};
```

**Response (guaranteed structure):**
```json
{
  "name": "iPhone 15 Pro",
  "price": 999,
  "inStock": true,
  "categories": ["Electronics", "Smartphones", "Apple"],
  "specs": {
    "weight": 187,
    "dimensions": {
      "length": 146.6,
      "width": 70.6,
      "height": 8.25
    }
  }
}
```

**Parsing (guaranteed success):**
```csharp
var responseText = response.Choices[0].Message.Content;
var product = JsonSerializer.Deserialize<Product>(responseText);

// No try-catch needed! Schema guarantees structure.
Console.WriteLine($"Product: {product.Name}");
Console.WriteLine($"Price: ${product.Price}");
Console.WriteLine($"In Stock: {product.InStock}");
```

---

### 5.3 Stop Sequences

**What it is:** Strings that **stop generation** when encountered.

**DTO Properties:**
```csharp
// OpenAI (up to 4 sequences)
OpenAiChatRequestDto.Stop = new List<string> { "END", "\n\n" };

// Gemini (up to 5 sequences)
GeminiChatGenerationConfigDto.StopSequences = new List<string> { "###", "STOP" };
```

**Use Case 1: Structured Output**
```csharp
var request = new OpenAiChatRequestDto
{
    Model = "gpt-4",
    Messages = new List<OpenAiChatMessageBaseDto>
    {
        new OpenAiChatMessageUserDto
        {
            Role = "user",
            Content = new OpenAiChatMessageContentStringDto
            {
                StringContent = "Write a haiku about spring. End with '###'"
            }
        }
    },
    Stop = new List<string> { "###" }
};
```

**Response:**
```
Cherry blossoms bloom
Soft petals drift on the breeze
Spring awakens life
```
(Stops at "###" - not included in output)

**Use Case 2: Prevent Overgeneration**
```csharp
Stop = new List<string> { "\n\n", "---" }
```
Stops after first paragraph.

---

### 5.4 Finish Reasons

**What it is:** Why the model **stopped generating**.

**DTO Property:**
```csharp
// OpenAI
response.Choices[0].FinishReason

// Gemini
response.Candidates[0].FinishReason
```

**Possible Values:**

| Reason | Meaning | Action |
|--------|---------|--------|
| **"stop"** | Natural completion | ✅ Output is complete |
| **"length"** | Hit max_tokens limit | ⚠️ Output truncated, may be incomplete |
| **"tool_calls"** | Called a function | 🔧 Execute function, continue conversation |
| **"content_filter"** | Safety filter triggered | 🚫 Content blocked, adjust prompt |
| **"end_turn"** | Multi-turn end | ✅ Turn complete (multi-agent) |
| **"stop_sequence"** | Hit stop sequence | ✅ Stopped at your marker |

**Example: Handling Truncation**
```csharp
var choice = response.Choices[0];

if (choice.FinishReason == "length")
{
    Console.WriteLine("⚠️ Response truncated! Consider:");
    Console.WriteLine("  - Increase MaxTokens");
    Console.WriteLine("  - Simplify prompt");
    Console.WriteLine("  - Request summary instead");

    // Option: Continue generation
    messages.Add(new OpenAiChatMessageAssistantDto
    {
        Role = "assistant",
        Content = new OpenAiChatMessageContentStringDto
        {
            StringContent = choice.Message.Content
        }
    });
    messages.Add(new OpenAiChatMessageUserDto
    {
        Role = "user",
        Content = new OpenAiChatMessageContentStringDto
        {
            StringContent = "Please continue."
        }
    });

    var continuationResponse = await CallOpenAiApi(new OpenAiChatRequestDto
    {
        Model = "gpt-4",
        Messages = messages
    });
}
else if (choice.FinishReason == "stop")
{
    Console.WriteLine("✅ Complete response received");
}
```

---

### 5.5 Candidate Count & Alternatives

**What it is:** Generate **multiple response candidates** and choose the best.

**DTO Property:**
```csharp
// OpenAI
OpenAiChatRequestDto.N = 3;  // Generate 3 alternatives

// Gemini
GeminiChatGenerationConfigDto.CandidateCount = 3;
```

**Example:**
```csharp
var request = new OpenAiChatRequestDto
{
    Model = "gpt-4",
    Messages = new List<OpenAiChatMessageBaseDto>
    {
        new OpenAiChatMessageUserDto
        {
            Role = "user",
            Content = new OpenAiChatMessageContentStringDto
            {
                StringContent = "Write a tagline for a coffee shop"
            }
        }
    },
    N = 3,  // Get 3 options
    Temperature = 0.9  // High creativity
};

var response = await CallOpenAiApi(request);

// Pick best candidate
foreach (var choice in response.Choices)
{
    Console.WriteLine($"Option {choice.Index + 1}: {choice.Message.Content}");
}
```

**Output:**
```
Option 1: "Where every cup tells a story"
Option 2: "Brew happiness, one cup at a time"
Option 3: "Your daily grind, perfected"
```

**Use Cases:**
- Creative writing (pick best option)
- A/B testing (compare approaches)
- Quality filtering (choose highest confidence)

**⚠️ Cost Warning:**
- `N=3` costs **3x** the tokens!
- Use sparingly

---

### 5.6 Max Completion Tokens

**What it is:** **Limit output length**.

**DTO Property:**
```csharp
// OpenAI
OpenAiChatRequestDto.MaxTokens = 100;
OpenAiChatRequestDto.MaxCompletionTokens = 100;  // Preferred (newer)

// Gemini
GeminiChatGenerationConfigDto.MaxOutputTokens = 100;
```

**Difference:**
- **MaxTokens:** Total (input + output) - deprecated
- **MaxCompletionTokens:** Output only (recommended)

**Example: Force Concise Response**
```csharp
var request = new OpenAiChatRequestDto
{
    Model = "gpt-4",
    Messages = new List<OpenAiChatMessageBaseDto>
    {
        new OpenAiChatMessageUserDto
        {
            Role = "user",
            Content = new OpenAiChatMessageContentStringDto
            {
                StringContent = "Explain quantum computing in simple terms"
            }
        }
    },
    MaxCompletionTokens = 50  // Force brief answer (~40 words)
};
```

**Response:**
```
Quantum computing uses quantum bits (qubits) that can be 0, 1, or both simultaneously (superposition).
This allows quantum computers to solve certain problems much faster than classical computers by exploring
multiple solutions at once.
```

(49 tokens - stays under limit)

**Use Cases:**
- Chat interfaces (prevent wall-of-text)
- Cost control (limit spending)
- Quick summaries
- Real-time responses (faster generation)

---

### 5.7 Response Metadata

#### OpenAI System Fingerprint

```csharp
response.SystemFingerprint  // e.g., "fp_44709d6fcb"
```

**What it is:** Identifier for the **exact model configuration** used.

**Use Case:**
```csharp
// Store with cached results
var cache = new Dictionary<string, (string Response, string Fingerprint)>();
cache[promptHash] = (response.Choices[0].Message.Content, response.SystemFingerprint);

// Later: Check if model changed
if (cache.TryGetValue(promptHash, out var cached))
{
    if (cached.Fingerprint == response.SystemFingerprint)
    {
        // Same model version - can trust cached results
        return cached.Response;
    }
    else
    {
        // Model updated - regenerate
        return await RegenerateResponse(prompt);
    }
}
```

#### Gemini Model Version

```csharp
response.ModelVersion  // e.g., "gemini-1.5-pro-001"
```

**What it is:** Exact model version used (includes patch number).

---

### 5.8 Verbosity Levels (Gemini)

**What it is:** Control **response detail level**.

**DTO Property:**
```csharp
GeminiChatGenerationConfigDto.VerbosityLevel
```

**Options:**
- **Low:** Brief, concise answers
- **Medium:** Balanced (default)
- **High:** Detailed, comprehensive explanations

**Example:**
```csharp
// Request concise response
var request = new GeminiChatRequestDto
{
    GenerationConfig = new GeminiChatGenerationConfigDto
    {
        VerbosityLevel = "low"
    },
    Contents = new List<GeminiChatContentDto>
    {
        new()
        {
            Role = "user",
            Parts = new List<GeminiChatPartDto>
            {
                new() { Text = "What is photosynthesis?" }
            }
        }
    }
};
```

**Low Verbosity:**
```
Photosynthesis is the process where plants convert sunlight, water, and CO2 into glucose and oxygen.
```

**High Verbosity:**
```
Photosynthesis is a complex biochemical process that occurs in plants, algae, and some bacteria.
It takes place primarily in the chloroplasts, where chlorophyll molecules absorb light energy.
The process consists of two main stages:

1. Light-dependent reactions: Light energy is captured and converted into chemical energy (ATP and NADPH)
2. Calvin cycle: CO2 is fixed into glucose using the energy from the light reactions

The overall equation is: 6CO2 + 6H2O + light → C6H12O6 + 6O2

This process is fundamental to life on Earth, producing oxygen and serving as the base of most food chains.
```

---

### Response Format Best Practices

1. **Use JSON Schema for Automation:**
   ```csharp
   // ✅ Good: Guaranteed parseable
   ResponseFormat = new OpenAiChatResponseFormatJsonSchemaDto
   {
       Type = "json_schema",
       JsonSchema = schema
   }

   // ❌ Bad: Fragile parsing
   Prompt = "Please respond in JSON format"
   ```

2. **Set Stop Sequences for Structured Output:**
   ```csharp
   Stop = new List<string> { "\n\n", "---", "END" }
   ```

3. **Check Finish Reason:**
   ```csharp
   if (choice.FinishReason == "length")
   {
       // Handle truncation
   }
   ```

4. **Limit Tokens for Cost Control:**
   ```csharp
   MaxCompletionTokens = 500  // Prevent runaway costs
   ```

5. **Use Multiple Candidates Sparingly:**
   ```csharp
   N = 3  // Only when you need options (3x cost!)
   ```

---

### Response Format Summary

| Concept | Purpose | Example |
|---------|---------|---------|
| **Response Format** | Control output structure | "text", "json_object", "json_schema" |
| **JSON Schema** | Enforce exact structure | Product extraction |
| **Stop Sequences** | Stop generation at marker | "###", "\n\n" |
| **Finish Reason** | Why generation stopped | "stop", "length", "tool_calls" |
| **Candidate Count** | Multiple options | N=3 for creative choices |
| **Max Tokens** | Limit output length | MaxCompletionTokens=100 |
| **Verbosity** | Control detail level | "low", "medium", "high" |

**When to Use Structured Output:**
- ✅ Parsing responses (JSON schema)
- ✅ Database insertion (strict structure)
- ✅ API integration (predictable format)
- ✅ Automation (no human review)
- ❌ Creative writing (too restrictive)
- ❌ Conversational chat (unnecessary)

---

## 6. Embeddings

**Embeddings** are **numerical representations** of text as vectors. They capture semantic meaning, enabling similarity comparisons and search.

### 6.1 What are Embeddings?

**Simple Analogy:**
- Words/sentences → Converted to **numbers**
- Similar meanings → **Close together** in vector space
- Different meanings → **Far apart**

**Example:**
```
Text: "The cat sat on the mat"
Embedding: [0.021, -0.153, 0.872, ..., 0.441]  ← 1536 numbers
           └────────── Vector ──────────┘

Text: "A feline rested on the rug"
Embedding: [0.019, -0.149, 0.868, ..., 0.438]  ← Very similar!

Text: "Quantum physics is complex"
Embedding: [-0.742, 0.513, -0.192, ..., 0.087]  ← Very different!
```

**Mathematical Similarity:**
```csharp
// Cosine similarity: -1 (opposite) to 1 (identical)
double similarity = CosineSimilarity(embedding1, embedding2);

// 0.98 = Very similar ("cat on mat" vs "feline on rug")
// 0.15 = Not similar ("cat on mat" vs "quantum physics")
```

---

### 6.2 Embedding Dimensions

**What it is:** The **length** of the embedding vector.

**DTO Properties:**
```csharp
// OpenAI (fixed per model)
// text-embedding-3-small: 1536 dimensions
// text-embedding-3-large: 3072 dimensions (more precise, but larger)

// Gemini (configurable)
GeminiEmbeddingRequestDto.OutputDimensionality = 768;  // e.g., 256, 512, 768
```

**Trade-offs:**

| Dimensions | Precision | Storage | Speed |
|------------|-----------|---------|-------|
| **256** | Lower | Small | Fast |
| **768** | Medium | Medium | Medium |
| **1536** | High | Large | Slower |
| **3072** | Highest | Largest | Slowest |

**Example:**
```csharp
// OpenAI - dimension is fixed per model
var request = new OpenAiEmbeddingRequestDto
{
    Model = "text-embedding-3-small",  // 1536 dimensions
    Input = new OpenAiEmbeddingInputStringDto
    {
        StringInput = "The quick brown fox"
    }
};

var response = await CallOpenAiEmbeddingApi(request);
var embedding = response.Data[0].Embedding;  // float[1536]

// Gemini - dimension is configurable
var geminiRequest = new GeminiEmbeddingRequestDto
{
    Model = "text-embedding-004",
    Content = new GeminiEmbeddingContentDto
    {
        Parts = new List<GeminiEmbeddingPartDto>
        {
            new() { Text = "The quick brown fox" }
        }
    },
    OutputDimensionality = 768  // Choose: 256, 512, 768, etc.
};
```

---

### 6.3 Task Types (Gemini)

**What it is:** Hint to optimize embeddings for **specific use cases**.

**DTO Property:**
```csharp
GeminiEmbeddingRequestDto.TaskType
```

**Options:**

**A) RETRIEVAL_QUERY**
```csharp
TaskType = "RETRIEVAL_QUERY"
```
- Use for: **User queries** in search
- Example: "What is photosynthesis?"

**B) RETRIEVAL_DOCUMENT**
```csharp
TaskType = "RETRIEVAL_DOCUMENT"
```
- Use for: **Documents** being indexed
- Example: "Photosynthesis is a process..."

**C) SEMANTIC_SIMILARITY**
```csharp
TaskType = "SEMANTIC_SIMILARITY"
```
- Use for: **Comparing texts**
- Example: Check if two sentences mean the same

**D) CLASSIFICATION**
```csharp
TaskType = "CLASSIFICATION"
```
- Use for: **Categorizing text**
- Example: Is this review positive or negative?

**E) CLUSTERING**
```csharp
TaskType = "CLUSTERING"
```
- Use for: **Grouping similar texts**
- Example: Group news articles by topic

**Complete Example:**
```csharp
// Step 1: Embed documents (indexing phase)
var documents = new[]
{
    "Photosynthesis converts sunlight to energy",
    "Mitochondria are the powerhouse of cells",
    "DNA contains genetic information"
};

var docEmbeddings = new List<float[]>();
foreach (var doc in documents)
{
    var request = new GeminiEmbeddingRequestDto
    {
        Model = "text-embedding-004",
        Content = new GeminiEmbeddingContentDto
        {
            Parts = new List<GeminiEmbeddingPartDto>
            {
                new() { Text = doc }
            }
        },
        TaskType = "RETRIEVAL_DOCUMENT",  // ← Indexing documents
        OutputDimensionality = 768
    };

    var response = await CallGeminiEmbeddingApi(request);
    docEmbeddings.Add(response.Embedding.Values);
}

// Step 2: Embed query (search phase)
var queryRequest = new GeminiEmbeddingRequestDto
{
    Model = "text-embedding-004",
    Content = new GeminiEmbeddingContentDto
    {
        Parts = new List<GeminiEmbeddingPartDto>
        {
            new() { Text = "How do plants make energy?" }
        }
    },
    TaskType = "RETRIEVAL_QUERY",  // ← Searching with a query
    OutputDimensionality = 768
};

var queryResponse = await CallGeminiEmbeddingApi(queryRequest);
var queryEmbedding = queryResponse.Embedding.Values;

// Step 3: Find most similar document
var similarities = docEmbeddings
    .Select((emb, idx) => new
    {
        Index = idx,
        Document = documents[idx],
        Similarity = CosineSimilarity(queryEmbedding, emb)
    })
    .OrderByDescending(x => x.Similarity)
    .ToList();

Console.WriteLine($"Best match: {similarities[0].Document}");
// Output: "Photosynthesis converts sunlight to energy" (similarity: 0.87)
```

---

### 6.4 Encoding Formats

**What it is:** How embedding vectors are **encoded** (affects size/precision).

**DTO Property:**
```csharp
OpenAiEmbeddingRequestDto.EncodingFormat
```

**Two Formats:**

**A) Float (Default)**
```csharp
EncodingFormat = new OpenAiEmbeddingEncodingFormatStringDto
{
    StringFormat = "float"
}
```
- Format: Array of floats
- Example: `[0.021, -0.153, 0.872, ...]`
- Size: 1536 floats × 4 bytes = **6.1 KB**

**B) Base64**
```csharp
EncodingFormat = new OpenAiEmbeddingEncodingFormatStringDto
{
    StringFormat = "base64"
}
```
- Format: Base64-encoded binary
- Example: `"AACAPwAAgD8AAIA/..."`
- Size: ~**3.0 KB** (50% smaller!)

**When to use Base64:**
```csharp
// ✅ Good: Storing millions of embeddings
// Saves 50% storage space!
var request = new OpenAiEmbeddingRequestDto
{
    Model = "text-embedding-3-small",
    Input = new OpenAiEmbeddingInputListDto
    {
        ListInput = documents  // Embed 10,000 documents
    },
    EncodingFormat = new OpenAiEmbeddingEncodingFormatStringDto
    {
        StringFormat = "base64"
    }
};

// Decode when needed
var bytes = Convert.FromBase64String(response.Data[0].Embedding);
var floats = new float[bytes.Length / 4];
Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
```

---

### 6.5 Batch Embeddings

**What it is:** Embed **multiple texts** in one API call.

**DTO Property:**
```csharp
// OpenAI: Input can be string or array
OpenAiEmbeddingRequestDto.Input = new OpenAiEmbeddingInputListDto
{
    ListInput = new List<string>
    {
        "First document",
        "Second document",
        "Third document"
    }
};

// Gemini: Embed multiple requests
var requests = new List<GeminiEmbeddingRequestDto>
{
    new() { Content = new GeminiEmbeddingContentDto { ... } },
    new() { Content = new GeminiEmbeddingContentDto { ... } }
};
```

**Example: Batch Processing**
```csharp
var documents = File.ReadAllLines("articles.txt");  // 1000 articles

// Batch into groups of 100 (API limit)
var batches = documents.Chunk(100);

var allEmbeddings = new List<float[]>();

foreach (var batch in batches)
{
    var request = new OpenAiEmbeddingRequestDto
    {
        Model = "text-embedding-3-small",
        Input = new OpenAiEmbeddingInputListDto
        {
            ListInput = batch.ToList()
        }
    };

    var response = await CallOpenAiEmbeddingApi(request);

    foreach (var data in response.Data)
    {
        allEmbeddings.Add(data.Embedding);
    }

    // Rate limiting: Wait 1 second between batches
    await Task.Delay(1000);
}

Console.WriteLine($"Embedded {allEmbeddings.Count} documents");
```

**Benefits:**
- ✅ Faster (fewer API calls)
- ✅ Cheaper (batch discounts)
- ✅ More efficient

---

### 6.6 Use Case: Semantic Search

**Goal:** Find documents similar to a query.

**Implementation:**
```csharp
public class SemanticSearchEngine
{
    private readonly List<(string Document, float[] Embedding)> _index = new();

    // Step 1: Index documents
    public async Task IndexDocumentsAsync(IEnumerable<string> documents)
    {
        var request = new OpenAiEmbeddingRequestDto
        {
            Model = "text-embedding-3-small",
            Input = new OpenAiEmbeddingInputListDto
            {
                ListInput = documents.ToList()
            }
        };

        var response = await CallOpenAiEmbeddingApi(request);

        var docList = documents.ToList();
        for (int i = 0; i < docList.Count; i++)
        {
            _index.Add((docList[i], response.Data[i].Embedding));
        }
    }

    // Step 2: Search
    public async Task<List<(string Document, double Similarity)>> SearchAsync(
        string query,
        int topK = 5)
    {
        // Embed query
        var request = new OpenAiEmbeddingRequestDto
        {
            Model = "text-embedding-3-small",
            Input = new OpenAiEmbeddingInputStringDto
            {
                StringInput = query
            }
        };

        var response = await CallOpenAiEmbeddingApi(request);
        var queryEmbedding = response.Data[0].Embedding;

        // Compute similarities
        var results = _index
            .Select(item => new
            {
                Document = item.Document,
                Similarity = CosineSimilarity(queryEmbedding, item.Embedding)
            })
            .OrderByDescending(x => x.Similarity)
            .Take(topK)
            .Select(x => (x.Document, x.Similarity))
            .ToList();

        return results;
    }

    private double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }
        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }
}

// Usage
var engine = new SemanticSearchEngine();
await engine.IndexDocumentsAsync(new[]
{
    "The Eiffel Tower is in Paris, France",
    "The Statue of Liberty is in New York, USA",
    "The Great Wall is in China",
    "The Colosseum is in Rome, Italy"
});

var results = await engine.SearchAsync("Where is the Eiffel Tower?");

foreach (var (doc, similarity) in results)
{
    Console.WriteLine($"{similarity:F3}: {doc}");
}

// Output:
// 0.912: The Eiffel Tower is in Paris, France
// 0.487: The Colosseum is in Rome, Italy
// 0.425: The Statue of Liberty is in New York, USA
// 0.391: The Great Wall is in China
```

---

### 6.7 Use Case: Text Clustering

**Goal:** Group similar documents together.

```csharp
public class DocumentClusterer
{
    public async Task<Dictionary<int, List<string>>> ClusterDocumentsAsync(
        List<string> documents,
        int numClusters)
    {
        // 1. Get embeddings
        var request = new GeminiEmbeddingRequestDto
        {
            Model = "text-embedding-004",
            TaskType = "CLUSTERING",  // ← Optimized for clustering
            OutputDimensionality = 768
        };

        var embeddings = new List<float[]>();
        foreach (var doc in documents)
        {
            request.Content = new GeminiEmbeddingContentDto
            {
                Parts = new List<GeminiEmbeddingPartDto>
                {
                    new() { Text = doc }
                }
            };

            var response = await CallGeminiEmbeddingApi(request);
            embeddings.Add(response.Embedding.Values);
        }

        // 2. K-means clustering
        var clusters = KMeans(embeddings, numClusters);

        // 3. Group documents by cluster
        var result = new Dictionary<int, List<string>>();
        for (int i = 0; i < documents.Count; i++)
        {
            int cluster = clusters[i];
            if (!result.ContainsKey(cluster))
                result[cluster] = new List<string>();
            result[cluster].Add(documents[i]);
        }

        return result;
    }

    private int[] KMeans(List<float[]> embeddings, int k)
    {
        // K-means implementation (simplified)
        // ... clustering algorithm ...
    }
}

// Usage
var clusterer = new DocumentClusterer();
var documents = new List<string>
{
    "Apple releases new iPhone",
    "Google announces Android update",
    "Stock market rises 2%",
    "Fed raises interest rates",
    "Microsoft launches AI product",
    "Tesla stock drops 5%"
};

var clusters = await clusterer.ClusterDocumentsAsync(documents, 2);

Console.WriteLine("Cluster 0 (Tech News):");
foreach (var doc in clusters[0])
    Console.WriteLine($"  - {doc}");

Console.WriteLine("\nCluster 1 (Finance News):");
foreach (var doc in clusters[1])
    Console.WriteLine($"  - {doc}");

// Output:
// Cluster 0 (Tech News):
//   - Apple releases new iPhone
//   - Google announces Android update
//   - Microsoft launches AI product
//
// Cluster 1 (Finance News):
//   - Stock market rises 2%
//   - Fed raises interest rates
//   - Tesla stock drops 5%
```

---

### 6.8 Use Case: Classification

**Goal:** Classify documents into predefined categories.

```csharp
public class TextClassifier
{
    private readonly Dictionary<string, float[]> _categoryEmbeddings = new();

    // Step 1: Train with category examples
    public async Task TrainAsync(Dictionary<string, List<string>> categoryExamples)
    {
        foreach (var (category, examples) in categoryExamples)
        {
            // Average embeddings of all examples
            var embeddings = new List<float[]>();

            foreach (var example in examples)
            {
                var request = new GeminiEmbeddingRequestDto
                {
                    Model = "text-embedding-004",
                    TaskType = "CLASSIFICATION",
                    Content = new GeminiEmbeddingContentDto
                    {
                        Parts = new List<GeminiEmbeddingPartDto>
                        {
                            new() { Text = example }
                        }
                    }
                };

                var response = await CallGeminiEmbeddingApi(request);
                embeddings.Add(response.Embedding.Values);
            }

            // Average all embeddings for this category
            var avgEmbedding = AverageEmbeddings(embeddings);
            _categoryEmbeddings[category] = avgEmbedding;
        }
    }

    // Step 2: Classify new text
    public async Task<(string Category, double Confidence)> ClassifyAsync(string text)
    {
        var request = new GeminiEmbeddingRequestDto
        {
            Model = "text-embedding-004",
            TaskType = "CLASSIFICATION",
            Content = new GeminiEmbeddingContentDto
            {
                Parts = new List<GeminiEmbeddingPartDto>
                {
                    new() { Text = text }
                }
            }
        };

        var response = await CallGeminiEmbeddingApi(request);
        var textEmbedding = response.Embedding.Values;

        // Find most similar category
        var best = _categoryEmbeddings
            .Select(kvp => new
            {
                Category = kvp.Key,
                Similarity = CosineSimilarity(textEmbedding, kvp.Value)
            })
            .OrderByDescending(x => x.Similarity)
            .First();

        return (best.Category, best.Similarity);
    }

    private float[] AverageEmbeddings(List<float[]> embeddings)
    {
        var avg = new float[embeddings[0].Length];
        foreach (var emb in embeddings)
        {
            for (int i = 0; i < emb.Length; i++)
                avg[i] += emb[i];
        }
        for (int i = 0; i < avg.Length; i++)
            avg[i] /= embeddings.Count;
        return avg;
    }
}

// Usage
var classifier = new TextClassifier();

// Train with examples
await classifier.TrainAsync(new Dictionary<string, List<string>>
{
    ["Sports"] = new()
    {
        "The team won the championship",
        "Player scores winning goal",
        "Record-breaking performance in the game"
    },
    ["Politics"] = new()
    {
        "Senator proposes new bill",
        "Election results announced",
        "Government policy changes"
    },
    ["Technology"] = new()
    {
        "New smartphone released",
        "Software update improves performance",
        "AI breakthrough announced"
    }
});

// Classify new texts
var texts = new[]
{
    "Athlete breaks world record",
    "Prime Minister addresses nation",
    "Tech company unveils new laptop"
};

foreach (var text in texts)
{
    var (category, confidence) = await classifier.ClassifyAsync(text);
    Console.WriteLine($"{text}");
    Console.WriteLine($"  → {category} ({confidence:P})");
}

// Output:
// Athlete breaks world record
//   → Sports (92.3%)
// Prime Minister addresses nation
//   → Politics (88.7%)
// Tech company unveils new laptop
//   → Technology (94.1%)
```

---

### Embeddings Best Practices

1. **Choose Appropriate Dimensions:**
   ```csharp
   // Large dataset (millions): Use smaller dimensions
   OutputDimensionality = 256  // Faster, less storage

   // High precision needed: Use larger dimensions
   OutputDimensionality = 1536  // More accurate
   ```

2. **Use Task Types (Gemini):**
   ```csharp
   // Indexing phase
   TaskType = "RETRIEVAL_DOCUMENT"

   // Search phase
   TaskType = "RETRIEVAL_QUERY"
   ```

3. **Batch for Efficiency:**
   ```csharp
   // ✅ Good: Batch 100 documents
   Input = new OpenAiEmbeddingInputListDto { ListInput = documents }

   // ❌ Bad: 100 separate API calls
   ```

4. **Use Base64 for Storage:**
   ```csharp
   // Saves 50% space when storing millions of embeddings
   EncodingFormat = new OpenAiEmbeddingEncodingFormatStringDto
   {
       StringFormat = "base64"
   }
   ```

5. **Cache Embeddings:**
   ```csharp
   var cache = new Dictionary<string, float[]>();

   if (!cache.TryGetValue(text, out var embedding))
   {
       embedding = await GetEmbeddingAsync(text);
       cache[text] = embedding;
   }
   ```

---

### Embeddings Summary

| Concept | Purpose | Example |
|---------|---------|---------|
| **Embedding** | Numerical representation of text | [0.021, -0.153, ...] |
| **Dimensions** | Vector length (precision) | 256, 768, 1536, 3072 |
| **Task Type** | Optimize for use case | RETRIEVAL_QUERY, CLUSTERING |
| **Encoding** | Storage format | "float" (6KB), "base64" (3KB) |
| **Batch** | Multiple texts at once | Embed 100 documents per call |
| **Similarity** | Compare vectors | Cosine similarity: 0-1 |

**When to Use Embeddings:**
- ✅ Semantic search (find similar documents)
- ✅ Recommendation systems (suggest similar items)
- ✅ Clustering (group related content)
- ✅ Classification (categorize text)
- ✅ Duplicate detection (find near-duplicates)
- ✅ RAG systems (retrieve relevant context)
- ❌ Simple keyword search (use database full-text search)
- ❌ Exact matching (use string comparison)

---

## 7. Streaming

**Streaming** delivers AI responses **incrementally** as they're generated, instead of waiting for the complete response.

### 7.1 Streaming vs Batch

**Batch Mode (Default):**
```
User: "Write a story about a dragon"
   ↓
[Wait 10 seconds...]
   ↓
Response: "Once upon a time, in a land far away, there lived a magnificent dragon..."
```

**Streaming Mode:**
```
User: "Write a story about a dragon"
   ↓
"Once" → "upon" → "a" → "time," → "in" → "a" → "land" → "far" → "away..."
└─ Each word arrives immediately as generated
```

**Benefits:**
- ✅ **Faster perceived response** (user sees progress immediately)
- ✅ **Better UX** (like ChatGPT's typing effect)
- ✅ **Lower latency** (start processing early tokens while waiting for rest)

**DTO Property:**
```csharp
// OpenAI
OpenAiChatRequestDto.Stream = true;

// Gemini (always streams by default in some SDKs)
// Control via API client settings
```

---

### 7.2 Stream Response Structure

#### OpenAI Stream Chunks

**Response:** Series of `OpenAiChatStreamResponseDto` objects.

```csharp
public class OpenAiChatStreamResponseDto
{
    public List<OpenAiChatStreamChoiceDto>? Choices { get; set; }
    public OpenAiChatUsageDto? Usage { get; set; }  // Only in last chunk
}

public class OpenAiChatStreamChoiceDto
{
    public int? Index { get; set; }
    public OpenAiChatStreamDeltaDto? Delta { get; set; }  // ← Incremental content
    public string? FinishReason { get; set; }
}

public class OpenAiChatStreamDeltaDto
{
    public string? Role { get; set; }  // Only in first chunk
    public object? Content { get; set; }  // New text
    public List<OpenAiChatToolCallDto>? ToolCalls { get; set; }  // Function calls
}
```

**Example Stream:**
```csharp
// Chunk 1
{
    "choices": [{
        "index": 0,
        "delta": {
            "role": "assistant",
            "content": "The"
        }
    }]
}

// Chunk 2
{
    "choices": [{
        "index": 0,
        "delta": {
            "content": " quick"
        }
    }]
}

// Chunk 3
{
    "choices": [{
        "index": 0,
        "delta": {
            "content": " brown"
        }
    }]
}

// ... more chunks ...

// Final chunk
{
    "choices": [{
        "index": 0,
        "delta": {},
        "finish_reason": "stop"
    }],
    "usage": {
        "prompt_tokens": 10,
        "completion_tokens": 25,
        "total_tokens": 35
    }
}
```

---

#### Gemini Stream Chunks

**Response:** Series of `GeminiChatResponseDto` objects.

```csharp
// Each chunk is a complete GeminiChatResponseDto
// with partial content in Candidates[0].Content.Parts
```

---

### 7.3 Consuming Streams

#### OpenAI Streaming Example

```csharp
public async Task StreamChatAsync(string userMessage)
{
    var request = new OpenAiChatRequestDto
    {
        Model = "gpt-4",
        Messages = new List<OpenAiChatMessageBaseDto>
        {
            new OpenAiChatMessageUserDto
            {
                Role = "user",
                Content = new OpenAiChatMessageContentStringDto
                {
                    StringContent = userMessage
                }
            }
        },
        Stream = true  // ← Enable streaming
    };

    var fullResponse = new StringBuilder();

    await foreach (var chunk in CallOpenAiStreamingApi(request))
    {
        if (chunk.Choices == null || chunk.Choices.Count == 0)
            continue;

        var delta = chunk.Choices[0].Delta;

        if (delta?.Content != null)
        {
            // Extract text content
            string text = ExtractTextContent(delta.Content);

            // Display immediately
            Console.Write(text);
            fullResponse.Append(text);
        }

        // Check if done
        if (chunk.Choices[0].FinishReason != null)
        {
            Console.WriteLine("\n\n--- Stream Complete ---");
            Console.WriteLine($"Finish Reason: {chunk.Choices[0].FinishReason}");

            if (chunk.Usage != null)
            {
                Console.WriteLine($"Tokens Used: {chunk.Usage.TotalTokens}");
            }
        }
    }

    return fullResponse.ToString();
}
```

**Output:**
```
The→ quick→ brown→ fox→ jumps→ over→ the→ lazy→ dog.

--- Stream Complete ---
Finish Reason: stop
Tokens Used: 35
```

---

#### Gemini Streaming Example

```csharp
public async Task StreamGeminiChatAsync(string userMessage)
{
    var request = new GeminiChatRequestDto
    {
        Contents = new List<GeminiChatContentDto>
        {
            new()
            {
                Role = "user",
                Parts = new List<GeminiChatPartDto>
                {
                    new() { Text = userMessage }
                }
            }
        }
    };

    var fullResponse = new StringBuilder();

    await foreach (var chunk in CallGeminiStreamingApi(request))
    {
        if (chunk.Candidates == null || chunk.Candidates.Count == 0)
            continue;

        var candidate = chunk.Candidates[0];

        foreach (var part in candidate.Content.Parts)
        {
            if (part.Text != null)
            {
                Console.Write(part.Text);
                fullResponse.Append(part.Text);
            }
        }

        if (candidate.FinishReason != null)
        {
            Console.WriteLine($"\n\nFinish Reason: {candidate.FinishReason}");
        }
    }

    return fullResponse.ToString();
}
```

---

### 7.4 Stream Options (OpenAI)

**What it is:** Additional data to include in streams.

**DTO Property:**
```csharp
public class OpenAiChatStreamOptionsDto
{
    // Include usage stats in final chunk
    public bool? IncludeUsage { get; set; }

    // Include character-level obfuscation info
    public bool? IncludeObfuscation { get; set; }
}
```

**Example:**
```csharp
var request = new OpenAiChatRequestDto
{
    Model = "gpt-4",
    Messages = messages,
    Stream = true,
    StreamOptions = new OpenAiChatStreamOptionsDto
    {
        IncludeUsage = true  // ← Get token counts in final chunk
    }
};
```

**Final Chunk with Usage:**
```json
{
    "choices": [{
        "index": 0,
        "delta": {},
        "finish_reason": "stop"
    }],
    "usage": {
        "prompt_tokens": 15,
        "completion_tokens": 42,
        "total_tokens": 57
    }
}
```

**Why useful:**
- Track token usage in real-time
- Cost estimation during generation
- Debugging

---

### 7.5 Handling Function Calls in Streams

**Challenge:** Function calls arrive incrementally.

**Example Stream:**
```csharp
// Chunk 1
{
    "delta": {
        "tool_calls": [{
            "index": 0,
            "id": "call_abc123",
            "type": "function",
            "function": {
                "name": "get_"  // ← Partial name
            }
        }]
    }
}

// Chunk 2
{
    "delta": {
        "tool_calls": [{
            "index": 0,
            "function": {
                "name": "weather"  // ← Rest of name
            }
        }]
    }
}

// Chunk 3
{
    "delta": {
        "tool_calls": [{
            "index": 0,
            "function": {
                "arguments": "{\"loc"  // ← Partial arguments
            }
        }]
    }
}

// Chunk 4
{
    "delta": {
        "tool_calls": [{
            "index": 0,
            "function": {
                "arguments": "ation\": \"Tokyo\"}"  // ← Rest of arguments
            }
        }]
    }
}
```

**Accumulation Logic:**
```csharp
public async Task HandleStreamWithToolCallsAsync()
{
    var toolCallAccumulator = new Dictionary<int, ToolCallBuilder>();

    await foreach (var chunk in CallOpenAiStreamingApi(request))
    {
        var delta = chunk.Choices[0].Delta;

        if (delta.ToolCalls != null)
        {
            foreach (var toolCall in delta.ToolCalls)
            {
                int index = toolCall.Index ?? 0;

                if (!toolCallAccumulator.ContainsKey(index))
                {
                    toolCallAccumulator[index] = new ToolCallBuilder
                    {
                        Id = toolCall.Id,
                        Type = toolCall.Type
                    };
                }

                var builder = toolCallAccumulator[index];

                if (toolCall.Function?.Name != null)
                    builder.FunctionName += toolCall.Function.Name;

                if (toolCall.Function?.Arguments != null)
                    builder.Arguments += toolCall.Function.Arguments;
            }
        }

        if (chunk.Choices[0].FinishReason == "tool_calls")
        {
            // All tool calls received - execute them
            foreach (var builder in toolCallAccumulator.Values)
            {
                Console.WriteLine($"Complete Tool Call:");
                Console.WriteLine($"  Function: {builder.FunctionName}");
                Console.WriteLine($"  Arguments: {builder.Arguments}");

                var result = ExecuteFunction(builder.FunctionName, builder.Arguments);
                // ... continue conversation ...
            }
        }
    }
}

class ToolCallBuilder
{
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string FunctionName { get; set; } = "";
    public string Arguments { get; set; } = "";
}
```

---

### 7.6 UI Integration: Real-time Display

**Example: Console UI**
```csharp
public async Task DisplayStreamWithTypingEffectAsync(string userMessage)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("Assistant: ");
    Console.ResetColor();

    var request = new OpenAiChatRequestDto
    {
        Model = "gpt-4",
        Messages = new List<OpenAiChatMessageBaseDto>
        {
            new OpenAiChatMessageUserDto
            {
                Role = "user",
                Content = new OpenAiChatMessageContentStringDto
                {
                    StringContent = userMessage
                }
            }
        },
        Stream = true
    };

    await foreach (var chunk in CallOpenAiStreamingApi(request))
    {
        if (chunk.Choices?[0]?.Delta?.Content != null)
        {
            string text = ExtractTextContent(chunk.Choices[0].Delta.Content);
            Console.Write(text);

            // Optional: Add slight delay for typing effect
            await Task.Delay(20);
        }
    }

    Console.WriteLine("\n");
}
```

**Example: Web UI (SignalR)**
```csharp
public async Task StreamToWebClientAsync(string connectionId, string userMessage)
{
    var request = new OpenAiChatRequestDto
    {
        Model = "gpt-4",
        Messages = new List<OpenAiChatMessageBaseDto>
        {
            new OpenAiChatMessageUserDto
            {
                Role = "user",
                Content = new OpenAiChatMessageContentStringDto
                {
                    StringContent = userMessage
                }
            }
        },
        Stream = true
    };

    await foreach (var chunk in CallOpenAiStreamingApi(request))
    {
        if (chunk.Choices?[0]?.Delta?.Content != null)
        {
            string text = ExtractTextContent(chunk.Choices[0].Delta.Content);

            // Send to web client via SignalR
            await _hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveMessageChunk", text);
        }

        if (chunk.Choices?[0]?.FinishReason != null)
        {
            // Notify client that streaming is complete
            await _hubContext.Clients.Client(connectionId)
                .SendAsync("StreamComplete", chunk.Choices[0].FinishReason);
        }
    }
}
```

---

### 7.7 Error Handling in Streams

**Challenge:** Errors can occur mid-stream.

**Robust Stream Handling:**
```csharp
public async Task<string> StreamWithErrorHandlingAsync(string userMessage)
{
    var fullResponse = new StringBuilder();
    int retryCount = 0;
    const int maxRetries = 3;

    while (retryCount < maxRetries)
    {
        try
        {
            var request = new OpenAiChatRequestDto
            {
                Model = "gpt-4",
                Messages = new List<OpenAiChatMessageBaseDto>
                {
                    new OpenAiChatMessageUserDto
                    {
                        Role = "user",
                        Content = new OpenAiChatMessageContentStringDto
                        {
                            StringContent = userMessage
                        }
                    }
                },
                Stream = true
            };

            await foreach (var chunk in CallOpenAiStreamingApi(request))
            {
                if (chunk.Choices?[0]?.Delta?.Content != null)
                {
                    string text = ExtractTextContent(chunk.Choices[0].Delta.Content);
                    Console.Write(text);
                    fullResponse.Append(text);
                }

                if (chunk.Choices?[0]?.FinishReason != null)
                {
                    // Success!
                    return fullResponse.ToString();
                }
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            // Rate limit - wait and retry
            retryCount++;
            Console.WriteLine($"\n⚠️ Rate limited. Retry {retryCount}/{maxRetries} in 5s...");
            await Task.Delay(5000);
        }
        catch (OperationCanceledException)
        {
            // User canceled - return partial response
            Console.WriteLine("\n⚠️ Stream canceled by user.");
            return fullResponse.ToString();
        }
        catch (Exception ex)
        {
            // Unexpected error
            Console.WriteLine($"\n❌ Stream error: {ex.Message}");

            if (fullResponse.Length > 0)
            {
                Console.WriteLine("Returning partial response.");
                return fullResponse.ToString();
            }

            throw;
        }
    }

    throw new Exception("Max retries exceeded");
}
```

---

### 7.8 Performance Considerations

**Stream Buffering:**
```csharp
// ❌ Bad: Process every tiny chunk
await foreach (var chunk in stream)
{
    if (chunk.Choices?[0]?.Delta?.Content != null)
    {
        UpdateUI(chunk.Choices[0].Delta.Content);  // ← 100+ UI updates/second!
    }
}

// ✅ Good: Buffer chunks for smoother UI updates
var buffer = new StringBuilder();
var lastUpdate = DateTime.UtcNow;

await foreach (var chunk in stream)
{
    if (chunk.Choices?[0]?.Delta?.Content != null)
    {
        buffer.Append(chunk.Choices[0].Delta.Content);

        // Update UI every 100ms
        if ((DateTime.UtcNow - lastUpdate).TotalMilliseconds >= 100)
        {
            UpdateUI(buffer.ToString());
            buffer.Clear();
            lastUpdate = DateTime.UtcNow;
        }
    }
}

// Final update
if (buffer.Length > 0)
    UpdateUI(buffer.ToString());
```

---

### Streaming Best Practices

1. **Always Accumulate Full Response:**
   ```csharp
   var fullResponse = new StringBuilder();
   await foreach (var chunk in stream)
   {
       fullResponse.Append(GetChunkText(chunk));
   }
   // Save fullResponse to database/logs
   ```

2. **Handle Tool Calls Carefully:**
   ```csharp
   // Accumulate function name and arguments across chunks
   var toolCallBuilder = new Dictionary<int, StringBuilder>();
   ```

3. **Check Finish Reason:**
   ```csharp
   if (chunk.Choices[0].FinishReason == "length")
   {
       // Response was truncated!
   }
   ```

4. **Include Usage Stats:**
   ```csharp
   StreamOptions = new OpenAiChatStreamOptionsDto
   {
       IncludeUsage = true  // Get token counts
   }
   ```

5. **Implement Error Recovery:**
   ```csharp
   // Return partial response if stream fails mid-way
   catch (Exception ex)
   {
       if (fullResponse.Length > 0)
           return fullResponse.ToString();
       throw;
   }
   ```

6. **Buffer UI Updates:**
   ```csharp
   // Update UI every 100ms, not every chunk
   ```

---

### Streaming Summary

| Concept | Purpose | Example |
|---------|---------|---------|
| **Stream** | Incremental response delivery | "The" → "quick" → "brown" |
| **Delta** | New content in each chunk | { "content": " fox" } |
| **Stream Options** | Control stream metadata | IncludeUsage, IncludeObfuscation |
| **Accumulation** | Rebuild full response/tool calls | StringBuilder for text |
| **Finish Reason** | Why stream ended | "stop", "length", "tool_calls" |
| **Buffering** | Smooth UI updates | Update every 100ms |

**When to Use Streaming:**
- ✅ Interactive chat interfaces
- ✅ Long responses (stories, articles, code)
- ✅ Better perceived performance
- ✅ Real-time feedback to users
- ❌ Batch processing (no user waiting)
- ❌ Automated systems (no UI benefit)
- ❌ When you need complete response first (e.g., JSON parsing)

---

## 8. Grounding & Citations

**Grounding** connects AI responses to **external knowledge sources** (documents, web search). **Citations** provide **attribution** for where information came from.

### 8.1 What is Grounding?

**Problem:** LLMs can:
- Generate outdated information (training data cutoff)
- Hallucinate facts
- Lack domain-specific knowledge

**Solution:** Ground responses in **real documents/data**.

**Simple Example:**
```
Without Grounding:
User: "What's the weather in Tokyo today?"
Model: "I don't have access to current weather data."

With Grounding (Web Search):
User: "What's the weather in Tokyo today?"
Model searches web → Finds "22°C, sunny"
Model: "The weather in Tokyo today is 22°C and sunny." [Source: weather.com]
```

---

### 8.2 Gemini Grounding

**What it is:** Gemini can ground responses using:
- **Google Search** (web search)
- **Your documents** (retrieval-augmented generation/RAG)

#### DTO Structure

```csharp
public class GeminiChatToolDto
{
    // Google Search grounding
    public GeminiChatGoogleSearchRetrievalDto? GoogleSearchRetrieval { get; set; }

    // Custom document grounding
    public GeminiChatRetrievalDto? Retrieval { get; set; }
}

public class GeminiChatGoogleSearchRetrievalDto
{
    // Optional: Control how search results are filtered/ranked
    public object? DynamicRetrievalConfig { get; set; }
}
```

**Example: Google Search Grounding**
```csharp
var request = new GeminiChatRequestDto
{
    Contents = new List<GeminiChatContentDto>
    {
        new()
        {
            Role = "user",
            Parts = new List<GeminiChatPartDto>
            {
                new() { Text = "What are the latest developments in quantum computing?" }
            }
        }
    },
    Tools = new List<GeminiChatToolDto>
    {
        new()
        {
            GoogleSearchRetrieval = new GeminiChatGoogleSearchRetrievalDto()
            // ↑ Enables Google Search grounding
        }
    }
};

var response = await CallGeminiApi(request);

// Response includes search results
var candidate = response.Candidates[0];
foreach (var part in candidate.Content.Parts)
{
    Console.WriteLine(part.Text);
}

// Check grounding metadata
if (candidate.GroundingMetadata != null)
{
    Console.WriteLine("\nGrounding Sources:");
    foreach (var chunk in candidate.GroundingMetadata.GroundingChunks)
    {
        if (chunk.Web != null)
        {
            Console.WriteLine($"- {chunk.Web.Title}: {chunk.Web.Uri}");
        }
    }
}
```

**Response:**
```
Recent developments in quantum computing include IBM's announcement of a 433-qubit processor,
Google's progress on error correction, and Microsoft's topological qubit research.

Grounding Sources:
- IBM unveils 433-qubit quantum computer: https://www.ibm.com/quantum/...
- Google achieves quantum error correction milestone: https://blog.google/technology/...
- Microsoft advances topological qubits: https://www.microsoft.com/research/...
```

---

### 8.3 Grounding Metadata (Gemini)

**What it is:** Details about **which sources** were used.

```csharp
public class GeminiChatGroundingMetadataDto
{
    // Chunks of grounded content
    public List<GeminiChatGroundingChunkDto>? GroundingChunks { get; set; }

    // Which chunks supported the response
    public List<GeminiChatGroundingSupportDto>? GroundingSupports { get; set; }

    // Web search queries executed
    public List<string>? SearchEntryPoint { get; set; }
}

public class GeminiChatGroundingChunkDto
{
    // Web source
    public GeminiChatGroundingChunkWebDto? Web { get; set; }

    // Custom document source
    public GeminiChatGroundingChunkRetrievedContextDto? RetrievedContext { get; set; }
}

public class GeminiChatGroundingChunkWebDto
{
    public string? Uri { get; set; }
    public string? Title { get; set; }
}
```

**Example: Analyzing Grounding Metadata**
```csharp
var response = await CallGeminiWithGroundingApi(request);
var metadata = response.Candidates[0].GroundingMetadata;

Console.WriteLine("Search Queries Used:");
foreach (var query in metadata.SearchEntryPoint)
{
    Console.WriteLine($"  - {query}");
}

Console.WriteLine("\nSources Retrieved:");
foreach (var chunk in metadata.GroundingChunks)
{
    if (chunk.Web != null)
    {
        Console.WriteLine($"  [{chunk.Web.Title}]");
        Console.WriteLine($"  {chunk.Web.Uri}");
    }
}

Console.WriteLine("\nGrounding Support:");
foreach (var support in metadata.GroundingSupports)
{
    Console.WriteLine($"  Segment [{support.SegmentStartIndex}-{support.SegmentEndIndex}]:");
    foreach (var index in support.GroundingChunkIndices)
    {
        var chunk = metadata.GroundingChunks[index];
        Console.WriteLine($"    → {chunk.Web.Title}");
    }
}
```

**Output:**
```
Search Queries Used:
  - latest quantum computing developments 2024

Sources Retrieved:
  [IBM unveils 433-qubit quantum computer]
  https://www.ibm.com/quantum/blog/2023-ibm-quantum-roadmap

  [Google achieves quantum error correction milestone]
  https://blog.google/technology/research/google-quantum-ai-willow/

Grounding Support:
  Segment [0-87]:
    → IBM unveils 433-qubit quantum computer
  Segment [89-156]:
    → Google achieves quantum error correction milestone
```

---

### 8.4 OpenAI Citations & Annotations

**What it is:** Track which parts of documents were used (for file search).

#### DTO Structure

```csharp
public class OpenAiChatMessageAnnotationDto
{
    public string? Type { get; set; }  // "file_citation", "file_path"
    public string? Text { get; set; }  // The cited text

    // Citation details
    public OpenAiChatFileCitationDto? FileCitation { get; set; }

    // Start/end positions in response
    public int? StartIndex { get; set; }
    public int? EndIndex { get; set; }
}

public class OpenAiChatFileCitationDto
{
    public string? FileId { get; set; }  // Which file was cited
    public string? Quote { get; set; }   // Exact quote from file
}
```

**Example: Processing Citations**
```csharp
var response = await CallOpenAiWithFileSearchApi(request);
var message = response.Choices[0].Message;

// Display response text
Console.WriteLine("Response:");
Console.WriteLine(message.Content);

// Display citations
if (message.Annotations != null && message.Annotations.Count > 0)
{
    Console.WriteLine("\nCitations:");
    int citationNum = 1;

    foreach (var annotation in message.Annotations)
    {
        if (annotation.FileCitation != null)
        {
            Console.WriteLine($"[{citationNum}] File: {annotation.FileCitation.FileId}");
            Console.WriteLine($"    Quote: \"{annotation.FileCitation.Quote}\"");
            Console.WriteLine($"    Position: {annotation.StartIndex}-{annotation.EndIndex}");
            citationNum++;
        }
    }
}
```

**Output:**
```
Response:
The company's Q4 revenue was $1.2 billion, representing a 15% increase year-over-year [1].
The growth was primarily driven by strong performance in the cloud services division [2].

Citations:
[1] File: file-abc123
    Quote: "Q4 revenue reached $1.2 billion, up 15% from the previous year"
    Position: 28-88

[2] File: file-abc123
    Quote: "Cloud services division contributed 60% of total revenue growth"
    Position: 140-195
```

---

### 8.5 Complete RAG Example (Custom Documents)

**Scenario:** Answer questions using your own documents.

```csharp
public class CustomRagSystem
{
    private readonly List<(string DocId, string Content, float[] Embedding)> _documents = new();

    // Step 1: Index documents
    public async Task IndexDocumentAsync(string docId, string content)
    {
        // Get embedding
        var embeddingRequest = new OpenAiEmbeddingRequestDto
        {
            Model = "text-embedding-3-small",
            Input = new OpenAiEmbeddingInputStringDto
            {
                StringInput = content
            }
        };

        var embeddingResponse = await CallOpenAiEmbeddingApi(embeddingRequest);
        var embedding = embeddingResponse.Data[0].Embedding;

        _documents.Add((docId, content, embedding));
    }

    // Step 2: Retrieve relevant documents
    private async Task<List<(string DocId, string Content, double Similarity)>>
        RetrieveRelevantDocumentsAsync(string query, int topK = 3)
    {
        // Embed query
        var queryEmbedding = await GetEmbeddingAsync(query);

        // Find similar documents
        var results = _documents
            .Select(doc => new
            {
                DocId = doc.DocId,
                Content = doc.Content,
                Similarity = CosineSimilarity(queryEmbedding, doc.Embedding)
            })
            .OrderByDescending(x => x.Similarity)
            .Take(topK)
            .Select(x => (x.DocId, x.Content, x.Similarity))
            .ToList();

        return results;
    }

    // Step 3: Generate grounded response
    public async Task<string> AskQuestionAsync(string question)
    {
        // Retrieve relevant documents
        var relevantDocs = await RetrieveRelevantDocumentsAsync(question);

        // Build context from documents
        var context = new StringBuilder();
        context.AppendLine("Relevant documents:");
        foreach (var (docId, content, similarity) in relevantDocs)
        {
            context.AppendLine($"\n[Document {docId}] (Relevance: {similarity:F2})");
            context.AppendLine(content);
        }

        // Generate response with context
        var chatRequest = new OpenAiChatRequestDto
        {
            Model = "gpt-4",
            Messages = new List<OpenAiChatMessageBaseDto>
            {
                new OpenAiChatMessageSystemDto
                {
                    Role = "system",
                    Content = new OpenAiChatMessageContentStringDto
                    {
                        StringContent = @"
You are a helpful assistant. Answer the user's question based on the provided documents.
Always cite which document you're referencing (e.g., [Document 1]).
If the documents don't contain the answer, say so."
                    }
                },
                new OpenAiChatMessageUserDto
                {
                    Role = "user",
                    Content = new OpenAiChatMessageContentStringDto
                    {
                        StringContent = $@"
{context}

Question: {question}

Please answer based on the documents above."
                    }
                }
            }
        };

        var response = await CallOpenAiApi(chatRequest);
        return response.Choices[0].Message.Content;
    }
}

// Usage
var ragSystem = new CustomRagSystem();

// Index documents
await ragSystem.IndexDocumentAsync("doc1",
    "Our company's Q4 revenue was $1.2 billion, up 15% from last year.");
await ragSystem.IndexDocumentAsync("doc2",
    "The cloud services division grew 30% and now represents 60% of total revenue.");
await ragSystem.IndexDocumentAsync("doc3",
    "We hired 500 new employees in Q4, bringing total headcount to 5,000.");

// Ask questions
var answer = await ragSystem.AskQuestionAsync("What was the Q4 revenue?");
Console.WriteLine(answer);
// Output: "According to [Document 1], Q4 revenue was $1.2 billion, representing a 15% increase."

var answer2 = await ragSystem.AskQuestionAsync("How many employees do we have?");
Console.WriteLine(answer2);
// Output: "Based on [Document 3], the company has 5,000 employees total after hiring 500 in Q4."
```

---

### 8.6 Grounding Confidence Scores

**What it is:** How confident the model is that response is **grounded in sources**.

**DTO Property (Gemini):**
```csharp
response.Candidates[0].GroundingMetadata.GroundingSupports
```

**Example: Filtering Low-Confidence Responses**
```csharp
var response = await CallGeminiWithGroundingApi(request);
var metadata = response.Candidates[0].GroundingMetadata;

// Check if well-grounded
bool isWellGrounded = metadata.GroundingSupports?.Count > 0;

if (!isWellGrounded)
{
    Console.WriteLine("⚠️ Warning: Response may not be well-grounded in sources.");
    Console.WriteLine("Consider rephrasing the question or checking the answer manually.");
}
else
{
    Console.WriteLine($"✅ Response grounded in {metadata.GroundingSupports.Count} sources.");
}
```

---

### 8.7 Dynamic Retrieval (Gemini)

**What it is:** Automatically decide **when to search** for information.

**DTO Property:**
```csharp
public class GeminiChatDynamicRetrievalConfigDto
{
    // Threshold for triggering search (0.0-1.0)
    public double? DynamicThreshold { get; set; }

    // Mode: "dynamic" or "always"
    public string? Mode { get; set; }
}
```

**Example:**
```csharp
var request = new GeminiChatRequestDto
{
    Contents = contents,
    Tools = new List<GeminiChatToolDto>
    {
        new()
        {
            GoogleSearchRetrieval = new GeminiChatGoogleSearchRetrievalDto
            {
                DynamicRetrievalConfig = new GeminiChatDynamicRetrievalConfigDto
                {
                    Mode = "dynamic",  // Only search when needed
                    DynamicThreshold = 0.7  // Search if uncertainty > 70%
                }
            }
        }
    }
};
```

**Behavior:**
```
User: "What is 2+2?"
→ Model knows answer → No search needed

User: "What's the current price of Bitcoin?"
→ Model uncertain (needs current data) → Triggers Google Search
```

---

### Grounding Best Practices

1. **Always Check Grounding Metadata:**
   ```csharp
   if (response.Candidates[0].GroundingMetadata?.GroundingChunks?.Count > 0)
   {
       // Response is grounded - show sources
   }
   else
   {
       // Not grounded - may be hallucinated
   }
   ```

2. **Display Sources to Users:**
   ```csharp
   Console.WriteLine("\nSources:");
   foreach (var chunk in metadata.GroundingChunks)
   {
       Console.WriteLine($"- {chunk.Web.Title}: {chunk.Web.Uri}");
   }
   ```

3. **Use Dynamic Retrieval:**
   ```csharp
   // Only search when model needs external info
   DynamicRetrievalConfig = new()
   {
       Mode = "dynamic",
       DynamicThreshold = 0.7
   }
   ```

4. **Combine with RAG:**
   ```csharp
   // Step 1: Retrieve your documents (RAG)
   var relevantDocs = await RetrieveRelevantDocumentsAsync(query);

   // Step 2: Also enable web search for current info
   Tools = new List<GeminiChatToolDto>
   {
       new() { GoogleSearchRetrieval = new() }
   }
   ```

5. **Validate Grounding Quality:**
   ```csharp
   // Check if response segments are well-supported
   var supportedSegments = metadata.GroundingSupports?.Count ?? 0;
   var totalResponse = response.Candidates[0].Content.Parts[0].Text.Length;

   var groundingCoverage = (double)supportedSegments / totalResponse;

   if (groundingCoverage < 0.5)
   {
       Console.WriteLine("⚠️ Less than 50% of response is grounded");
   }
   ```

---

### Grounding & Citations Summary

| Concept | Purpose | Example |
|---------|---------|---------|
| **Grounding** | Connect responses to sources | Web search, documents |
| **Google Search Retrieval** | Search web for current info | News, facts, prices |
| **Grounding Metadata** | Track which sources used | URLs, titles, quotes |
| **Citations** | Attribution for quoted text | [Document 1], [Source] |
| **Dynamic Retrieval** | Search only when needed | Uncertainty threshold |
| **RAG** | Use your own documents | Custom knowledge base |

**When to Use Grounding:**
- ✅ Factual questions (current events, data)
- ✅ Domain-specific knowledge (your documents)
- ✅ Reducing hallucinations
- ✅ Requiring source attribution
- ✅ Building trustworthy AI systems
- ❌ Creative writing (no sources needed)
- ❌ Opinion-based responses
- ❌ General knowledge (model already knows)

---

## 9. Safety & Moderation

**Safety** controls ensure AI responses are **appropriate** and **responsible**. Both OpenAI and Gemini provide safety filtering mechanisms.

### 9.1 Gemini Safety Settings

**What it is:** Control how strictly to filter potentially harmful content.

#### DTO Structure

```csharp
public class GeminiChatSafetySettingDto
{
    // Category of harm to filter
    public string? Category { get; set; }

    // Threshold for blocking
    public string? Threshold { get; set; }
}
```

**Safety Categories:**
- **HARM_CATEGORY_HATE_SPEECH:** Hate speech, discrimination
- **HARM_CATEGORY_DANGEROUS_CONTENT:** Violence, self-harm instructions
- **HARM_CATEGORY_HARASSMENT:** Bullying, threats
- **HARM_CATEGORY_SEXUALLY_EXPLICIT:** Adult content

**Block Thresholds:**
- **BLOCK_NONE:** Don't block anything (use with caution!)
- **BLOCK_ONLY_HIGH:** Block only high-probability harmful content
- **BLOCK_MEDIUM_AND_ABOVE:** Block medium and high (default)
- **BLOCK_LOW_AND_ABOVE:** Block low, medium, and high (strictest)

**Example:**
```csharp
var request = new GeminiChatRequestDto
{
    Contents = contents,
    SafetySettings = new List<GeminiChatSafetySettingDto>
    {
        new()
        {
            Category = "HARM_CATEGORY_HATE_SPEECH",
            Threshold = "BLOCK_MEDIUM_AND_ABOVE"
        },
        new()
        {
            Category = "HARM_CATEGORY_DANGEROUS_CONTENT",
            Threshold = "BLOCK_LOW_AND_ABOVE"  // Strictest for dangerous content
        },
        new()
        {
            Category = "HARM_CATEGORY_HARASSMENT",
            Threshold = "BLOCK_MEDIUM_AND_ABOVE"
        },
        new()
        {
            Category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
            Threshold = "BLOCK_MEDIUM_AND_ABOVE"
        }
    }
};
```

---

### 9.2 Safety Ratings (Gemini)

**What it is:** Probability that content contains harmful material.

#### DTO Structure

```csharp
public class GeminiChatSafetyRatingDto
{
    // Which category
    public string? Category { get; set; }

    // Probability: "NEGLIGIBLE", "LOW", "MEDIUM", "HIGH"
    public string? Probability { get; set; }

    // Was this content blocked?
    public bool? Blocked { get; set; }
}
```

**Example: Checking Safety Ratings**
```csharp
var response = await CallGeminiApi(request);
var candidate = response.Candidates[0];

// Check if response was blocked
if (candidate.FinishReason == "SAFETY")
{
    Console.WriteLine("❌ Response blocked due to safety concerns:");

    foreach (var rating in candidate.SafetyRatings)
    {
        if (rating.Blocked == true)
        {
            Console.WriteLine($"  Category: {rating.Category}");
            Console.WriteLine($"  Probability: {rating.Probability}");
        }
    }

    return "I cannot provide a response to that request.";
}

// Check ratings even for non-blocked responses
Console.WriteLine("Safety Ratings:");
foreach (var rating in candidate.SafetyRatings)
{
    Console.WriteLine($"  {rating.Category}: {rating.Probability}");
}
```

**Output Example:**
```
Safety Ratings:
  HARM_CATEGORY_HATE_SPEECH: NEGLIGIBLE
  HARM_CATEGORY_DANGEROUS_CONTENT: LOW
  HARM_CATEGORY_HARASSMENT: NEGLIGIBLE
  HARM_CATEGORY_SEXUALLY_EXPLICIT: NEGLIGIBLE
```

---

### 9.3 Prompt Feedback (Gemini)

**What it is:** Indicates if the **prompt itself** triggered safety filters.

#### DTO Structure

```csharp
public class GeminiChatPromptFeedbackDto
{
    // Block reason (if blocked)
    public string? BlockReason { get; set; }

    // Safety ratings for the prompt
    public List<GeminiChatSafetyRatingDto>? SafetyRatings { get; set; }
}
```

**Block Reasons:**
- **"SAFETY":** Safety concerns
- **"OTHER":** Other policy violations
- **"BLOCKLIST":** Matched blocklist
- **"PROHIBITED_CONTENT":** Prohibited content

**Example:**
```csharp
var response = await CallGeminiApi(request);

// Check if prompt was blocked
if (response.PromptFeedback?.BlockReason != null)
{
    Console.WriteLine("❌ Prompt blocked!");
    Console.WriteLine($"Reason: {response.PromptFeedback.BlockReason}");

    if (response.PromptFeedback.SafetyRatings != null)
    {
        foreach (var rating in response.PromptFeedback.SafetyRatings)
        {
            if (rating.Probability == "HIGH" || rating.Probability == "MEDIUM")
            {
                Console.WriteLine($"  {rating.Category}: {rating.Probability}");
            }
        }
    }

    return "Your request could not be processed due to safety policies.";
}
```

---

### 9.4 OpenAI Content Filtering

**What it is:** OpenAI automatically filters harmful content (less configurable than Gemini).

#### Refusals

```csharp
public class OpenAiChatMessageBaseDto
{
    // Refusal message if model declined to respond
    public string? Refusal { get; set; }
}
```

**Example:**
```csharp
var response = await CallOpenAiApi(request);
var message = response.Choices[0].Message;

if (!string.IsNullOrEmpty(message.Refusal))
{
    Console.WriteLine("Model refused to respond:");
    Console.WriteLine(message.Refusal);

    // Example refusal:
    // "I can't assist with that request as it involves creating harmful content."

    return null;
}

// Normal response
return message.Content;
```

---

### 9.5 Complete Safety Handling Example

```csharp
public class SafeChatHandler
{
    public async Task<(bool Success, string? Response, string? ErrorReason)>
        HandleChatRequestSafelyAsync(string userMessage)
    {
        // Gemini request with safety settings
        var request = new GeminiChatRequestDto
        {
            Contents = new List<GeminiChatContentDto>
            {
                new()
                {
                    Role = "user",
                    Parts = new List<GeminiChatPartDto>
                    {
                        new() { Text = userMessage }
                    }
                }
            },
            SafetySettings = new List<GeminiChatSafetySettingDto>
            {
                new()
                {
                    Category = "HARM_CATEGORY_HATE_SPEECH",
                    Threshold = "BLOCK_MEDIUM_AND_ABOVE"
                },
                new()
                {
                    Category = "HARM_CATEGORY_DANGEROUS_CONTENT",
                    Threshold = "BLOCK_LOW_AND_ABOVE"
                },
                new()
                {
                    Category = "HARM_CATEGORY_HARASSMENT",
                    Threshold = "BLOCK_MEDIUM_AND_ABOVE"
                },
                new()
                {
                    Category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                    Threshold = "BLOCK_MEDIUM_AND_ABOVE"
                }
            }
        };

        var response = await CallGeminiApi(request);

        // Check 1: Prompt feedback (prompt itself blocked)
        if (response.PromptFeedback?.BlockReason != null)
        {
            var reason = $"Prompt blocked: {response.PromptFeedback.BlockReason}";

            // Log which categories triggered
            var triggeredCategories = response.PromptFeedback.SafetyRatings
                ?.Where(r => r.Probability == "HIGH" || r.Probability == "MEDIUM")
                .Select(r => r.Category)
                .ToList();

            if (triggeredCategories?.Count > 0)
            {
                reason += $" (Categories: {string.Join(", ", triggeredCategories)})";
            }

            return (false, null, reason);
        }

        // Check 2: Response safety (response blocked)
        var candidate = response.Candidates?[0];
        if (candidate?.FinishReason == "SAFETY")
        {
            var blockedCategories = candidate.SafetyRatings
                ?.Where(r => r.Blocked == true)
                .Select(r => r.Category)
                .ToList();

            var reason = $"Response blocked due to safety concerns: {string.Join(", ", blockedCategories)}";

            return (false, null, reason);
        }

        // Check 3: Safety ratings (not blocked but concerning)
        var highRiskRatings = candidate?.SafetyRatings
            ?.Where(r => r.Probability == "HIGH")
            .ToList();

        if (highRiskRatings?.Count > 0)
        {
            // Log warning but allow response
            Console.WriteLine("⚠️ High-risk safety ratings detected:");
            foreach (var rating in highRiskRatings)
            {
                Console.WriteLine($"  {rating.Category}: {rating.Probability}");
            }
        }

        // Success
        var responseText = candidate?.Content?.Parts?[0]?.Text;
        return (true, responseText, null);
    }
}

// Usage
var handler = new SafeChatHandler();
var (success, response, errorReason) = await handler.HandleChatRequestSafelyAsync(userInput);

if (success)
{
    Console.WriteLine(response);
}
else
{
    Console.WriteLine($"❌ Request blocked: {errorReason}");
    Console.WriteLine("Please rephrase your request and try again.");
}
```

---

### 9.6 Custom Content Moderation

**Beyond built-in filters:** Add your own moderation layer.

```csharp
public class ContentModerator
{
    private readonly HashSet<string> _blockedKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "badword1", "badword2", "badword3"
    };

    private readonly List<Regex> _blockedPatterns = new()
    {
        new Regex(@"\b\d{3}-\d{2}-\d{4}\b"),  // SSN pattern
        new Regex(@"\b\d{16}\b"),              // Credit card pattern
    };

    public (bool IsAllowed, string? Reason) ModerateInput(string input)
    {
        // Check blocked keywords
        foreach (var keyword in _blockedKeywords)
        {
            if (input.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"Blocked keyword detected: {keyword}");
            }
        }

        // Check blocked patterns
        foreach (var pattern in _blockedPatterns)
        {
            if (pattern.IsMatch(input))
            {
                return (false, "Potentially sensitive information detected");
            }
        }

        return (true, null);
    }

    public (bool IsAllowed, string? Reason) ModerateOutput(string output)
    {
        // Check for personally identifiable information (PII)
        if (ContainsPII(output))
        {
            return (false, "Response contains PII");
        }

        // Check for company-confidential patterns
        if (output.Contains("CONFIDENTIAL", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Response contains confidential information");
        }

        return (true, null);
    }

    private bool ContainsPII(string text)
    {
        // Email addresses
        if (Regex.IsMatch(text, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b"))
            return true;

        // Phone numbers
        if (Regex.IsMatch(text, @"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b"))
            return true;

        return false;
    }
}

// Usage
var moderator = new ContentModerator();

// Moderate input
var (inputAllowed, inputReason) = moderator.ModerateInput(userMessage);
if (!inputAllowed)
{
    return $"Input blocked: {inputReason}";
}

// Call AI
var response = await CallAiApi(userMessage);

// Moderate output
var (outputAllowed, outputReason) = moderator.ModerateOutput(response);
if (!outputAllowed)
{
    return $"Response blocked: {outputReason}";
}

return response;
```

---

### Safety & Moderation Best Practices

1. **Always Configure Safety Settings:**
   ```csharp
   // Set appropriate thresholds for your use case
   SafetySettings = new List<GeminiChatSafetySettingDto>
   {
       new() { Category = "HARM_CATEGORY_HATE_SPEECH", Threshold = "BLOCK_MEDIUM_AND_ABOVE" },
       new() { Category = "HARM_CATEGORY_DANGEROUS_CONTENT", Threshold = "BLOCK_LOW_AND_ABOVE" }
   }
   ```

2. **Check Multiple Safety Signals:**
   ```csharp
   // 1. Prompt feedback
   if (response.PromptFeedback?.BlockReason != null) { ... }

   // 2. Response finish reason
   if (candidate.FinishReason == "SAFETY") { ... }

   // 3. Safety ratings
   if (candidate.SafetyRatings.Any(r => r.Probability == "HIGH")) { ... }

   // 4. Refusals (OpenAI)
   if (!string.IsNullOrEmpty(message.Refusal)) { ... }
   ```

3. **Log Safety Incidents:**
   ```csharp
   if (candidate.FinishReason == "SAFETY")
   {
       _logger.LogWarning("Safety block triggered for user {UserId}: {Categories}",
           userId, string.Join(", ", blockedCategories));
   }
   ```

4. **Provide User-Friendly Messages:**
   ```csharp
   // ❌ Bad: Technical error
   return "Error: HARM_CATEGORY_DANGEROUS_CONTENT BLOCK_MEDIUM_AND_ABOVE";

   // ✅ Good: Helpful message
   return "I can't provide information on that topic. Please ask something else.";
   ```

5. **Add Custom Moderation Layers:**
   ```csharp
   // Company-specific rules
   var moderator = new CustomModerator();
   if (!moderator.IsAllowed(userInput))
   {
       return "This request violates company policy.";
   }
   ```

---

### Safety & Moderation Summary

| Concept | Purpose | Example |
|---------|---------|---------|
| **Safety Settings** | Configure filtering thresholds | BLOCK_MEDIUM_AND_ABOVE |
| **Safety Categories** | Types of harmful content | Hate speech, violence, harassment |
| **Safety Ratings** | Probability of harmful content | NEGLIGIBLE, LOW, MEDIUM, HIGH |
| **Prompt Feedback** | Prompt blocked before processing | BlockReason: "SAFETY" |
| **Finish Reason** | Response blocked after generation | "SAFETY" |
| **Refusals** | Model declines to respond (OpenAI) | "I can't assist with that" |

**When to Use Strict Safety:**
- ✅ Public-facing chatbots
- ✅ Child-safe applications
- ✅ Regulated industries (healthcare, education)
- ✅ Brand protection
- ❌ Internal tools (less strict may be okay)
- ❌ Research/analysis (may need unfiltered)

---

## 10. Multi-Modal AI

**Multi-modal AI** processes **multiple types of content**: text, images, audio, video.

### 10.1 Supported Modalities

**OpenAI:**
- ✅ **Text:** All models
- ✅ **Images:** GPT-4 Vision models (gpt-4-vision, gpt-4o)
- ✅ **Audio:** GPT-4o audio models

**Gemini:**
- ✅ **Text:** All models
- ✅ **Images:** Gemini 1.5 Pro/Flash
- ✅ **Video:** Gemini 1.5 Pro
- ✅ **Audio:** Gemini 1.5 Pro (experimental)

---

### 10.2 Image Input

#### OpenAI Image Messages

```csharp
var message = new OpenAiChatMessageUserDto
{
    Role = "user",
    Content = new OpenAiChatMessageContentPartsDto
    {
        ListContent = new List<OpenAiChatMessageContentPartDto>
        {
            new OpenAiChatMessageContentPartTextDto
            {
                Type = "text",
                Text = "What's in this image?"
            },
            new OpenAiChatMessageContentPartImageDto
            {
                Type = "image_url",
                ImageUrl = new OpenAiChatImageUrlDto
                {
                    Url = "https://example.com/image.jpg",
                    Detail = "high"  // "low", "high", "auto"
                }
            }
        }
    }
};
```

**Image Detail Levels:**
- **"low":** 512x512 (cheaper, faster, less detail)
- **"high":** Full resolution (more expensive, slower, more detail)
- **"auto":** Model decides based on image size

---

#### Gemini Image/Video Content

```csharp
public class GeminiChatPartDto
{
    // Text
    public string? Text { get; set; }

    // Inline data (base64)
    public GeminiChatInlineDataDto? InlineData { get; set; }

    // File URI (Google Cloud Storage)
    public GeminiChatFileDataDto? FileData { get; set; }
}

public class GeminiChatInlineDataDto
{
    public string? MimeType { get; set; }  // "image/jpeg", "video/mp4", "audio/wav"
    public string? Data { get; set; }       // Base64-encoded
}

public class GeminiChatFileDataDto
{
    public string? MimeType { get; set; }
    public string? FileUri { get; set; }    // gs://bucket/path
}
```

**Example: Image Analysis**
```csharp
var imageBytes = File.ReadAllBytes("photo.jpg");
var base64Image = Convert.ToBase64String(imageBytes);

var request = new GeminiChatRequestDto
{
    Contents = new List<GeminiChatContentDto>
    {
        new()
        {
            Role = "user",
            Parts = new List<GeminiChatPartDto>
            {
                new() { Text = "Describe this image in detail." },
                new()
                {
                    InlineData = new GeminiChatInlineDataDto
                    {
                        MimeType = "image/jpeg",
                        Data = base64Image
                    }
                }
            }
        }
    }
};

var response = await CallGeminiApi(request);
Console.WriteLine(response.Candidates[0].Content.Parts[0].Text);
// Output: "The image shows a sunset over a mountain range..."
```

---

### 10.3 Audio

#### OpenAI Audio (GPT-4o Audio)

```csharp
public class OpenAiChatAudioDto
{
    // Voice: "alloy", "echo", "fable", "onyx", "nova", "shimmer"
    public string? Voice { get; set; }

    // Format: "wav", "mp3", "flac", "opus", "pcm16"
    public string? Format { get; set; }
}

// Request audio output
var request = new OpenAiChatRequestDto
{
    Model = "gpt-4o-audio",
    Modalities = new List<string> { "text", "audio" },
    Audio = new OpenAiChatAudioDto
    {
        Voice = "alloy",
        Format = "mp3"
    },
    Messages = messages
};

var response = await CallOpenAiApi(request);

// Response includes audio
var audioContent = response.Choices[0].Message.Audio;
if (audioContent != null)
{
    var audioBytes = Convert.FromBase64String(audioContent.Data);
    File.WriteAllBytes("response.mp3", audioBytes);

    // Also includes transcript
    Console.WriteLine("Transcript: " + audioContent.Transcript);
}
```

---

### 10.4 Video Analysis (Gemini)

```csharp
public async Task AnalyzeVideoAsync(string videoPath)
{
    // Upload video to Google Cloud Storage first
    var videoUri = await UploadToGCS(videoPath);

    var request = new GeminiChatRequestDto
    {
        Contents = new List<GeminiChatContentDto>
        {
            new()
            {
                Role = "user",
                Parts = new List<GeminiChatPartDto>
                {
                    new()
                    {
                        FileData = new GeminiChatFileDataDto
                        {
                            MimeType = "video/mp4",
                            FileUri = videoUri
                        }
                    },
                    new()
                    {
                        Text = "Summarize what happens in this video."
                    }
                }
            }
        }
    };

    var response = await CallGeminiApi(request);
    Console.WriteLine(response.Candidates[0].Content.Parts[0].Text);
    // Output: "The video shows a person cooking pasta..."
}
```

---

### 10.5 Token Counting for Modalities

**Different modalities use different token counts.**

#### OpenAI Token Details

```csharp
public class OpenAiChatCompletionTokensDetailsDto
{
    // Audio tokens (input)
    public int? AudioTokens { get; set; }

    // Reasoning tokens (o1 models)
    public int? ReasoningTokens { get; set; }

    // Cached tokens (prompt caching)
    public int? CachedTokens { get; set; }
}

// Check usage
var usage = response.Usage;
Console.WriteLine($"Total tokens: {usage.TotalTokens}");
Console.WriteLine($"Audio tokens: {usage.CompletionTokensDetails.AudioTokens}");
```

---

#### Gemini Usage Metadata

```csharp
public class GeminiChatUsageMetadataDto
{
    // Prompt tokens
    public int? PromptTokenCount { get; set; }

    // Response tokens
    public int? CandidatesTokenCount { get; set; }

    // Total
    public int? TotalTokenCount { get; set; }

    // Cached tokens
    public int? CachedContentTokenCount { get; set; }
}
```

**Image Token Estimation:**
- Small image (< 512px): ~85 tokens
- Medium image (512-1024px): ~170 tokens
- Large image (> 1024px): ~340 tokens

**Video Token Estimation:**
- ~1 token per frame
- 30 FPS video: ~1800 tokens/minute

---

### Multi-Modal Best Practices

1. **Optimize Image Detail:**
   ```csharp
   // ✅ Good: Use "low" for simple tasks
   ImageUrl = new() { Url = imageUrl, Detail = "low" }

   // ❌ Bad: Always using "high" (expensive!)
   ```

2. **Consider Token Costs:**
   ```csharp
   // Images/video use LOTS of tokens
   // 1-minute video = ~1800 tokens!
   // Be mindful of costs
   ```

3. **Use Appropriate Formats:**
   ```csharp
   // Images: JPEG for photos, PNG for graphics
   // Audio: MP3 for speech, WAV for high quality
   // Video: MP4 (H.264) for compatibility
   ```

---

## 11. Advanced Features

### 11.1 Reasoning Effort (OpenAI o1 Models)

**What it is:** Control how much "thinking" the model does.

```csharp
public class OpenAiChatRequestDto
{
    // "low", "medium", "high"
    public string? ReasoningEffort { get; set; }
}
```

**Example:**
```csharp
var request = new OpenAiChatRequestDto
{
    Model = "o1-preview",
    Messages = messages,
    ReasoningEffort = "high"  // More thinking = better reasoning, slower, more tokens
};
```

---

### 11.2 Thinking Configuration (Gemini)

**What it is:** Control internal "thinking" process.

```csharp
public class GeminiChatThinkingConfigDto
{
    // Include thoughts in response
    public bool? IncludeThoughts { get; set; }

    // Budget for thinking tokens
    public int? ThinkingBudget { get; set; }

    // Thinking level: "low", "medium", "high"
    public string? ThinkingLevel { get; set; }
}
```

---

### 11.3 Prompt Caching

**What it is:** Cache long prompts to save costs/time on repeated requests.

**OpenAI:**
```csharp
// Automatic prompt caching (GPT-4o, GPT-4o-mini)
// Prompts > 1024 tokens cached automatically for 5-10 minutes
```

**Gemini:**
```csharp
public class GeminiChatCachedContentDto
{
    public string? Name { get; set; }  // Cache identifier
    public int? TtlSeconds { get; set; }  // Time to live
}
```

---

### Advanced Features Summary

These advanced concepts complete the comprehensive guide to AI/LLM DTOs in your Nekote library!

---

## Conclusion

### What We've Covered

This comprehensive guide has explored **ALL major AI/LLM concepts** represented in your Nekote DTOs:

1. **Sampling Parameters** - Control randomness and creativity (Temperature, Top-P, Top-K, Frequency/Presence Penalty, Seed)
2. **Tokens & Tokenization** - Understanding AI's language units, costs, and limits
3. **Log Probabilities** - Confidence scoring, hallucination detection, fact verification
4. **Function Calling** - Connecting AI to external APIs and tools
5. **Response Format** - Structured output, JSON schemas, stop sequences
6. **Embeddings** - Semantic search, classification, clustering, RAG systems
7. **Streaming** - Real-time incremental responses for better UX
8. **Grounding & Citations** - Connecting responses to sources, reducing hallucinations
9. **Safety & Moderation** - Content filtering, safety ratings, responsible AI
10. **Multi-Modal AI** - Working with images, audio, and video
11. **Advanced Features** - Reasoning effort, thinking, caching, code execution

### Key Takeaways

**For Developers:**
- Your DTOs cover **two major AI platforms** (OpenAI and Gemini)
- Each DTO property serves a **specific purpose** in AI behavior control
- Understanding these concepts enables **production-ready AI applications**

**Best Practices Reminder:**
1. ✅ **Always** validate user input and AI output
2. ✅ **Always** implement error handling and retry logic
3. ✅ **Always** monitor token usage and costs
4. ✅ **Always** configure safety settings appropriately
5. ✅ **Always** check finish reasons and grounding metadata
6. ✅ **Consider** prompt caching for repeated patterns
7. ✅ **Consider** streaming for better user experience
8. ✅ **Consider** function calling for real-time data needs

### Cost Optimization Tips

1. **Use smaller models** when appropriate (GPT-3.5 Turbo, Gemini Flash)
2. **Set MaxCompletionTokens** to prevent runaway generation
3. **Cache embeddings** - don't re-embed the same text
4. **Use Base64 encoding** for embedding storage (50% smaller)
5. **Batch API calls** when possible
6. **Use prompt caching** for repeated long prompts
7. **Monitor usage** with token counting DTOs

### Building Production AI Systems

**Checklist:**
- [ ] Input validation and sanitization
- [ ] Output validation and moderation
- [ ] Error handling (rate limits, timeouts, API errors)
- [ ] Retry logic with exponential backoff
- [ ] Logging and monitoring
- [ ] Cost tracking and budgets
- [ ] Safety settings configured
- [ ] Grounding/RAG for factual accuracy
- [ ] User feedback collection
- [ ] Performance optimization (streaming, caching)

### Next Steps

Now that you understand these concepts, you can:

1. **Build conversational AI** - Chatbots, assistants, customer support
2. **Create search systems** - Semantic search, RAG, document Q&A
3. **Develop AI agents** - Function-calling agents that take actions
4. **Implement content generation** - Articles, summaries, creative writing
5. **Build classification systems** - Sentiment analysis, categorization
6. **Create multimodal apps** - Image analysis, video understanding
7. **Design safety-first AI** - Content moderation, brand protection

### Resources

**Official Documentation:**
- OpenAI API: https://platform.openai.com/docs
- Gemini API: https://ai.google.dev/docs
- Azure OpenAI: https://learn.microsoft.com/azure/ai-services/openai/

**Your Nekote DTOs:**
- 166 DTO files covering both APIs
- Complete type safety for all properties
- Ready for production use

### Final Note

AI/LLM technology evolves rapidly. These concepts represent the **current state-of-the-art** as of 2024, but new features and capabilities emerge regularly. Your DTO architecture is well-designed to accommodate future additions.

**Remember:** AI is a tool. The real value comes from how you apply these concepts to solve **real problems** for **real users**.

Good luck building amazing AI applications with Nekote! 🚀

---

**Document Stats:**
- **Sections:** 11 major topics
- **Lines:** ~6,000+
- **Concepts:** 50+ explained in detail
- **Code Examples:** 100+ real-world C# implementations
- **Coverage:** OpenAI + Gemini APIs comprehensively documented

---


