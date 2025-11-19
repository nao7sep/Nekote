using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// モデルによって作成されたカスタムツールの呼び出し。
    /// </summary>
    public class OpenAiChatToolCallCustomToolDto : OpenAiChatToolCallBaseDto
    {
        /// <summary>
        /// モデルが呼び出したカスタムツール。
        /// </summary>
        [JsonPropertyName("custom")]
        public OpenAiChatToolCallCustomDto? Custom { get; set; }

        /// <summary>
        /// ツール呼び出しの ID。
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// ツールのタイプ (常に "custom")。
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
