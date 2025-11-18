using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ストリーミング関数呼び出しの増分 DTO (レスポンス解析用)。
    /// </summary>
    internal class OpenAiChatStreamDeltaFunctionCallDto
    {
        /// <summary>
        /// 関数名の増分部分。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// 関数の引数の増分部分 (JSON 文字列)。
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
