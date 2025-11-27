using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Converters;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "tools" 配列の要素のポリモーフィックな値を表現するための抽象基底 DTO。
    /// </summary>
    [JsonConverter(typeof(OpenAiChatToolConverter))]
    public abstract class OpenAiChatToolBaseDto
    {
        /// <summary>
        /// ツールの種類（"function", "custom"）。
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
