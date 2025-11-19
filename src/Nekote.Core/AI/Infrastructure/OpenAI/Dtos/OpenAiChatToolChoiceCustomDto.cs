using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "tool_choice" がカスタムツールオブジェクトの場合の具象 DTO。
    /// </summary>
    /// <remarks>
    /// モデルが使用すべきカスタムツールを指定する。
    /// </remarks>
    public class OpenAiChatToolChoiceCustomDto : OpenAiChatToolChoiceBaseDto
    {
        /// <summary>
        /// カスタムツールの情報。
        /// </summary>
        [JsonPropertyName("custom")]
        public OpenAiChatToolChoiceCustomDefinitionDto? Custom { get; set; }

        /// <summary>
        /// ツールの種類 (常に "custom")。
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
