using System;
using System.Globalization;

namespace Nekote.Core.Time
{
    /// <summary>
    /// 日付と時刻の操作に関する拡張メソッドを提供します。
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// 指定された書式を使用して、この <see cref="DateTimeOffset"/> のインスタンスの値を、それと等価な文字列形式に変換します。
        /// </summary>
        /// <param name="value">変換対象の <see cref="DateTimeOffset"/>。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>指定した書式による文字列形式。</returns>
        public static string ToString(this DateTimeOffset value, DateTimeFormatKind format)
        {
            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "The specified format is not a valid DateTimeFormatKind value.");
            }

            var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
            return value.ToString(dateTimeFormatString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="DateTimeOffset"/> に変換します。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>変換された <see cref="DateTimeOffset"/>。</returns>
        public static DateTimeOffset ParseDateTimeOffset(string value, DateTimeFormatKind format)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null, empty, or whitespace.", nameof(value));
            }

            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "The specified format is not a valid DateTimeFormatKind value.");
            }

            var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
            var dateTimeStyles = GetDateTimeStyles(format);
            return DateTimeOffset.ParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles);
        }

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="DateTimeOffset"/> に変換しようと試みます。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <param name="result">変換に成功した場合、変換された <see cref="DateTimeOffset"/> が格納されます。</param>
        /// <returns>変換に成功した場合は true、それ以外は false。</returns>
        public static bool TryParseDateTimeOffset(string value, DateTimeFormatKind format, out DateTimeOffset result)
        {
            if (string.IsNullOrWhiteSpace(value) || !Enum.IsDefined<DateTimeFormatKind>(format))
            {
                result = default;
                return false;
            }

            var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
            var dateTimeStyles = GetDateTimeStyles(format);
            return DateTimeOffset.TryParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles, out result);
        }

        /// <summary>
        /// 指定された書式を使用して、この <see cref="DateTime"/> のインスタンスの値を、それと等価な文字列形式に変換します。
        /// </summary>
        /// <param name="value">変換対象の <see cref="DateTime"/>。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>指定した書式による文字列形式。</returns>
        public static string ToString(this DateTime value, DateTimeFormatKind format)
        {
            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "The specified format is not a valid DateTimeFormatKind value.");
            }

            var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
            return value.ToString(dateTimeFormatString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="DateTime"/> に変換します。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>変換された <see cref="DateTime"/>。</returns>
        public static DateTime ParseDateTime(string value, DateTimeFormatKind format)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null, empty, or whitespace.", nameof(value));
            }

            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "The specified format is not a valid DateTimeFormatKind value.");
            }

            var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
            var dateTimeStyles = GetDateTimeStyles(format);
            return DateTime.ParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles);
        }

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="DateTime"/> に変換しようと試みます。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <param name="result">変換に成功した場合、変換された <see cref="DateTime"/> が格納されます。</param>
        /// <returns>変換に成功した場合は true、それ以外は false。</returns>
        public static bool TryParseDateTime(string value, DateTimeFormatKind format, out DateTime result)
        {
            if (string.IsNullOrWhiteSpace(value) || !Enum.IsDefined<DateTimeFormatKind>(format))
            {
                result = default;
                return false;
            }

            var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
            var dateTimeStyles = GetDateTimeStyles(format);
            return DateTime.TryParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles, out result);
        }

        /// <summary>
        /// 指定された書式を使用して、この <see cref="DateOnly"/> のインスタンスの値を、それと等価な文字列形式に変換します。
        /// </summary>
        /// <param name="value">変換対象の <see cref="DateOnly"/>。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>指定した書式による文字列形式。</returns>
        public static string ToString(this DateOnly value, DateTimeFormatKind format)
        {
            switch (format)
            {
                case DateTimeFormatKind.DateSortable:
                case DateTimeFormatKind.DateUserFriendly:
                    var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
                    return value.ToString(dateTimeFormatString, CultureInfo.InvariantCulture);

                default:
                    throw new ArgumentException($"The format '{format}' is not valid for DateOnly. Only DateSortable and DateUserFriendly formats are supported.", nameof(format));
            }
        }

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="DateOnly"/> に変換します。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>変換された <see cref="DateOnly"/>。</returns>
        public static DateOnly ParseDateOnly(string value, DateTimeFormatKind format)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null, empty, or whitespace.", nameof(value));
            }

            switch (format)
            {
                case DateTimeFormatKind.DateSortable:
                case DateTimeFormatKind.DateUserFriendly:
                    var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
                    var dateTimeStyles = GetDateTimeStyles(format);
                    return DateOnly.ParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles);

                default:
                    throw new ArgumentException($"The format '{format}' is not valid for DateOnly. Only DateSortable and DateUserFriendly formats are supported.", nameof(format));
            }
        }

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="DateOnly"/> に変換しようと試みます。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <param name="result">変換に成功した場合、変換された <see cref="DateOnly"/> が格納されます。</param>
        /// <returns>変換に成功した場合は true、それ以外は false。</returns>
        public static bool TryParseDateOnly(string value, DateTimeFormatKind format, out DateOnly result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default;
                return false;
            }

            switch (format)
            {
                case DateTimeFormatKind.DateSortable:
                case DateTimeFormatKind.DateUserFriendly:
                    var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
                    var dateTimeStyles = GetDateTimeStyles(format);
                    return DateOnly.TryParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles, out result);

                default:
                    result = default;
                    return false;
            }
        }

        /// <summary>
        /// 指定された書式を使用して、この <see cref="TimeOnly"/> のインスタンスの値を、それと等価な文字列形式に変換します。
        /// </summary>
        /// <param name="value">変換対象の <see cref="TimeOnly"/>。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>指定した書式による文字列形式。</returns>
        public static string ToString(this TimeOnly value, DateTimeFormatKind format)
        {
            switch (format)
            {
                case DateTimeFormatKind.TimeSortable:
                case DateTimeFormatKind.TimeSortableMilliseconds:
                case DateTimeFormatKind.TimeSortableTicks:
                case DateTimeFormatKind.TimeUserFriendlyMinutes:
                case DateTimeFormatKind.TimeUserFriendlySeconds:
                case DateTimeFormatKind.TimeUserFriendlyMilliseconds:
                case DateTimeFormatKind.TimeUserFriendlyTicks:
                    var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
                    return value.ToString(dateTimeFormatString, CultureInfo.InvariantCulture);

                default:
                    throw new ArgumentException($"The format '{format}' is not valid for TimeOnly. Only time-related formats are supported.", nameof(format));
            }
        }

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="TimeOnly"/> に変換します。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>変換された <see cref="TimeOnly"/>。</returns>
        public static TimeOnly ParseTimeOnly(string value, DateTimeFormatKind format)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null, empty, or whitespace.", nameof(value));
            }

            switch (format)
            {
                case DateTimeFormatKind.TimeSortable:
                case DateTimeFormatKind.TimeSortableMilliseconds:
                case DateTimeFormatKind.TimeSortableTicks:
                case DateTimeFormatKind.TimeUserFriendlyMinutes:
                case DateTimeFormatKind.TimeUserFriendlySeconds:
                case DateTimeFormatKind.TimeUserFriendlyMilliseconds:
                case DateTimeFormatKind.TimeUserFriendlyTicks:
                    var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
                    var dateTimeStyles = GetDateTimeStyles(format);
                    return TimeOnly.ParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles);

                default:
                    throw new ArgumentException($"The format '{format}' is not valid for TimeOnly. Only time-related formats are supported.", nameof(format));
            }
        }

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="TimeOnly"/> に変換しようと試みます。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <param name="result">変換に成功した場合、変換された <see cref="TimeOnly"/> が格納されます。</param>
        /// <returns>変換に成功した場合は true、それ以外は false。</returns>
        public static bool TryParseTimeOnly(string value, DateTimeFormatKind format, out TimeOnly result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default;
                return false;
            }

            switch (format)
            {
                case DateTimeFormatKind.TimeSortable:
                case DateTimeFormatKind.TimeSortableMilliseconds:
                case DateTimeFormatKind.TimeSortableTicks:
                case DateTimeFormatKind.TimeUserFriendlyMinutes:
                case DateTimeFormatKind.TimeUserFriendlySeconds:
                case DateTimeFormatKind.TimeUserFriendlyMilliseconds:
                case DateTimeFormatKind.TimeUserFriendlyTicks:
                    var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
                    var dateTimeStyles = GetDateTimeStyles(format);
                    return TimeOnly.TryParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles, out result);

                default:
                    result = default;
                    return false;
            }
        }

        /// <summary>
        /// 指定された <see cref="DateTimeFormatKind"/> に応じた <see cref="DateTimeStyles"/> を返します。
        /// </summary>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>対応する <see cref="DateTimeStyles"/>。</returns>
        private static DateTimeStyles GetDateTimeStyles(DateTimeFormatKind format) => format switch
        {
            // 詳細: `AssumeUniversal` と `AdjustToUniversal` の併用に関する経緯と技術的背景
            //
            // 1. 経緯:
            // `AdjustToUniversal` の必要性を検証するため、このフラグを除外してテストを実施したところ、
            // 全てのUTC関連書式（例: "yyyyMMddTHHmmssZ"）のパースが失敗しました。
            // 具体的には、JST（+09:00）環境において、期待されるUTC時刻より9時間進んだローカル時刻が返されました。
            // この結果から、`AdjustToUniversal` が意図した動作に不可欠であることが確認されました。
            //
            // 2. 技術的背景:
            // .NETの `DateTime.ParseExact` メソッドは、デフォルトでシステムのローカルタイムゾーンを強く意識します。
            // 公式ドキュメントにも「タイムゾーン情報が文字列に含まれない場合、OSのタイムゾーン設定に基づいて解釈する」
            // と記載されています。
            // 参照: https://learn.microsoft.com/en-us/dotnet/api/system.globalization.datetimestyles?view=net-9.0
            //
            // この動作は、UTC指示子（'Z'など）を含む文字列をパースする際にも影響します。
            // パーサーは文字列をUTCとして認識しますが、最終的に返す `DateTime` オブジェクトを
            // 自動的にローカル時刻に変換します。これが前述の9時間のずれの原因です。
            //
            // 3. 解決策としてのフラグの役割:
            // - `AssumeUniversal`: このフラグは、文字列にタイムゾーン指示子がない場合でも、UTCとして解釈するよう指示します。
            // - `AdjustToUniversal`: このフラグは、上記（2.）で発生する自動的なローカル時刻への変換を「元に戻し」、
            //   `DateTime` オブジェクトが純粋なUTC値（`Kind=Utc`）として返されることを強制します。
            //
            // 結論として、この2つのフラグの組み合わせは、`DateTime` の既定の動作を上書きし、
            // 環境に依存しない一貫したUTCパースを実現するために必須となります。

            // --- 並べ替え可能な書式 ---

            DateTimeFormatKind.LocalSortable or
            DateTimeFormatKind.LocalSortableMilliseconds or
            DateTimeFormatKind.LocalSortableTicks
                => DateTimeStyles.AssumeLocal,

            DateTimeFormatKind.UtcSortable or
            DateTimeFormatKind.UtcSortableMilliseconds or
            DateTimeFormatKind.UtcSortableTicks
                => DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,

            // --- 日付のみ・時刻のみの書式 ---

            DateTimeFormatKind.DateSortable or
            DateTimeFormatKind.TimeSortable or
            DateTimeFormatKind.TimeSortableMilliseconds or
            DateTimeFormatKind.TimeSortableTicks
                => DateTimeStyles.None,

            // --- 人間が読みやすい書式 ---

            DateTimeFormatKind.LocalUserFriendlyMinutes or
            DateTimeFormatKind.LocalUserFriendlySeconds or
            DateTimeFormatKind.LocalUserFriendlyMilliseconds or
            DateTimeFormatKind.LocalUserFriendlyTicks
                => DateTimeStyles.AssumeLocal,

            DateTimeFormatKind.UtcUserFriendlyMinutes or
            DateTimeFormatKind.UtcUserFriendlySeconds or
            DateTimeFormatKind.UtcUserFriendlyMilliseconds or
            DateTimeFormatKind.UtcUserFriendlyTicks
                => DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,

            // --- 日付・時刻の人間が読みやすい書式 ---

            DateTimeFormatKind.DateUserFriendly or
            DateTimeFormatKind.TimeUserFriendlyMinutes or
            DateTimeFormatKind.TimeUserFriendlySeconds or
            DateTimeFormatKind.TimeUserFriendlyMilliseconds or
            DateTimeFormatKind.TimeUserFriendlyTicks
                => DateTimeStyles.None,

            _ => throw new ArgumentException($"The specified format is not valid for this operation.", nameof(format))
        };
    }
}
