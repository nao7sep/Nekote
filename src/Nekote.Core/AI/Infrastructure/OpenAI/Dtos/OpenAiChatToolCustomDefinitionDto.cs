using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// カスタムツールのプロパティ。
    /// </summary>
    public class OpenAiChatToolCustomDefinitionDto
    {
        /// <summary>
        /// カスタムツールの名前 (ツール呼び出しでの識別に使用される)。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// カスタムツールのオプションの説明 (より多くのコンテキストを提供するために使用される)。
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// カスタムツールの入力フォーマット (デフォルトは制約のないテキスト)。
        /// </summary>
        [JsonPropertyName("format")]
        public OpenAiChatToolCustomFormatBaseDto? Format { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
