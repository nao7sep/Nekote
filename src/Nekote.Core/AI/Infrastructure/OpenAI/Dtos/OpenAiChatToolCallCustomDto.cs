using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// カスタムツール呼び出しの詳細情報。
    /// </summary>
    public class OpenAiChatToolCallCustomDto
    {
        /// <summary>
        /// モデルによって生成されたカスタムツール呼び出しの入力。
        /// </summary>
        [JsonPropertyName("input")]
        public string? Input { get; set; }

        /// <summary>
        /// 呼び出すカスタムツールの名前。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
