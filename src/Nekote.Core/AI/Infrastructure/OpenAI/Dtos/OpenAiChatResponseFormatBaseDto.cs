using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI Chat API のレスポンスフォーマット基底クラス。
    /// </summary>
    [JsonConverter(typeof(Converters.OpenAiChatResponseFormatConverter))]
    public abstract class OpenAiChatResponseFormatBaseDto
    {
        /// <summary>
        /// レスポンスフォーマットのタイプ（"text", "json_schema", "json_object"）。
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
