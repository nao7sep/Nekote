using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 情報取得に関連するメタデータ。
    /// </summary>
    public class GeminiChatRetrievalMetadataDto
    {
        /// <summary>
        /// Google 検索の情報がプロンプトの回答に役立つ可能性を示すスコア（範囲は 0 ～ 1）。
        /// </summary>
        [JsonPropertyName("googleSearchDynamicRetrievalScore")]
        public double? GoogleSearchDynamicRetrievalScore { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
