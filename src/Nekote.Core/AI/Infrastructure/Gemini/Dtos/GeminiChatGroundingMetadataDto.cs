using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// グラウンディングが有効になっている場合にクライアントに返されるメタデータ DTO。
    /// </summary>
    internal class GeminiChatGroundingMetadataDto
    {
        /// <summary>
        /// 指定されたグラウンディング ソースから取得されたサポート参照のリスト。
        /// </summary>
        [JsonPropertyName("groundingChunks")]
        public List<GeminiChatGroundingChunkDto>? GroundingChunks { get; set; }

        /// <summary>
        /// グラウンディング サポートのリスト。
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
        /// グラウンディング フローでの取得に関連するメタデータ。
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
