using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI API エラーレスポンス DTO。
    /// </summary>
    /// <remarks>
    /// Realtime API と Chat Completions API の両方で使用される。
    /// Chat Completions API では event_id と type は含まれない。
    /// </remarks>
    public class OpenAiErrorResponseDto
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
        /// イベントの種類。"error" である必要がある。
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
