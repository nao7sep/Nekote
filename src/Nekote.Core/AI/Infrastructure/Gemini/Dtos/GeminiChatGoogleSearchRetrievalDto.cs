using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Google 検索取得。
    /// </summary>
    public class GeminiChatGoogleSearchRetrievalDto
    {
        /// <summary>
        /// 動的取得構成。
        /// </summary>
        [JsonPropertyName("dynamicRetrievalConfig")]
        public GeminiChatDynamicRetrievalConfigDto? DynamicRetrievalConfig { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
