using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Google マップの特定の場所の機能に関する回答を提供するソースのコレクション DTO。
    /// </summary>
    public class GeminiChatPlaceAnswerSourcesDto
    {
        /// <summary>
        /// Google マップで特定の場所の特徴に関する回答を生成するために使用されるクチコミのスニペット。
        /// </summary>
        [JsonPropertyName("reviewSnippets")]
        public List<GeminiChatReviewSnippetDto>? ReviewSnippets { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
