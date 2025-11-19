using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 関数呼び出し。
    /// </summary>
    public class GeminiChatFunctionCallDto
    {
        /// <summary>
        /// 関数呼び出しの一意の ID。
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// 呼び出す関数の名前。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// JSON オブジェクト形式の関数パラメータと値。
        /// </summary>
        [JsonPropertyName("args")]
        public JsonElement? Args { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
