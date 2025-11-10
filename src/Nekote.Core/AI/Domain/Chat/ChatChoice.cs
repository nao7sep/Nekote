namespace Nekote.Core.AI.Domain.Chat
{
    /// <summary>
    /// AI からの単一の選択肢を表します。
    /// </summary>
    public sealed class ChatChoice
    {
        /// <summary>
        /// この選択肢のインデックスを取得します (複数の選択肢がある場合)。
        /// </summary>
        public required int Index { get; init; }

        /// <summary>
        /// 生成されたメッセージの内容を取得します。
        /// </summary>
        public required string Content { get; init; }

        /// <summary>
        /// この選択肢の完了理由を取得します。
        /// </summary>
        public required string FinishReason { get; init; }
    }
}
