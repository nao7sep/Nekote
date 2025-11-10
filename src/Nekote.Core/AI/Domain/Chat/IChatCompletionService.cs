namespace Nekote.Core.AI.Domain.Chat
{
    /// <summary>
    /// チャット補完サービスのインターフェースを定義します。
    /// </summary>
    public interface IChatCompletionService
    {
        /// <summary>
        /// チャットメッセージのリストを受け取り、AI からの応答を非同期で取得します。
        /// </summary>
        /// <param name="messages">チャットメッセージのリスト。</param>
        /// <param name="options">オプション設定。</param>
        /// <param name="cancellationToken">キャンセルトークン。</param>
        /// <returns>AI からの応答を含む <see cref="ChatResponse"/>。</returns>
        Task<ChatResponse> GetCompletionAsync(
            IReadOnlyList<ChatMessage> messages,
            ChatCompletionOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// ストリーミングモードでチャット補完を実行し、チャンクを逐次的に取得します。
        /// </summary>
        /// <param name="messages">チャットメッセージのリスト。</param>
        /// <param name="options">オプション設定。</param>
        /// <param name="cancellationToken">キャンセルトークン。</param>
        /// <returns>チャンクのストリーム。最終チャンクには Usage, FinishReason, ResponseId が含まれる場合があります。</returns>
        IAsyncEnumerable<ChatStreamChunk> GetCompletionStreamAsync(
            IReadOnlyList<ChatMessage> messages,
            ChatCompletionOptions? options = null,
            CancellationToken cancellationToken = default);
    }
}
