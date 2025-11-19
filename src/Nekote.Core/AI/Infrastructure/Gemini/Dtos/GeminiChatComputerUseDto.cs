using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// コンピュータ使用ツール。
    /// </summary>
    public class GeminiChatComputerUseDto
    {
        /// <summary>
        /// 運用されている環境。
        /// </summary>
        [JsonPropertyName("environment")]
        public string? Environment { get; set; }

        /// <summary>
        /// 除外される事前定義関数。
        /// </summary>
        [JsonPropertyName("excludedPredefinedFunctions")]
        public List<string>? ExcludedPredefinedFunctions { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
