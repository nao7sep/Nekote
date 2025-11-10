namespace Nekote.Core.AI.Domain.Chat
{
    /// <summary>
    /// トークン使用量の情報を表します。
    /// </summary>
    public sealed class TokenUsage
    {
        /// <summary>
        /// プロンプトで使用されたトークン数を取得します。
        /// </summary>
        public required int PromptTokens { get; init; }

        /// <summary>
        /// 補完 (生成) で使用されたトークン数を取得します。
        /// </summary>
        public required int CompletionTokens { get; init; }

        /// <summary>
        /// 合計トークン数を取得します。
        /// </summary>
        public required int TotalTokens { get; init; }
    }
}
