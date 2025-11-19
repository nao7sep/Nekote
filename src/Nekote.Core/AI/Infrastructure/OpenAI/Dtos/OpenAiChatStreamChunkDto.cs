using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI Chat API のストリーミングレスポンスチャンク DTO。
    /// </summary>
    public class OpenAiChatStreamChunkDto
    {
        /// <summary>
        /// ストリーミングチャンクの候補リスト。
        /// </summary>
        [JsonPropertyName("choices")]
        public List<OpenAiChatStreamChoiceDto>? Choices { get; set; }

        /// <summary>
        /// レスポンスが作成された Unix タイムスタンプ。
        /// </summary>
        [JsonPropertyName("created")]
        public long? Created { get; set; }

        /// <summary>
        /// レスポンスの一意識別子。
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// 使用されたモデルの識別子。
        /// </summary>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        /// <summary>
        /// オブジェクトの種類 (通常は "chat.completion.chunk")。
        /// </summary>
        [JsonPropertyName("object")]
        public string? Object { get; set; }

        /// <summary>
        /// 実際に使用されたサービス層。
        /// </summary>
        [JsonPropertyName("service_tier")]
        public string? ServiceTier { get; set; }

        /// <summary>
        /// システムフィンガープリント (非推奨)。
        /// </summary>
        [JsonPropertyName("system_fingerprint")]
        public string? SystemFingerprint { get; set; }

        /// <summary>
        /// トークン使用量の詳細 (最終チャンクでのみ返される)。
        /// </summary>
        [JsonPropertyName("usage")]
        public OpenAiChatUsageDto? Usage { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
