using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI Chat API のメッセージ基底クラス。
    /// </summary>
    [JsonConverter(typeof(Converters.OpenAiChatMessageConverter))]
    public abstract class OpenAiChatMessageBaseDto
    {
        /// <summary>
        /// メッセージのロール（"system", "user", "assistant", "developer", "tool", "function"）。
        /// </summary>
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
