using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nInvariantCulture
    {
        public static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        public static readonly StringComparer Comparer = StringComparer.InvariantCulture;

        public static readonly StringComparer ComparerIgnoreCase = StringComparer.InvariantCultureIgnoreCase;

        /// <summary>
        /// [0] が January。
        /// </summary>
        public static readonly string [] MonthNames = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        // DayOfWeek Enum (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.dayofweek

        /// <summary>
        /// [0] が Sunday。
        /// </summary>
        public static readonly string [] DayOfWeekNames = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        // Ordinal Numbers - Meaning, Examples | What are Ordinal Numbers?
        // https://www.cuemath.com/numbers/ordinal-numbers/

        // 英語の序数1から1000までの表記と読み方一覧 - キーワードノート
        // https://kw-note.com/translation/english-ordinal-numbers/

        // 11～13が変則的なのは、100を超えても残る
        // たとえば111は one hundred and eleventh

        // 1, 2, 3, 4, 11, 12, 13, 14, 21, 22, 23, 24, 101, 102, 103, 104, 111, 112, 113, 114, 121, 122, 123, 124
        //     → 1st, 2nd, 3rd, 4th, 11th, 12th, 13th, 14th, 21st, 22nd, 23rd, 24th, 101st, 102nd, 103rd, 104th, 111th, 112th, 113th, 114th, 121st, 122nd, 123rd, 124th

        public static string GetOrdinalNumberString (int number)
        {
            if (number <= 0)
                throw new nArgumentException ();

            string xLastPart;

            int xLastDigit = number % 10,
                xLastTwoDigits = number % 100;

            if (xLastDigit == 1 && xLastTwoDigits != 11)
                xLastPart = "st";

            else if (xLastDigit == 2 && xLastTwoDigits != 12)
                xLastPart = "nd";

            else if (xLastDigit == 3 && xLastTwoDigits != 13)
                xLastPart = "rd";

            else xLastPart = "th";

            return FormattableString.Invariant ($"{number}{xLastPart}");
        }
    }
}
