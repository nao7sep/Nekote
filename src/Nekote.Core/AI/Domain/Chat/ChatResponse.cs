namespace Nekote.Core.AI.Domain.Chat
{
    /// <summary>
    /// AI からのチャット応答を表します。
    /// </summary>
    public sealed class ChatResponse
    {
        /// <summary>
        /// 生成されたメッセージの内容を取得します。
        /// </summary>
        public required string Content { get; init; }

        /// <summary>
        /// 応答の完了理由を取得します。
        /// </summary>
        public required string FinishReason { get; init; }

        /// <summary>
        /// 使用されたトークン数の情報を取得します。
        /// </summary>
        public TokenUsage? Usage { get; init; }

        /// <summary>
        /// プロバイダー固有の応答 ID を取得します。
        /// </summary>
        public string? ResponseId { get; init; }
    }
}
