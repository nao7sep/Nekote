using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// モデルによって作成された関数ツールの呼び出し。
    /// </summary>
    public class OpenAiChatToolCallFunctionToolDto : OpenAiChatToolCallBaseDto
    {
        /// <summary>
        /// モデルが呼び出した関数。
        /// </summary>
        [JsonPropertyName("function")]
        public OpenAiChatToolCallFunctionDto? Function { get; set; }

        /// <summary>
        /// ツール呼び出しの ID。
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// ツールのタイプ (常に "function")。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
