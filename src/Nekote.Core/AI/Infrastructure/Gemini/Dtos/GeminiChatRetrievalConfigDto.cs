using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 取得構成。
    /// </summary>
    public class GeminiChatRetrievalConfigDto
    {
        /// <summary>
        /// ユーザーの現在地。
        /// </summary>
        [JsonPropertyName("latLng")]
        public GeminiChatLatLngDto? LatLng { get; set; }

        /// <summary>
        /// ユーザーの言語コード。
        /// </summary>
        [JsonPropertyName("languageCode")]
        public string? LanguageCode { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
