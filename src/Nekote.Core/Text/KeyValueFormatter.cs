namespace Nekote.Core.Text
{
    /// <summary>
    /// キーと値のペアをフォーマットするためのクラスです。
    /// 値が空かどうかに応じて、適切なセパレーターを選択します。
    /// </summary>
    public sealed class KeyValueFormatter
    {
        private readonly string _separatorWhenValueHasContent;
        private readonly string _separatorWhenValueIsEmpty;

        /// <summary>
        /// 既定の <see cref="KeyValueFormatter"/> インスタンスを取得します。
        /// 値に内容がある場合は ": " を、値が空の場合は ":" を使用します。
        /// </summary>
        public static KeyValueFormatter Default { get; } = new KeyValueFormatter(": ", ":");

        /// <summary>
        /// <see cref="KeyValueFormatter"/> の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="separatorWhenValueHasContent">値に内容がある場合に使用するセパレーター。</param>
        /// <param name="separatorWhenValueIsEmpty">値が空の場合に使用するセパレーター。</param>
        public KeyValueFormatter(
            string separatorWhenValueHasContent,
            string separatorWhenValueIsEmpty)
        {
            _separatorWhenValueHasContent = separatorWhenValueHasContent;
            _separatorWhenValueIsEmpty = separatorWhenValueIsEmpty;
        }

        /// <summary>
        /// 値に内容がある場合に使用するセパレーターを取得します。
        /// </summary>
        public string SeparatorWhenValueHasContent => _separatorWhenValueHasContent;

        /// <summary>
        /// 値が空の場合に使用するセパレーターを取得します。
        /// </summary>
        public string SeparatorWhenValueIsEmpty => _separatorWhenValueIsEmpty;

        /// <summary>
        /// 値の内容に基づいて適切なセパレーターを取得します。
        /// </summary>
        /// <param name="value">評価する値。</param>
        /// <returns>値に内容がある場合は <see cref="SeparatorWhenValueHasContent"/>、
        /// そうでない場合は <see cref="SeparatorWhenValueIsEmpty"/>。</returns>
        public string GetSeparator(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? _separatorWhenValueIsEmpty
                : _separatorWhenValueHasContent;
        }

        /// <summary>
        /// キーと値を適切なセパレーターで結合します。
        /// </summary>
        /// <param name="key">キー。</param>
        /// <param name="value">値。</param>
        /// <returns>フォーマットされたキーと値のペア。</returns>
        public string Format(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return key + _separatorWhenValueIsEmpty;
            }
            else
            {
                return key + _separatorWhenValueHasContent + value;
            }
        }
    }
}
