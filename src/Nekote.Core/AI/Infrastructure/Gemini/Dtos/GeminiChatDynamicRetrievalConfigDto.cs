using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 動的検索の設定。
    /// </summary>
    public class GeminiChatDynamicRetrievalConfigDto
    {
        /// <summary>
        /// 動的検索のモード。
        /// </summary>
        [JsonPropertyName("mode")]
        public string? Mode { get; set; }

        /// <summary>
        /// 検索を実行する信頼度のしきい値。
        /// </summary>
        [JsonPropertyName("dynamicThreshold")]
        public double? DynamicThreshold { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
