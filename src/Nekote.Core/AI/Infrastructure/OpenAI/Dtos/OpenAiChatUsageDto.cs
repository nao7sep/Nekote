using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// トークン使用量。
    /// ストリーミングおよび非ストリーミングレスポンスで共有される。
    /// </summary>
    public class OpenAiChatUsageDto
    {
        /// <summary>
        /// 補完で生成されたトークン数。
        /// </summary>
        [JsonPropertyName("completion_tokens")]
        public int? CompletionTokens { get; set; }

        /// <summary>
        /// プロンプトで使用されたトークン数。
        /// </summary>
        [JsonPropertyName("prompt_tokens")]
        public int? PromptTokens { get; set; }

        /// <summary>
        /// 合計トークン数。
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public int? TotalTokens { get; set; }

        /// <summary>
        /// 完了トークンの詳細（推論トークン等）。
        /// </summary>
        [JsonPropertyName("completion_tokens_details")]
        public OpenAiChatCompletionTokensDetailsDto? CompletionTokensDetails { get; set; }

        /// <summary>
        /// プロンプトトークンの詳細（キャッシュヒット等）。
        /// </summary>
        [JsonPropertyName("prompt_tokens_details")]
        public OpenAiChatPromptTokensDetailsDto? PromptTokensDetails { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
