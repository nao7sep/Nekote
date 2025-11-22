namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "content" がコンテンツパーツの配列の場合の具体的な DTO。
    /// </summary>
    public class OpenAiChatMessageContentArrayDto : OpenAiChatMessageContentBaseDto
    {
        /// <summary>
        /// コンテンツパーツのリスト。
        /// </summary>
        public List<OpenAiChatMessageContentPartBaseDto>? Parts { get; set; }
    }
}
