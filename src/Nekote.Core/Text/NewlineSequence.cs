namespace Nekote.Core.Text
{
    /// <summary>
    /// 段落内の行を結合するために使用する改行シーケンスを指定します。
    /// </summary>
    public enum NewlineSequence
    {
        /// <summary>
        /// 単一のラインフィード（LF、"\n"）を使用します。
        /// </summary>
        Lf,

        /// <summary>
        /// キャリッジリターンとラインフィード（CRLF、"\r\n"）を使用します。
        /// </summary>
        CrLf,

        /// <summary>
        /// 現在のプラットフォームのデフォルト（<see cref="System.Environment.NewLine"/>）を使用します。
        /// </summary>
        PlatformDefault
    }
}
