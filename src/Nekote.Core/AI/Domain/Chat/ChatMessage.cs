namespace Nekote.Core.AI.Domain.Chat
{
    /// <summary>
    /// チャットメッセージを表します。
    /// </summary>
    public sealed class ChatMessage
    {
        /// <summary>
        /// メッセージの役割を取得します。
        /// </summary>
        public required ChatRole Role { get; init; }

        /// <summary>
        /// メッセージの内容を取得します。
        /// </summary>
        public required string Content { get; init; }

        /// <summary>
        /// メッセージの名前 (オプション) を取得します。
        /// </summary>
        public string? Name { get; init; }
    }
}
