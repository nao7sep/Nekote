namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 文字列形式のツール選択 ("none", "auto", "required")。
    /// </summary>
    public class OpenAiChatToolChoiceStringDto : OpenAiChatToolChoiceBaseDto
    {
        /// <summary>
        /// ツール選択モード。
        /// </summary>
        public string? Value { get; set; }
    }
}
