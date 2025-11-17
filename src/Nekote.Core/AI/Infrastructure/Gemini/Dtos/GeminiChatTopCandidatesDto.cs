using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 各デコード ステップでログ確率が最も高い候補 DTO。
    /// </summary>
    internal class GeminiChatTopCandidatesDto
    {
        /// <summary>
        /// 対数確率の降順で並べ替えられた候補。
        /// </summary>
        [JsonPropertyName("candidates")]
        public List<GeminiChatLogprobsCandidateDto>? Candidates { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
