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
        /// メッセージのロール ("system", "user", "assistant", "developer", "tool", "function")。
        /// </summary>
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        /// <summary>
        /// 名前フィールド。意味はロールによって異なる。
        /// function: 呼び出す関数の名前 (必須)。
        /// その他: 参加者の名前 (省略可能)。同じ役割の参加者を区別するために使用される。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
