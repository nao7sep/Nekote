using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 完了トークンの内訳詳細 DTO。
    /// </summary>
    internal class OpenAiChatCompletionTokensDetailsDto
    {
        /// <summary>
        /// 推論 (reasoning) に使用されたトークン数。
        /// </summary>
        [JsonPropertyName("reasoning_tokens")]
        public int? ReasoningTokens { get; set; }

        /// <summary>
        /// オーディオ出力に使用されたトークン数。
        /// </summary>
        [JsonPropertyName("audio_tokens")]
        public int? AudioTokens { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
