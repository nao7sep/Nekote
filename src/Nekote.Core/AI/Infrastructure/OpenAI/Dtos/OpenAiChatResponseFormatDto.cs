using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// レスポンスフォーマットの指定 DTO。
    /// </summary>
    internal class OpenAiChatResponseFormatDto
    {
        /// <summary>
        /// フォーマットのタイプ ("text", "json_schema", "json_object")。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// JSON スキーマの構成 (type が "json_schema" の場合にのみ使用)。
        /// </summary>
        [JsonPropertyName("json_schema")]
        public OpenAiChatJsonSchemaDto? JsonSchema { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
