using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "tool_choice" が許可ツール制約オブジェクトの場合の具体的な DTO。
    /// </summary>
    /// <remarks>
    /// モデルが使用できるツールを事前定義されたセットに制約する。
    /// </remarks>
    public class OpenAiChatToolChoiceAllowedDto : OpenAiChatToolChoiceBaseDto
    {
        /// <summary>
        /// 許可ツールの構成。
        /// </summary>
        [JsonPropertyName("allowed_tools")]
        public OpenAiChatToolChoiceAllowedDefinitionDto? AllowedTools { get; set; }

        /// <summary>
        /// ツールの種類（常に "allowed_tools"）。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
