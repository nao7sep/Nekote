namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "prediction" 内の "content" がコンテンツパーツの配列の場合の具象 DTO。
    /// </summary>
    public class OpenAiChatPredictionContentArrayDto : OpenAiChatPredictionContentBaseDto
    {
        /// <summary>
        /// コンテンツパーツのリスト。
        /// </summary>
        public List<OpenAiChatPredictionContentPartDto>? Parts { get; set; }
    }
}
