using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "tools" がカスタムツールの場合の具象 DTO。
    /// </summary>
    /// <remarks>
    /// 指定された形式を使用して入力を処理するカスタムツール。
    /// </remarks>
    public class OpenAiChatToolCustomDto : OpenAiChatToolBaseDto
    {
        /// <summary>
        /// ツールの種類 (カスタムツールの場合は常に "custom")。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// カスタムツールのプロパティ。
        /// </summary>
        [JsonPropertyName("custom")]
        public OpenAiChatToolCustomDefinitionDto? Custom { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
