using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// ファイル検索ツールで取得されたコンテキストのグラウンディング チャンク DTO。
    /// </summary>
    /// <remarks>
    /// https://ai.google.dev/api/generate-content の RetrievedContext セクションの JSON 例には
    /// fileSearchStore フィールドが出現するが、ドキュメント本文には記載されていない。
    /// このフィールドは Google の File Search 機能
    /// (https://ai.google.dev/gemini-api/docs/file-search) に関連すると思われるが、
    /// 正式なドキュメントに記載されておらず、かつチャットとエンベディング以外の機能に関連するため、
    /// 現時点 (2025-11-19) では省略されている。
    /// </remarks>
    public class GeminiChatRetrievedContextDto
    {
        /// <summary>
        /// セマンティック検索ドキュメントの URI 参照。
        /// </summary>
        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        /// <summary>
        /// ドキュメントのタイトル。
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// チャンクのテキスト。
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
