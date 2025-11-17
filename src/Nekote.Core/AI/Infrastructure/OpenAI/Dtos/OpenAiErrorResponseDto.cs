using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI API エラーレスポンス DTO。
    /// </summary>
    internal class OpenAiErrorResponseDto
    {
        /// <summary>
        /// エラーの詳細情報。
        /// </summary>
        [JsonPropertyName("error")]
        public OpenAiErrorDto? Error { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
