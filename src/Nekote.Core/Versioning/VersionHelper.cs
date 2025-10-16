using System;

namespace Nekote.Core.Versioning
{
    /// <summary>
    /// バージョン情報を扱うためのヘルパークラスです。
    /// </summary>
    public static class VersionHelper
    {
        /// <summary>
        /// バージョン情報を、指定された最小フィールド数と、0より大きい最下位のフィールドに基づいて文字列に変換します。
        /// </summary>
        /// <param name="version">変換する System.Version オブジェクト。</param>
        /// <param name="minimumFieldCount">出力に含める最小フィールド数。1から4の間でなければなりません。既定値は 2 です。</param>
        /// <returns>バージョン番号の文字列表現。</returns>
        /// <exception cref="ArgumentNullException">version が null の場合にスローされます。</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumFieldCount が 1 未満または 4 を超える場合にスローされます。</exception>
        /// <example>
        /// <code>
        /// ToString(new Version(1, 0, 0, 0), 2) // "1.0"
        /// ToString(new Version(0, 1, 2, 0), 2) // "0.1.2"
        /// ToString(new Version(1, 2, 0, 0), 3) // "1.2.0"
        /// </code>
        /// </example>
        public static string ToString(Version version, int minimumFieldCount = 2)
        {
            ArgumentNullException.ThrowIfNull(version);
            ArgumentOutOfRangeException.ThrowIfLessThan(minimumFieldCount, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(minimumFieldCount, 4);

            var significantFieldCount = version switch
            {
                { Revision: > 0 } => 4,
                { Build: > 0 } => 3,
                { Minor: > 0 } => 2,
                { Major: > 0 } => 1,
                _ => 0
            };

            return version.ToString(Math.Max(significantFieldCount, minimumFieldCount));
        }
    }
}
