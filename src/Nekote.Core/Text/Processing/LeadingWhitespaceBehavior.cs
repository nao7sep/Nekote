namespace Nekote.Core.Text.Processing
{
    /// <summary>
    /// 行頭の空白文字の処理方法を指定します。
    /// </summary>
    public enum LeadingWhitespaceBehavior
    {
        /// <summary>
        /// 行頭の空白をすべて保持します。
        /// </summary>
        Keep,

        /// <summary>
        /// 行頭の空白をすべて削除します。
        /// </summary>
        Trim,
    }
}
