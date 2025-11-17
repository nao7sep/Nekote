using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Chat API のストリーミングレスポンスチャンク DTO。
    /// </summary>
    internal class GeminiChatStreamChunkDto
    {
        /// <summary>
        /// ストリーミングチャンクの候補リスト。
        /// </summary>
        [JsonPropertyName("candidates")]
        public List<GeminiChatStreamCandidateDto>? Candidates { get; set; }

        /// <summary>
        /// 使用量メタデータ (最終チャンクでのみ返される)。
        /// </summary>
        [JsonPropertyName("usageMetadata")]
        public GeminiChatUsageMetadataDto? UsageMetadata { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
