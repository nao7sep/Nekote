namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "content" が単純な文字列の場合の具体的な DTO。
    /// </summary>
    public class OpenAiChatMessageContentStringDto : OpenAiChatMessageContentBaseDto
    {
        /// <summary>
        /// テキスト内容。
        /// </summary>
        public string? Text { get; set; }
    }
}
