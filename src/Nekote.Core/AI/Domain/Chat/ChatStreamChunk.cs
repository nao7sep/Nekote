namespace Nekote.Core.AI.Domain.Chat
{
    /// <summary>
    /// ストリーミング中に受信するチャンクを表します。
    /// </summary>
    public sealed class ChatStreamChunk
    {
        /// <summary>
        /// このチャンクが属する選択肢のインデックスを取得します。
        /// 複数の選択肢をストリーミングする場合 (n > 1)、各チャンクは異なるインデックスを持つ場合があります。
        /// </summary>
        public required int ChoiceIndex { get; init; }

        /// <summary>
        /// このチャンクのテキストコンテンツを取得します。
        /// </summary>
        public string? ContentDelta { get; init; }

        /// <summary>
        /// この選択肢のストリーム完了理由を取得します (この選択肢の最終チャンクでのみ設定)。
        /// </summary>
        public string? FinishReason { get; init; }

        /// <summary>
        /// 使用されたトークン数の情報を取得します (全体の最終チャンクでのみ設定される場合があります)。
        /// </summary>
        public TokenUsage? Usage { get; init; }

        /// <summary>
        /// プロバイダー固有の応答 ID を取得します。
        /// </summary>
        public string? ResponseId { get; init; }
    }
}
