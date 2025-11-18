using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI API エラー詳細 DTO。
    /// Realtime API のエラーイベント構造 (https://platform.openai.com/docs/api-reference/realtime-server-events/error) に基づく。
    /// Chat Completions API のエラーでは event_id は含まれず、message、type、code、param のみが使用される。
    /// </summary>
    internal class OpenAiErrorDetailsDto
    {
        /// <summary>
        /// エラーメッセージ。
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// エラーの種類。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// エラーコード。
        /// </summary>
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        /// <summary>
        /// 関連するイベントの ID。
        /// </summary>
        [JsonPropertyName("event_id")]
        public string? EventId { get; set; }

        /// <summary>
        /// エラーに関連するパラメータ名。
        /// </summary>
        [JsonPropertyName("param")]
        public string? Param { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
