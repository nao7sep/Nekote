using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Google マップのグラウンディング チャンク DTO。
    /// </summary>
    public class GeminiChatMapsDto
    {
        /// <summary>
        /// 場所の URI 参照。
        /// </summary>
        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        /// <summary>
        /// 場所のタイトル。
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// 場所の回答のテキストによる説明。
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// この場所の ID（places/{placeId} 形式）。
        /// </summary>
        [JsonPropertyName("placeId")]
        public string? PlaceId { get; set; }

        /// <summary>
        /// Google マップで特定の場所の機能に関する回答を提供するソース。
        /// </summary>
        [JsonPropertyName("placeAnswerSources")]
        public GeminiChatPlaceAnswerSourcesDto? PlaceAnswerSources { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
