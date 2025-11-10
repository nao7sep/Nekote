namespace Nekote.Core.AI.Domain.Chat
{
    /// <summary>
    /// AI からのチャット応答を表します。
    /// </summary>
    public sealed class ChatResponse
    {
        /// <summary>
        /// 生成された選択肢のリストを取得します。
        /// 通常は 1 つの選択肢ですが、n > 1 の場合は複数の選択肢が含まれます。
        /// </summary>
        public required IReadOnlyList<ChatChoice> Choices { get; init; }

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
