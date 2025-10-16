namespace Nekote.Core.Text
{
    /// <summary>
    /// 文字列操作に関するヘルパーメソッドを提供します。
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// 指定された文字列が null または空の場合に null を返します。
        /// それ以外の場合は、元の文字列を返します。
        /// </summary>
        /// <param name="value">確認する文字列。</param>
        /// <returns>null または空の場合は null。それ以外の場合は元の文字列。</returns>
        public static string? NullIfEmpty(string? value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }

        /// <summary>
        /// 指定された文字列が null、空、または空白文字のみで構成されている場合に null を返します。
        /// それ以外の場合は、元の文字列を返します。
        /// </summary>
        /// <param name="value">確認する文字列。</param>
        /// <returns>null、空、または空白の場合は null。それ以外の場合は元の文字列。</returns>
        public static string? NullIfWhiteSpace(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
