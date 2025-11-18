namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "prediction" 内の "content" が単純な文字列の場合の具象 DTO。
    /// </summary>
    internal class OpenAiChatPredictionContentStringDto : OpenAiChatPredictionContentBaseDto
    {
        /// <summary>
        /// 予測されるテキスト内容。
        /// </summary>
        public string? Text { get; set; }
    }
}
