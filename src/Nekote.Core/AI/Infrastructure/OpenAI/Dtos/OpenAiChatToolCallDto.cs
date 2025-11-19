using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ツール呼び出し情報。
    /// </summary>
    public class OpenAiChatToolCallDto
    {
        /// <summary>
        /// ツール呼び出しの一意識別子。
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// ツールの種類 (通常は "function")。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

    /// <summary>
    /// 呼び出される関数の情報。
    /// </summary>
    [JsonPropertyName("function")]
    public OpenAiChatToolCallFunctionDto? Function { get; set; }        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
