namespace Nekote.Core.Text.Processing
{
    /// <summary>
    /// コンテンツの行間にある空行の処理方法を指定します。
    /// </summary>
    public enum InterstitialEmptyLineHandling
    {
        /// <summary>
        /// 行間の空行をすべて保持します。
        /// </summary>
        Keep,

        /// <summary>
        /// 連続する空行を1行にまとめます。
        /// </summary>
        CollapseToOne,

        /// <summary>
        /// 行間の空行をすべて無視します。
        /// </summary>
        Ignore,
    }
}
