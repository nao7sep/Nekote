using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 動的取得構成。
    /// </summary>
    public class GeminiChatDynamicRetrievalConfigDto
    {
        /// <summary>
        /// 動的取得で使用される予測子のモード。
        /// </summary>
        [JsonPropertyName("mode")]
        public string? Mode { get; set; }

        /// <summary>
        /// 動的取得で使用されるしきい値。
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
