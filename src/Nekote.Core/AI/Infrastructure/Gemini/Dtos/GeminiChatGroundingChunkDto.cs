using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// グラウンディング チャンク DTO。
    /// </summary>
    public class GeminiChatGroundingChunkDto
    {
        /// <summary>
        /// ウェブからのグラウンディング チャンク。
        /// </summary>
        [JsonPropertyName("web")]
        public GeminiChatWebDto? Web { get; set; }

        /// <summary>
        /// ファイル検索ツールで取得されたコンテキストのグラウンディング チャンク。
        /// </summary>
        [JsonPropertyName("retrievedContext")]
        public GeminiChatRetrievedContextDto? RetrievedContext { get; set; }

        /// <summary>
        /// Google マップのグラウンディング チャンク。
        /// </summary>
        [JsonPropertyName("maps")]
        public GeminiChatMapsDto? Maps { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
