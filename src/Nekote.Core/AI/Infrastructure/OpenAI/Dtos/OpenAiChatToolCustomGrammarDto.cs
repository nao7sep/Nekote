using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// カスタムツールの文法定義。
    /// </summary>
    public class OpenAiChatToolCustomGrammarDto
    {
        /// <summary>
        /// 文法定義。
        /// </summary>
        [JsonPropertyName("definition")]
        public string? Definition { get; set; }

        /// <summary>
        /// 文法定義の構文（"lark" または "regex"）。
        /// </summary>
        [JsonPropertyName("syntax")]
        public string? Syntax { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
