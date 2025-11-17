using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Chat API の使用量メタデータ DTO。
    /// ストリーミングおよび非ストリーミングレスポンスで共有される。
    /// </summary>
    internal class GeminiChatUsageMetadataDto
    {
        /// <summary>
        /// プロンプトで使用されたトークン数。
        /// </summary>
        [JsonPropertyName("promptTokenCount")]
        public int? PromptTokenCount { get; set; }

        /// <summary>
        /// 生成された候補で使用されたトークン数。
        /// </summary>
        [JsonPropertyName("candidatesTokenCount")]
        public int? CandidatesTokenCount { get; set; }

        /// <summary>
        /// 合計トークン数。
        /// </summary>
        [JsonPropertyName("totalTokenCount")]
        public int? TotalTokenCount { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
