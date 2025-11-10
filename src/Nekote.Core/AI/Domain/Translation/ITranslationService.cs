namespace Nekote.Core.AI.Domain.Translation
{
    /// <summary>
    /// テキスト翻訳サービスのインターフェースを定義します。
    /// </summary>
    public interface ITranslationService
    {
        /// <summary>
        /// テキストを指定された言語に翻訳します。
        /// </summary>
        /// <param name="request">翻訳リクエスト。</param>
        /// <param name="cancellationToken">キャンセルトークン。</param>
        /// <returns>翻訳結果を含む <see cref="TranslationResult"/>。</returns>
        Task<TranslationResult> TranslateAsync(
            TranslationRequest request,
            CancellationToken cancellationToken = default);
    }
}
