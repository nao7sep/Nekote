using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// コード実行結果。
    /// </summary>
    public class GeminiChatCodeExecutionResultDto
    {
        /// <summary>
        /// コード実行の結果。
        /// </summary>
        [JsonPropertyName("outcome")]
        public string? Outcome { get; set; }

        /// <summary>
        /// コードの実行が成功した場合は stdout、それ以外の場合は stderr またはその他の説明。
        /// </summary>
        [JsonPropertyName("output")]
        public string? Output { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
