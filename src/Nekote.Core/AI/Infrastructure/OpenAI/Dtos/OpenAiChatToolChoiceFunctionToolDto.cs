using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "tool_choice" が関数ツールオブジェクトの場合の具象 DTO。
    /// </summary>
    /// <remarks>
    /// {"type": "function", "function": {"name": "my_function"}} のように指定することで、
    /// 特定の関数の呼び出しを強制する。
    /// </remarks>
    public class OpenAiChatToolChoiceFunctionDto : OpenAiChatToolChoiceBaseDto
    {
        /// <summary>
        /// ツールの種類 (関数呼び出しの場合は常に "function")。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// 関数の情報。
        /// </summary>
        [JsonPropertyName("function")]
        public OpenAiChatToolChoiceFunctionSpecDto? Function { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
