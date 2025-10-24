namespace Nekote.Core.Text.Processing
{
    /// <summary>
    /// 「空行」と見なす条件を定義します。
    /// </summary>
    public enum EmptyLineDefinition
    {
        /// <summary>
        /// 長さが0（""）の行のみを空行と見なします。
        /// </summary>
        IsEmpty,

        /// <summary>
        /// null、長さが0、または空白文字のみで構成される行を空行と見なします。
        /// これは <see cref="string.IsNullOrWhiteSpace"/> と同等です。
        /// </summary>
        IsWhitespace,
    }
}
