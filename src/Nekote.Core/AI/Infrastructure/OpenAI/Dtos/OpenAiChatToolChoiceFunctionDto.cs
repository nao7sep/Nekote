using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "tool_choice" が関数ツールオブジェクトの場合の具体的な DTO。
    /// </summary>
    /// <remarks>
    /// {"type": "function", "function": {"name": "my_function"}} のように指定することで、
    /// 特定の関数の呼び出しを強制する。
    /// </remarks>
    public class OpenAiChatToolChoiceFunctionDto : OpenAiChatToolChoiceBaseDto
    {
        /// <summary>
        /// 関数の定義。
        /// </summary>
        [JsonPropertyName("function")]
        public OpenAiChatToolChoiceFunctionDefinitionDto? Function { get; set; }

        /// <summary>
        /// ツールの種類（関数呼び出しの場合は常に "function"）。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
