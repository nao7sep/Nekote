using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 特定のレスポンスの一部に対するソースの引用 DTO。
    /// </summary>
    internal class GeminiChatCitationSourceDto
    {
        /// <summary>
        /// このソースに起因するレスポンスのセグメントの開始（バイト単位）。
        /// </summary>
        [JsonPropertyName("startIndex")]
        public int? StartIndex { get; set; }

        /// <summary>
        /// アトリビューション セグメントの終了（この値を含まない、バイト単位）。
        /// </summary>
        [JsonPropertyName("endIndex")]
        public int? EndIndex { get; set; }

        /// <summary>
        /// テキストの一部にソースとして帰属する URI。
        /// </summary>
        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        /// <summary>
        /// セグメントのソースとして帰属する GitHub プロジェクトのライセンス。
        /// </summary>
        [JsonPropertyName("license")]
        public string? License { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
