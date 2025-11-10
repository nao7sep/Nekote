namespace Nekote.Core.AI.Domain.Translation
{
    /// <summary>
    /// 翻訳結果を表します。
    /// </summary>
    public sealed class TranslationResult
    {
        /// <summary>
        /// 翻訳されたテキストを取得します。
        /// </summary>
        public required string TranslatedText { get; init; }

        /// <summary>
        /// 検出されたソース言語コードを取得します。
        /// </summary>
        public string? DetectedSourceLanguage { get; init; }
    }
}
