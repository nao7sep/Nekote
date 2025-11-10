namespace Nekote.Core.AI.Domain.Embedding
{
    /// <summary>
    /// テキストエンベディングの結果を表します。
    /// </summary>
    public sealed class EmbeddingResult
    {
        /// <summary>
        /// エンベディングベクトルを取得します。
        /// </summary>
        public required IReadOnlyList<float> Vector { get; init; }

        /// <summary>
        /// 元のテキストを取得します。
        /// </summary>
        public required string OriginalText { get; init; }

        /// <summary>
        /// 使用されたトークン数を取得します。
        /// </summary>
        public int? TokenCount { get; init; }
    }
}
