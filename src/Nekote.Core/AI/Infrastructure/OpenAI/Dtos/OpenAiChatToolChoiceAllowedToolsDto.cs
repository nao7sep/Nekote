using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "tool_choice" が許可ツール制約オブジェクトの場合の具象 DTO。
    /// </summary>
    /// <remarks>
    /// モデルが使用できるツールを事前定義されたセットに制約する。
    /// </remarks>
    public class OpenAiChatToolChoiceAllowedToolsDto : OpenAiChatToolChoiceBaseDto
    {
        /// <summary>
        /// ツールの種類 (常に "allowed_tools")。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// 許可ツールの構成。
        /// </summary>
        [JsonPropertyName("allowed_tools")]
        public OpenAiChatToolChoiceAllowedToolsConfigDto? AllowedTools { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
