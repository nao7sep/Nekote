using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// URL コンテキスト取得ツールに関連するメタデータ DTO。
    /// </summary>
    internal class GeminiChatUrlContextMetadataDto
    {
        /// <summary>
        /// URL コンテキストのリスト。
        /// </summary>
        [JsonPropertyName("urlMetadata")]
        public List<GeminiChatUrlMetadataDto>? UrlMetadata { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
