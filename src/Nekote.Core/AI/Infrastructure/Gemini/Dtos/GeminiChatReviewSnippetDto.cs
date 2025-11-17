using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Google マップの特定の場所の機能に関する質問に回答するユーザー レビューのスニペット DTO。
    /// </summary>
    internal class GeminiChatReviewSnippetDto
    {
        /// <summary>
        /// レビュー スニペットの ID。
        /// </summary>
        [JsonPropertyName("reviewId")]
        public string? ReviewId { get; set; }

        /// <summary>
        /// Google マップのユーザー レビューに対応するリンク。
        /// </summary>
        [JsonPropertyName("googleMapsUri")]
        public string? GoogleMapsUri { get; set; }

        /// <summary>
        /// レビューのタイトル。
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
