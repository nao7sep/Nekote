namespace Nekote.Core.AI.Domain.Embedding
{
    /// <summary>
    /// テキストエンベディングサービスのインターフェースを定義します。
    /// </summary>
    public interface ITextEmbeddingService
    {
        /// <summary>
        /// 単一のテキストをベクトル表現に変換します。
        /// </summary>
        /// <param name="text">エンベディング化するテキスト。</param>
        /// <param name="cancellationToken">キャンセルトークン。</param>
        /// <returns>エンベディングベクトルを含む <see cref="EmbeddingResult"/>。</returns>
        Task<EmbeddingResult> GetEmbeddingAsync(
            string text,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 複数のテキストを一括でベクトル表現に変換します。
        /// </summary>
        /// <param name="texts">エンベディング化するテキストのリスト。</param>
        /// <param name="cancellationToken">キャンセルトークン。</param>
        /// <returns>各テキストのエンベディングベクトルを含むリスト。</returns>
        Task<IReadOnlyList<EmbeddingResult>> GetEmbeddingsAsync(
            IReadOnlyList<string> texts,
            CancellationToken cancellationToken = default);
    }
}
