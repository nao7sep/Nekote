using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// オブジェクト形式のツール選択 (特定のツールを強制)。
    /// </summary>
    public class OpenAiChatToolChoiceObjectDto : OpenAiChatToolChoiceBaseDto
    {
        /// <summary>
        /// ツールの種類 (通常は "function")。
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
