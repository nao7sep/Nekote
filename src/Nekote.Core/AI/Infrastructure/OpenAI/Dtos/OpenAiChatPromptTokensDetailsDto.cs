using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// プロンプトトークンの内訳詳細 DTO。
    /// </summary>
    internal class OpenAiChatPromptTokensDetailsDto
    {
        /// <summary>
        /// オーディオ入力に使用されたトークン数。
        /// </summary>
        [JsonPropertyName("audio_tokens")]
        public int? AudioTokens { get; set; }

        /// <summary>
        /// キャッシュから読み込まれたトークン数。
        /// </summary>
        [JsonPropertyName("cached_tokens")]
        public int? CachedTokens { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
