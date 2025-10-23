namespace Nekote.Core.Text.Processing
{
    /// <summary>
    /// テキストの先頭にある連続した空行の処理方法を指定します。
    /// </summary>
    public enum LeadingEmptyLineHandling
    {
        /// <summary>
        /// 先頭の空行をすべて保持します。
        /// </summary>        
        Keep,

        /// <summary>
        /// 先頭の空行をすべて無視します。
        /// </summary>
        Ignore,
    }
}
