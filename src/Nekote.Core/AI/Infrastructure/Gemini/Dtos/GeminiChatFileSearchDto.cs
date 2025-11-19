using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// ファイル検索ツール。
    /// </summary>
    public class GeminiChatFileSearchDto
    {
        /// <summary>
        /// 取得元の fileSearchStore の名前。
        /// </summary>
        [JsonPropertyName("fileSearchStoreNames")]
        public List<string>? FileSearchStoreNames { get; set; }

        /// <summary>
        /// メタデータ フィルタ。
        /// </summary>
        [JsonPropertyName("metadataFilter")]
        public string? MetadataFilter { get; set; }

        /// <summary>
        /// 取得するセマンティック検索チャンクの数。
        /// </summary>
        [JsonPropertyName("topK")]
        public int? TopK { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
