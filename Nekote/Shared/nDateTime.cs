using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nDateTime
    {
        public static string ToRoundtripString (this DateTime value)
        {
            return value.ToString ("O", CultureInfo.InvariantCulture);
        }

        public static DateTime ParseRoundtripString (string value)
        {
            return DateTime.ParseExact (value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        public static bool TryParseRoundtripString (string value, out DateTime result)
        {
            return DateTime.TryParseExact (value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result);
        }

        // =============================================================================

        public static string ToRfc1123String (this DateTime value)
        {
            return value.ToString ("R", CultureInfo.InvariantCulture);
        }

        public static DateTime ParseRfc1123String (string value)
        {
            return DateTime.ParseExact (value, "R", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static bool TryParseRfc1123String (string value, out DateTime result)
        {
            return DateTime.TryParseExact (value, "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        // =============================================================================

        // システムを作り込むなら、もちろん日時のフォーマットもロケールのファイルで指定できるようにするなどを考えるが、
        //     日本語または英語でちょっとしたツールを書くときには、誰も困らない決め打ちのフォーマットもあってよい
        // 使う記号については、iTimeTester.AnalyzeDateAndTimeSeparators が詳しい

        public static readonly string FriendlyDateTimeStringFormat = "yyyy'/'M'/'d H':'mm':'ss";

        public static string ToFriendlyDateTimeString (this DateTime value)
        {
            return value.ToString (FriendlyDateTimeStringFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime ParseFriendlyDateTimeString (string value)
        {
            return DateTime.ParseExact (value, FriendlyDateTimeStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static bool TryParseFriendlyDateTimeString (string value, out DateTime result)
        {
            return DateTime.TryParseExact (value, FriendlyDateTimeStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        // =============================================================================

        public static readonly string FriendlyDateTimeShortStringFormat = "yyyy'/'M'/'d H':'mm";

        public static string ToFriendlyDateTimeShortString (this DateTime value)
        {
            return value.ToString (FriendlyDateTimeShortStringFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime ParseFriendlyDateTimeShortString (string value)
        {
            return DateTime.ParseExact (value, FriendlyDateTimeShortStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static bool TryParseFriendlyDateTimeShortString (string value, out DateTime result)
        {
            return DateTime.TryParseExact (value, FriendlyDateTimeShortStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        // =============================================================================

        public static readonly string FriendlyDateStringFormat = "yyyy'/'M'/'d";

        public static string ToFriendlyDateString (this DateTime value)
        {
            return value.ToString (FriendlyDateStringFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime ParseFriendlyDateString (string value)
        {
            return DateTime.ParseExact (value, FriendlyDateStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static bool TryParseFriendlyDateString (string value, out DateTime result)
        {
            return DateTime.TryParseExact (value, FriendlyDateStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        // =============================================================================

        public static readonly string FriendlyTimeStringFormat = "H':'mm':'ss";

        public static string ToFriendlyTimeString (this DateTime value)
        {
            return value.ToString (FriendlyTimeStringFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime ParseFriendlyTimeString (string value)
        {
            return DateTime.ParseExact (value, FriendlyTimeStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static bool TryParseFriendlyTimeString (string value, out DateTime result)
        {
            return DateTime.TryParseExact (value, FriendlyTimeStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        // =============================================================================

        public static readonly string FriendlyTimeShortStringFormat = "H':'mm";

        public static string ToFriendlyTimeShortString (this DateTime value)
        {
            return value.ToString (FriendlyTimeShortStringFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime ParseFriendlyTimeShortString (string value)
        {
            return DateTime.ParseExact (value, FriendlyTimeShortStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static bool TryParseFriendlyTimeShortString (string value, out DateTime result)
        {
            return DateTime.TryParseExact (value, FriendlyTimeShortStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        // =============================================================================

        // 次のページの右上に表示される 20221227T032315Z のようなもの
        // T で日付と時刻を区切り、Z により UTC であることを示す

        // ISO 8601 - Wikipedia
        // https://en.wikipedia.org/wiki/ISO_8601

        // 日付と時刻が全く区切られないのでは、桁を数えて読むことになり、人間による可読性が下がる
        // URL に使いやすい区切り文字では - も考えられるが、20221227-032315Z というのはバランスが悪い
        // そもそも、ISO 8601 の例から外れることで、長いものに巻かれない独自仕様になってくる

        public static readonly string MinimalUniversalDateTimeStringFormat = "yyyyMMdd'T'HHmmss'Z'";

        public static string ToMinimalUniversalDateTimeString (this DateTime value)
        {
            return value.ToString (MinimalUniversalDateTimeStringFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime ParseMinimalUniversalDateTimeString (string value)
        {
            return DateTime.ParseExact (value, MinimalUniversalDateTimeStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static bool TryParseMinimalUniversalDateTimeString (string value, out DateTime result)
        {
            return DateTime.TryParseExact (value, MinimalUniversalDateTimeStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        // =============================================================================

        // 20221227T032315 というのは、あまり美しくない
        // ローカル日時を扱うにおいて末尾の Z を落とすなら、20221227-032315Z が、地味だが文句の付けにくい仕様
        // 区切り文字としては、/ : - . _ あたりが考えられる
        // / と : は Windows のファイル名に使えず、. は拡張子に使われる
        // - と _ では、Google が URL での使用を推奨しているのが -

        // Consider using hyphens to separate words in your URLs, as it helps users and search engines identify concepts in the URL more easily
        //     We recommend that you use hyphens (-) instead of underscores (_) in your URLs とのこと

        // Google URL Structure Guidelines | Google Search Central  |  Documentation  |  Google Developers
        // https://developers.google.com/search/docs/crawling-indexing/url-structure

        public static readonly string MinimalLocalDateTimeStringFormat = "yyyyMMdd'-'HHmmss";

        public static string ToMinimalLocalDateTimeString (this DateTime value)
        {
            return value.ToString (MinimalLocalDateTimeStringFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime ParseMinimalLocalDateTimeString (string value)
        {
            return DateTime.ParseExact (value, MinimalLocalDateTimeStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static bool TryParseMinimalLocalDateTimeString (string value, out DateTime result)
        {
            return DateTime.TryParseExact (value, MinimalLocalDateTimeStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }
    }
}
