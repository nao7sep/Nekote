namespace Nekote.Core.AI.Domain.Translation
{
    /// <summary>
    /// 翻訳リクエストを表します。
    /// </summary>
    public sealed class TranslationRequest
    {
        /// <summary>
        /// 翻訳するテキストを取得します。
        /// </summary>
        public required string Text { get; init; }

        /// <summary>
        /// ソース言語コード (例: "en", "ja") を取得します。
        /// </summary>
        public string? SourceLanguage { get; init; }

        /// <summary>
        /// ターゲット言語コード (例: "en", "ja") を取得します。
        /// </summary>
        public required string TargetLanguage { get; init; }
    }
}
