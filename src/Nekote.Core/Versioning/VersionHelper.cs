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
        /// <param name="minimumFieldCount">出力に含める最小フィールド数。2から4の間でなければなりません。既定値は 2 です。</param>
        /// <returns>バージョン番号の文字列表現。</returns>
        /// <exception cref="ArgumentNullException">version が null の場合にスローされます。</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumFieldCount が 2 未満または 4 を超える場合にスローされます。</exception>
        /// <remarks>
        /// このメソッドの設計思想：
        /// - メジャーとマイナーバージョンは常に表示されるべきです（例：「1.0」は意味のあるバージョン文字列ですが、「1」だけでは不十分）
        /// - 「0.0.0.0」のような入力では、significantFieldCount は 0 になりますが、minimumFieldCount により「0.0」として表示されます
        /// - セマンティックバージョニングに従い、ビルド番号が頻繁に更新される場合は minimumFieldCount を 3 に設定することを推奨します
        /// - 現在の実装では、リリース時にマイナーバージョンのみを更新し、「0.1」を最初のバージョンとして使用します
        /// </remarks>
        public static string ToString(Version version, int minimumFieldCount = 2)
        {
            ArgumentNullException.ThrowIfNull(version);
            ArgumentOutOfRangeException.ThrowIfLessThan(minimumFieldCount, 2);
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
