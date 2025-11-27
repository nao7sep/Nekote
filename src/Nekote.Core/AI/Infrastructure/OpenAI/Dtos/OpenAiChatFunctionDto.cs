using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 関数の定義（非推奨: tools に置き換えられた）。
    /// </summary>
    [Obsolete("This class is deprecated. Use Tools instead.")]
    public class OpenAiChatFunctionDto
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
        /// 関数のパラメータ（JSON スキーマ形式）。
        /// </summary>
        [JsonPropertyName("parameters")]
        public JsonElement? Parameters { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
