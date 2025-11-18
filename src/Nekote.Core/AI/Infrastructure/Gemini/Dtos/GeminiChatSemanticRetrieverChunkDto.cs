using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// セマンティック リトリーバーで取得された Chunk の識別子 DTO。
    /// </summary>
    public class GeminiChatSemanticRetrieverChunkDto
    {
        /// <summary>
        /// リクエストの SemanticRetrieverConfig.source に一致するソースの名前（出力専用）。
        /// 例: corpora/123 または corpora/123/documents/abc
        /// </summary>
        [JsonPropertyName("source")]
        public string? Source { get; set; }

        /// <summary>
        /// 帰属テキストを含む Chunk の名前（出力専用）。
        /// 例: corpora/123/documents/abc/chunks/xyz
        /// </summary>
        [JsonPropertyName("chunk")]
        public string? Chunk { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
