namespace Nekote.Core.Text.Processing
{
    /// <summary>
    /// 行中の空白文字の処理方法を指定します。
    /// </summary>
    public enum InternalWhitespaceBehavior
    {
        /// <summary>
        /// 行中の空白をすべて保持します。
        /// </summary>
        Keep,

        /// <summary>
        /// 連続する空白を1つのスペースにまとめます。
        /// </summary>
        CollapseToOneSpace,
    }
}
