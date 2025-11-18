using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 予測出力の構成 DTO。
    /// </summary>
    internal class OpenAiChatPredictionDto
    {
        /// <summary>
        /// 予測のタイプ。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// 予測コンテンツ。
        /// </summary>
        [JsonPropertyName("content")]
        public object? Content { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
