using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ユーザーの位置情報パラメータ DTO。
    /// </summary>
    public class OpenAiChatUserLocationDto
    {
        /// <summary>
        /// おおよその位置情報パラメータ。
        /// </summary>
        [JsonPropertyName("approximate")]
        public OpenAiChatApproximateLocationDto? Approximate { get; set; }

        /// <summary>
        /// 位置近似のタイプ (常に "approximate")。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
