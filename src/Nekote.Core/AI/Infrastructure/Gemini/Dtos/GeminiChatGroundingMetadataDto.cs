using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 根拠情報のメタデータ。
    /// </summary>
    public class GeminiChatGroundingMetadataDto
    {
        /// <summary>
        /// 引用元の情報チャンクのリスト。
        /// </summary>
        [JsonPropertyName("groundingChunks")]
        public List<GeminiChatGroundingChunkDto>? GroundingChunks { get; set; }

        /// <summary>
        /// 回答と引用元の対応付け情報のリスト。
        /// </summary>
        [JsonPropertyName("groundingSupports")]
        public List<GeminiChatGroundingSupportDto>? GroundingSupports { get; set; }

        /// <summary>
        /// ウェブ検索のフォローアップのためのウェブ検索クエリ。
        /// </summary>
        [JsonPropertyName("webSearchQueries")]
        public List<string>? WebSearchQueries { get; set; }

        /// <summary>
        /// ウェブ検索のフォローアップ用の Google 検索エントリ。
        /// </summary>
        [JsonPropertyName("searchEntryPoint")]
        public GeminiChatSearchEntryPointDto? SearchEntryPoint { get; set; }

        /// <summary>
        /// 情報取得に関連するメタデータ。
        /// </summary>
        [JsonPropertyName("retrievalMetadata")]
        public GeminiChatRetrievalMetadataDto? RetrievalMetadata { get; set; }

        /// <summary>
        /// Google マップ ウィジェット コンテキスト トークンのリソース名。
        /// </summary>
        [JsonPropertyName("googleMapsWidgetContextToken")]
        public string? GoogleMapsWidgetContextToken { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
