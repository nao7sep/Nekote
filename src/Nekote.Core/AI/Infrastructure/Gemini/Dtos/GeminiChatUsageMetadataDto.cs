using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Chat API の使用量メタデータ DTO。
    /// ストリーミングおよび非ストリーミングレスポンスで共有される。
    /// </summary>
    public class GeminiChatUsageMetadataDto
    {
        /// <summary>
        /// プロンプト内のトークン数。キャッシュに保存されたコンテンツのトークン数も含まれる。
        /// </summary>
        [JsonPropertyName("promptTokenCount")]
        public int? PromptTokenCount { get; set; }

        /// <summary>
        /// プロンプトのキャッシュに保存された部分のトークン数。
        /// </summary>
        [JsonPropertyName("cachedContentTokenCount")]
        public int? CachedContentTokenCount { get; set; }

        /// <summary>
        /// 生成されたレスポンス候補全体のトークンの合計数。
        /// </summary>
        [JsonPropertyName("candidatesTokenCount")]
        public int? CandidatesTokenCount { get; set; }

        /// <summary>
        /// ツール使用プロンプト内のトークン数（出力専用）。
        /// </summary>
        [JsonPropertyName("toolUsePromptTokenCount")]
        public int? ToolUsePromptTokenCount { get; set; }

        /// <summary>
        /// 思考モデルの思考のトークン数（出力専用）。
        /// </summary>
        [JsonPropertyName("thoughtsTokenCount")]
        public int? ThoughtsTokenCount { get; set; }

        /// <summary>
        /// 生成リクエストのトークンの合計数（プロンプト + レスポンス候補）。
        /// </summary>
        [JsonPropertyName("totalTokenCount")]
        public int? TotalTokenCount { get; set; }

        /// <summary>
        /// リクエスト入力で処理されたモダリティのリスト（出力専用）。
        /// </summary>
        [JsonPropertyName("promptTokensDetails")]
        public List<GeminiChatModalityTokenCountDto>? PromptTokensDetails { get; set; }

        /// <summary>
        /// リクエスト入力内のキャッシュに保存されたコンテンツのモダリティのリスト（出力専用）。
        /// </summary>
        [JsonPropertyName("cacheTokensDetails")]
        public List<GeminiChatModalityTokenCountDto>? CacheTokensDetails { get; set; }

        /// <summary>
        /// レスポンスで返されたモダリティのリスト（出力専用）。
        /// </summary>
        [JsonPropertyName("candidatesTokensDetails")]
        public List<GeminiChatModalityTokenCountDto>? CandidatesTokensDetails { get; set; }

        /// <summary>
        /// ツール使用リクエストの入力に対して処理されたモダリティのリスト（出力専用）。
        /// </summary>
        [JsonPropertyName("toolUsePromptTokensDetails")]
        public List<GeminiChatModalityTokenCountDto>? ToolUsePromptTokensDetails { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
