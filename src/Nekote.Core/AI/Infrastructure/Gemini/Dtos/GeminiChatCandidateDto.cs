using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Chat API の候補 DTO。
    /// </summary>
    public class GeminiChatCandidateDto
    {
        /// <summary>
        /// モデルから返された生成コンテンツ（出力専用）。
        /// </summary>
        [JsonPropertyName("content")]
        public GeminiChatContentDto? Content { get; set; }

        /// <summary>
        /// モデルがトークンの生成を停止した理由（出力専用）。
        /// </summary>
        [JsonPropertyName("finishReason")]
        public string? FinishReason { get; set; }

        /// <summary>
        /// レスポンス候補の安全性に関する評価のリスト。
        /// </summary>
        [JsonPropertyName("safetyRatings")]
        public List<GeminiChatSafetyRatingDto>? SafetyRatings { get; set; }

        /// <summary>
        /// モデル生成候補の引用情報（出力専用）。
        /// </summary>
        [JsonPropertyName("citationMetadata")]
        public GeminiChatCitationMetadataDto? CitationMetadata { get; set; }

        /// <summary>
        /// この候補のトークン数（出力専用）。
        /// </summary>
        [JsonPropertyName("tokenCount")]
        public int? TokenCount { get; set; }

        /// <summary>
        /// 根拠のある回答に貢献したソースの帰属情報（出力専用）。
        /// </summary>
        [JsonPropertyName("groundingAttributions")]
        public List<GeminiChatGroundingAttributionDto>? GroundingAttributions { get; set; }

        /// <summary>
        /// 根拠情報のメタデータ（出力専用）。
        /// </summary>
        [JsonPropertyName("groundingMetadata")]
        public GeminiChatGroundingMetadataDto? GroundingMetadata { get; set; }

        /// <summary>
        /// 候補の平均対数確率（出力専用）。
        /// </summary>
        [JsonPropertyName("avgLogprobs")]
        public double? AvgLogprobs { get; set; }

        /// <summary>
        /// レスポンストークンと上位トークンの対数確率（出力専用）。
        /// </summary>
        [JsonPropertyName("logprobsResult")]
        public GeminiChatLogprobsResultDto? LogprobsResult { get; set; }

        /// <summary>
        /// URL コンテキスト取得ツールに関連するメタデータ（出力専用）。
        /// </summary>
        [JsonPropertyName("urlContextMetadata")]
        public GeminiChatUrlContextMetadataDto? UrlContextMetadata { get; set; }

        /// <summary>
        /// レスポンス候補のリスト内の候補のインデックス（出力専用）。
        /// </summary>
        [JsonPropertyName("index")]
        public int? Index { get; set; }

        /// <summary>
        /// モデルがトークンの生成を停止した理由の詳細（出力専用）。
        /// </summary>
        [JsonPropertyName("finishMessage")]
        public string? FinishMessage { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
