using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Logprobs の結果 DTO。
    /// </summary>
    internal class GeminiChatLogprobsResultDto
    {
        /// <summary>
        /// 各デコード ステップでログ確率が最も高い候補のリスト。
        /// </summary>
        [JsonPropertyName("topCandidates")]
        public List<GeminiChatTopCandidatesDto>? TopCandidates { get; set; }

        /// <summary>
        /// 選択された候補のリスト。
        /// </summary>
        [JsonPropertyName("chosenCandidates")]
        public List<GeminiChatLogprobsCandidateDto>? ChosenCandidates { get; set; }

        /// <summary>
        /// すべてのトークンのログ確率の合計。
        /// </summary>
        [JsonPropertyName("logProbabilitySum")]
        public double? LogProbabilitySum { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
