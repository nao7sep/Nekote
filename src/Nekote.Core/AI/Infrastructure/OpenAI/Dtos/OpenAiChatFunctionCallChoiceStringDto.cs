namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 文字列形式の関数呼び出し選択 ("none", "auto")。
    /// </summary>
    [Obsolete("This class is deprecated. Use ToolChoice instead.")]
    public class OpenAiChatFunctionCallChoiceStringDto : OpenAiChatFunctionCallChoiceBaseDto
    {
        /// <summary>
        /// 関数呼び出し選択モード。
        /// </summary>
        public string? Value { get; set; }
    }
}
