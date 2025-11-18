using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ツール呼び出し情報 DTO (レスポンス解析用)。
    /// </summary>
    internal class OpenAiChatToolCallDto
    {
        /// <summary>
        /// ツール呼び出しの一意識別子。
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// ツールのタイプ ("function" など)。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// 関数呼び出し情報。
        /// </summary>
        [JsonPropertyName("function")]
        public OpenAiChatFunctionCallDto? Function { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
