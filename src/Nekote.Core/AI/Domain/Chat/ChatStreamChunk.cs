namespace Nekote.Core.AI.Domain.Chat
{
    /// <summary>
    /// ストリーミング中に受信するチャンクを表します。
    /// </summary>
    public sealed class ChatStreamChunk
    {
        /// <summary>
        /// このチャンクのテキストコンテンツを取得します。
        /// </summary>
        public string? ContentDelta { get; init; }

        /// <summary>
        /// ストリームの完了理由を取得します (最終チャンクでのみ設定)。
        /// </summary>
        public string? FinishReason { get; init; }

        /// <summary>
        /// 使用されたトークン数の情報を取得します (最終チャンクでのみ設定される場合があります)。
        /// </summary>
        public TokenUsage? Usage { get; init; }

        /// <summary>
        /// プロバイダー固有の応答 ID を取得します。
        /// </summary>
        public string? ResponseId { get; init; }

        /// <summary>
        /// これが最終チャンクかどうかを示します。
        /// </summary>
        public bool IsComplete { get; init; }
    }
}
