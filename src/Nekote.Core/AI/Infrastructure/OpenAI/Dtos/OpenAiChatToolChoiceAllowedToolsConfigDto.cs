using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 許可ツールの構成。
    /// </summary>
    public class OpenAiChatToolChoiceAllowedToolsConfigDto
    {
        /// <summary>
        /// モデルが使用できるツールを制約するモード。
        /// </summary>
        /// <remarks>
        /// "auto": モデルは許可されたツールの中から選択するか、メッセージを生成できる。
        /// "required": モデルは許可されたツールの中から 1 つ以上を呼び出す必要がある。
        /// </remarks>
        [JsonPropertyName("mode")]
        public string? Mode { get; set; }

        /// <summary>
        /// モデルが呼び出しを許可されるツール定義のリスト。
        /// </summary>
        [JsonPropertyName("tools")]
        public List<OpenAiChatToolChoiceAllowedToolDefinitionDto>? Tools { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
