namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "tool_choice" が文字列の場合の具象 DTO。
    /// </summary>
    /// <remarks>
    /// "none": モデルはツールを呼び出さず、メッセージを生成する。
    /// "auto": モデルはメッセージの生成と 1 つ以上のツール呼び出しのいずれかを選択できる。
    /// "required": モデルは 1 つ以上のツールを呼び出す必要がある。
    /// </remarks>
    public class OpenAiChatToolChoiceStringDto : OpenAiChatToolChoiceBaseDto
    {
        /// <summary>
        /// ツール選択モード ("none", "auto", または "required")。
        /// </summary>
        public string? Value { get; set; }
    }
}
