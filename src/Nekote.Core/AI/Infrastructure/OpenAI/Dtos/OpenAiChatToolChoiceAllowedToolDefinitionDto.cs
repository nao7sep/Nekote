using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 許可ツールリスト内のツール定義。
    /// </summary>
    public class OpenAiChatToolChoiceAllowedToolDefinitionDto
    {
        /// <summary>
        /// ツールの種類 (例: "function")。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// 関数の情報。
        /// </summary>
        [JsonPropertyName("function")]
        public OpenAiChatToolChoiceFunctionDto? Function { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
