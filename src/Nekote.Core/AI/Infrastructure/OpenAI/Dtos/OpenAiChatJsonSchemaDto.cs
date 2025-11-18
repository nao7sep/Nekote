using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// JSON スキーマレスポンスフォーマットの構成 DTO。
    /// </summary>
    public class OpenAiChatJsonSchemaDto
    {
        /// <summary>
        /// レスポンスフォーマットの名前 (a-z, A-Z, 0-9, アンダースコア、ダッシュのみ、最大64文字)。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// レスポンスフォーマットの説明。
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// JSON スキーマオブジェクトとして記述されたレスポンスフォーマットのスキーマ。
        /// </summary>
        [JsonPropertyName("schema")]
        public JsonElement? Schema { get; set; }

        /// <summary>
        /// 出力生成時に厳密なスキーマ準拠を有効にするかどうか。
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
