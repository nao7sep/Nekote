using System.Text;

namespace Nekote.Core.Text
{
    /// <summary>
    /// 文字列構築のためのヘルパーメソッドを提供します。
    /// </summary>
    public static class StringBuilderHelper
    {
        /// <summary>
        /// 値が null、空文字列、または空白のみでない場合に、キーと値のペアを追加します。
        /// </summary>
        /// <param name="builder">StringBuilder インスタンス。</param>
        /// <param name="key">キー。</param>
        /// <param name="value">値。</param>
        /// <param name="prefix">値の前に追加するプレフィックス (デフォルト: ": ")。</param>
        /// <param name="suffix">値の後に追加するサフィックス (デフォルト: ", ")。</param>
        /// <returns>値が追加された場合は true、それ以外は false。</returns>
        public static bool AppendIfNotEmpty(
            this StringBuilder builder,
            string key,
            string? value,
            string prefix = ": ",
            string suffix = ", ")
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            builder.Append(key);
            builder.Append(prefix);
            builder.Append(value);
            builder.Append(suffix);

            return true;
        }

        /// <summary>
        /// 値が null、空文字列、または空白のみでない場合に、キーと値のペアを行として追加します。
        /// </summary>
        /// <param name="builder">StringBuilder インスタンス。</param>
        /// <param name="key">キー。</param>
        /// <param name="value">値。</param>
        /// <param name="indent">インデント文字列 (デフォルト: "  ")。</param>
        /// <param name="prefix">値の前に追加するプレフィックス (デフォルト: ": ")。</param>
        /// <returns>値が追加された場合は true、それ以外は false。</returns>
        public static bool AppendLineIfNotEmpty(
            this StringBuilder builder,
            string key,
            string? value,
            string indent = "  ",
            string prefix = ": ")
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            builder.Append(indent);
            builder.Append(key);
            builder.Append(prefix);
            builder.AppendLine(value);

            return true;
        }

        /// <summary>
        /// 複数のキーと値のペアを追加し、最後のサフィックスを削除します。
        /// </summary>
        /// <param name="builder">StringBuilder インスタンス。</param>
        /// <param name="suffix">削除するサフィックス (デフォルト: ", ")。</param>
        public static void RemoveTrailingSuffix(this StringBuilder builder, string suffix = ", ")
        {
            if (builder.Length >= suffix.Length)
            {
                var actualEnd = builder.ToString(builder.Length - suffix.Length, suffix.Length);
                if (actualEnd == suffix)
                {
                    builder.Length -= suffix.Length;
                }
            }
        }
    }
}
