using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 単一の URL の取得のコンテキスト DTO。
    /// </summary>
    public class GeminiChatUrlMetadataDto
    {
        /// <summary>
        /// ツールによって取得された URL。
        /// </summary>
        [JsonPropertyName("retrievedUrl")]
        public string? RetrievedUrl { get; set; }

        /// <summary>
        /// URL 取得のステータス。
        /// </summary>
        [JsonPropertyName("urlRetrievalStatus")]
        public string? UrlRetrievalStatus { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
