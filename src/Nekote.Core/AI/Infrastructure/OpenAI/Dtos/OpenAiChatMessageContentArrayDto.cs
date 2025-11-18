namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "content" が "parts" の配列の場合の具象 DTO。
    /// </summary>
    public class OpenAiChatMessageContentArrayDto : OpenAiChatMessageContentBaseDto
    {
        /// <summary>
        /// コンテンツパーツのリスト。
        /// </summary>
        public List<OpenAiChatMessageContentPartDto>? Parts { get; set; }
    }
}
