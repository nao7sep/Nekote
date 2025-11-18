using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI API エラーレスポンス DTO。
    /// Realtime API のエラーイベント構造 (https://platform.openai.com/docs/api-reference/realtime-server-events/error) に基づく。
    /// Chat Completions API のエラーレスポンスにも使用されるが、その場合 event_id と type は含まれない。
    /// </summary>
    internal class OpenAiErrorResponseDto
    {
        /// <summary>
        /// エラーの詳細情報。
        /// </summary>
        [JsonPropertyName("error")]
        public OpenAiErrorDetailsDto? Error { get; set; }

        /// <summary>
        /// サーバーイベントの一意の ID。
        /// </summary>
        [JsonPropertyName("event_id")]
        public string? EventId { get; set; }

        /// <summary>
        /// イベントの種類。"error" である必要があります。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
