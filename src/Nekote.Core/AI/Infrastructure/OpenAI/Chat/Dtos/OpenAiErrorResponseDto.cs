using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos
{
    /// <summary>
    /// OpenAI API エラーレスポンスの DTO。
    /// </summary>
    internal sealed class OpenAiErrorResponseDto
    {
        /// <summary>
        /// エラーの詳細情報を取得します。
        /// </summary>
        [JsonPropertyName("error")]
        public OpenAiErrorDetailDto? Error { get; init; }
    }
}
