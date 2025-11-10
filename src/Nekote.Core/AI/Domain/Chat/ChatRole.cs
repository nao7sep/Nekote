namespace Nekote.Core.AI.Domain.Chat
{
    /// <summary>
    /// チャットメッセージの役割を表します。
    /// </summary>
    public enum ChatRole
    {
        /// <summary>
        /// システムメッセージ。
        /// </summary>
        System,

        /// <summary>
        /// ユーザーメッセージ。
        /// </summary>
        User,

        /// <summary>
        /// アシスタント (AI) メッセージ。
        /// </summary>
        Assistant
    }
}
