namespace Nekote.Core.Text.Processing
{
    /// <summary>
    /// テキストの末尾にある連続した空行の処理方法を指定します。
    /// </summary>
    public enum TrailingEmptyLineHandling
    {
        /// <summary>
        /// 末尾の空行をすべて保持します。
        /// </summary>
        Keep,

        /// <summary>
        /// 末尾の空行をすべて無視します。
        /// </summary>
        Ignore,
    }
}
