using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 関数宣言。
    /// </summary>
    public class GeminiChatFunctionDeclarationDto
    {
        /// <summary>
        /// 関数の名前。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// 関数の簡単な説明。
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 関数の動作。
        /// </summary>
        [JsonPropertyName("behavior")]
        public string? Behavior { get; set; }

        /// <summary>
        /// 関数のパラメータ。
        /// </summary>
        [JsonPropertyName("parameters")]
        public GeminiChatSchemaDto? Parameters { get; set; }

        /// <summary>
        /// 関数のパラメータ（JSON スキーマ形式）。
        /// </summary>
        [JsonPropertyName("parametersJsonSchema")]
        public JsonElement? ParametersJsonSchema { get; set; }

        /// <summary>
        /// 関数からの出力。
        /// </summary>
        [JsonPropertyName("response")]
        public GeminiChatSchemaDto? Response { get; set; }

        /// <summary>
        /// 関数からの出力（JSON スキーマ形式）。
        /// </summary>
        [JsonPropertyName("responseJsonSchema")]
        public JsonElement? ResponseJsonSchema { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
