using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 関数呼び出し情報 DTO (レスポンス解析用)。
    /// </summary>
    internal class OpenAiChatFunctionCallDto
    {
        /// <summary>
        /// 関数名。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// 関数の引数 (JSON 文字列)。
        /// </summary>
        [JsonPropertyName("arguments")]
        public string? Arguments { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
