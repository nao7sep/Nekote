namespace Nekote.Core.Operations
{
    /// <summary>
    /// 複数のアクション実行時のエラー処理方法を指定します。
    /// </summary>
    public enum ActionErrorHandling
    {
        /// <summary>
        /// 最初の例外で実行を停止します。
        /// </summary>
        /// <remarks>
        /// これはデフォルトの動作です。
        /// 例外は即座に呼び出し元に伝播されます。
        /// デバッグ時にはこの値を使用してください。
        /// </remarks>
        StopOnFirstException,

        /// <summary>
        /// すべてのアクションを実行し、発生したすべての例外を AggregateException にまとめて最後にスローします。
        /// </summary>
        /// <remarks>
        /// すべてのアクションの実行を試み、発生した例外をすべて収集します。
        /// 複数の処理先がある場合に、すべての失敗を確認したいときに使用します。
        /// </remarks>
        CollectAndThrowAll,

        /// <summary>
        /// すべての例外を捕捉して破棄し、実行を続行します。
        /// </summary>
        /// <remarks>
        /// 例外が発生しても無視され、すべてのアクションの実行が試みられます。
        /// 例外を完全に排除できない場合にのみ使用してください。
        /// </remarks>
        SuppressAll
    }
}
