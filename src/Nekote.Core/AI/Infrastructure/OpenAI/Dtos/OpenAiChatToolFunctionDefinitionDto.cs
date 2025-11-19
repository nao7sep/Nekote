using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ツール内の関数定義。
    /// </summary>
    public class OpenAiChatToolFunctionDefinitionDto
    {
        /// <summary>
        /// 関数の名前。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// 関数の説明。
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 関数のパラメータ (JSON スキーマ形式)。
        /// </summary>
        [JsonPropertyName("parameters")]
        public JsonElement? Parameters { get; set; }

        /// <summary>
        /// 厳格モード (パラメータスキーマに定義されたすべてのパラメータが必須となる)。
        /// </summary>
        [JsonPropertyName("strict")]
        public bool? Strict { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
