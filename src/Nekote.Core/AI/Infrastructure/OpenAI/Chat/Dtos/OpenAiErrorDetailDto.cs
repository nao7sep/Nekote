using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos
{
    /// <summary>
    /// OpenAI API エラー詳細の DTO。
    /// </summary>
    internal sealed class OpenAiErrorDetailDto
    {
        /// <summary>
        /// エラーメッセージを取得します。
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; init; }

        /// <summary>
        /// エラーの種類を取得します (例: "invalid_request_error")。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; init; }

        /// <summary>
        /// エラーが発生したパラメータ名を取得します。
        /// </summary>
        [JsonPropertyName("param")]
        public string? Param { get; init; }

        /// <summary>
        /// エラーコードを取得します (例: "invalid_api_key")。
        /// </summary>
        [JsonPropertyName("code")]
        public string? Code { get; init; }
    }
}
