using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "content" 配列内の "part" オブジェクト基底クラス。
    /// </summary>
    [JsonConverter(typeof(Converters.OpenAiChatMessageContentPartConverter))]
    public abstract class OpenAiChatMessageContentPartBaseDto
    {
        /// <summary>
        /// パーツの種類 ("text", "image_url", "input_audio", "file", "refusal")。
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
