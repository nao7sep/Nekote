using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// スキーマ (OpenAPI スキーマのサブセット)。
    /// </summary>
    public class GeminiChatSchemaDto
    {
        /// <summary>
        /// 型。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// 形式。
        /// </summary>
        [JsonPropertyName("format")]
        public string? Format { get; set; }

        /// <summary>
        /// タイトル。
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// 説明。
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// nullable かどうか。
        /// </summary>
        [JsonPropertyName("nullable")]
        public bool? Nullable { get; set; }

        /// <summary>
        /// 列挙型の値のリスト。
        /// </summary>
        [JsonPropertyName("enum")]
        public List<string>? Enum { get; set; }

        /// <summary>
        /// 配列要素の最大数。
        /// </summary>
        [JsonPropertyName("maxItems")]
        public string? MaxItems { get; set; }

        /// <summary>
        /// 配列要素の最小数。
        /// </summary>
        [JsonPropertyName("minItems")]
        public string? MinItems { get; set; }

        /// <summary>
        /// オブジェクトのプロパティ。
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, GeminiChatSchemaDto>? Properties { get; set; }

        /// <summary>
        /// 必須プロパティのリスト。
        /// </summary>
        [JsonPropertyName("required")]
        public List<string>? Required { get; set; }

        /// <summary>
        /// プロパティの最小数。
        /// </summary>
        [JsonPropertyName("minProperties")]
        public string? MinProperties { get; set; }

        /// <summary>
        /// プロパティの最大数。
        /// </summary>
        [JsonPropertyName("maxProperties")]
        public string? MaxProperties { get; set; }

        /// <summary>
        /// 文字列の最小長。
        /// </summary>
        [JsonPropertyName("minLength")]
        public string? MinLength { get; set; }

        /// <summary>
        /// 文字列の最大長。
        /// </summary>
        [JsonPropertyName("maxLength")]
        public string? MaxLength { get; set; }

        /// <summary>
        /// 文字列のパターン（正規表現）。
        /// </summary>
        [JsonPropertyName("pattern")]
        public string? Pattern { get; set; }

        /// <summary>
        /// 例。
        /// </summary>
        [JsonPropertyName("example")]
        public JsonElement? Example { get; set; }

        /// <summary>
        /// いずれかのサブスキーマ。
        /// </summary>
        [JsonPropertyName("anyOf")]
        public List<GeminiChatSchemaDto>? AnyOf { get; set; }

        /// <summary>
        /// プロパティの順序。
        /// </summary>
        [JsonPropertyName("propertyOrdering")]
        public List<string>? PropertyOrdering { get; set; }

        /// <summary>
        /// デフォルト値。
        /// </summary>
        [JsonPropertyName("default")]
        public JsonElement? Default { get; set; }

        /// <summary>
        /// 配列要素のスキーマ。
        /// </summary>
        [JsonPropertyName("items")]
        public GeminiChatSchemaDto? Items { get; set; }

        /// <summary>
        /// 最小値。
        /// </summary>
        [JsonPropertyName("minimum")]
        public double? Minimum { get; set; }

        /// <summary>
        /// 最大値。
        /// </summary>
        [JsonPropertyName("maximum")]
        public double? Maximum { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
