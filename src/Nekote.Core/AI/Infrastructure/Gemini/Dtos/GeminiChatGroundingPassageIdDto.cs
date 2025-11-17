using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// GroundingPassage 内のパートの識別子 DTO。
    /// </summary>
    internal class GeminiChatGroundingPassageIdDto
    {
        /// <summary>
        /// GenerateAnswerRequest の GroundingPassage.id と一致するパッセージの ID（出力専用）。
        /// </summary>
        [JsonPropertyName("passageId")]
        public string? PassageId { get; set; }

        /// <summary>
        /// GenerateAnswerRequest の GroundingPassage.content 内のパーツのインデックス（出力専用）。
        /// </summary>
        [JsonPropertyName("partIndex")]
        public int? PartIndex { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
