using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 生成構成 DTO。
    /// </summary>
    internal class GeminiChatGenerationConfigDto
    {
        /// <summary>
        /// 停止シーケンスのリスト。
        /// </summary>
        [JsonPropertyName("stopSequences")]
        public List<string>? StopSequences { get; set; }

        /// <summary>
        /// レスポンスの MIME タイプ。
        /// </summary>
        [JsonPropertyName("responseMimeType")]
        public string? ResponseMimeType { get; set; }

        /// <summary>
        /// レスポンスのスキーマ。
        /// </summary>
        [JsonPropertyName("responseSchema")]
        public GeminiChatSchemaDto? ResponseSchema { get; set; }

        /// <summary>
        /// JSON スキーマ形式のレスポンススキーマ。
        /// </summary>
        [JsonPropertyName("responseJsonSchema")]
        public JsonElement? ResponseJsonSchema { get; set; }

        /// <summary>
        /// レスポンスのモダリティのリスト。
        /// </summary>
        [JsonPropertyName("responseModalities")]
        public List<string>? ResponseModalities { get; set; }

        /// <summary>
        /// 生成する候補の数。
        /// </summary>
        [JsonPropertyName("candidateCount")]
        public int? CandidateCount { get; set; }

        /// <summary>
        /// 最大出力トークン数。
        /// </summary>
        [JsonPropertyName("maxOutputTokens")]
        public int? MaxOutputTokens { get; set; }

        /// <summary>
        /// 温度。
        /// </summary>
        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        /// <summary>
        /// Top-P パラメータ。
        /// </summary>
        [JsonPropertyName("topP")]
        public double? TopP { get; set; }

        /// <summary>
        /// Top-K パラメータ。
        /// </summary>
        [JsonPropertyName("topK")]
        public int? TopK { get; set; }

        /// <summary>
        /// シード値。
        /// </summary>
        [JsonPropertyName("seed")]
        public int? Seed { get; set; }

        /// <summary>
        /// プレゼンスペナルティ。
        /// </summary>
        [JsonPropertyName("presencePenalty")]
        public double? PresencePenalty { get; set; }

        /// <summary>
        /// 頻度ペナルティ。
        /// </summary>
        [JsonPropertyName("frequencyPenalty")]
        public double? FrequencyPenalty { get; set; }

        /// <summary>
        /// logprobs を返すかどうか。
        /// </summary>
        [JsonPropertyName("responseLogprobs")]
        public bool? ResponseLogprobs { get; set; }

        /// <summary>
        /// 返す logprobs の数。
        /// </summary>
        [JsonPropertyName("logprobs")]
        public int? Logprobs { get; set; }

        /// <summary>
        /// 強化された市民向け回答を有効にするかどうか。
        /// </summary>
        [JsonPropertyName("enableEnhancedCivicAnswers")]
        public bool? EnableEnhancedCivicAnswers { get; set; }

        /// <summary>
        /// 音声生成の構成。
        /// </summary>
        [JsonPropertyName("speechConfig")]
        public GeminiChatSpeechConfigDto? SpeechConfig { get; set; }

        /// <summary>
        /// 思考機能の構成。
        /// </summary>
        [JsonPropertyName("thinkingConfig")]
        public GeminiChatThinkingConfigDto? ThinkingConfig { get; set; }

        /// <summary>
        /// 画像生成の構成。
        /// </summary>
        [JsonPropertyName("imageConfig")]
        public GeminiChatImageConfigDto? ImageConfig { get; set; }

        /// <summary>
        /// メディアの解像度。
        /// </summary>
        [JsonPropertyName("mediaResolution")]
        public string? MediaResolution { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
