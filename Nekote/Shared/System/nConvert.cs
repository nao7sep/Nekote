using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nConvert
    {
        public static string? VersionToString (Version version, int maxFieldCount)
        {
            // 負だと Math.Min も負になるので一応
            // 大きすぎるのは影響がない

            if (maxFieldCount < 0)
                throw new nArgumentException ();

            int xFieldCount = 0;

            if (version.Revision > 0)
                xFieldCount = 4;

            else if (version.Build > 0)
                xFieldCount = 3;

            else if (version.Minor > 0)
                xFieldCount = 2;

            else if (version.Major > 0)
                xFieldCount = 1;

            return Math.Min (xFieldCount, maxFieldCount) switch
            {
                4 => FormattableString.Invariant ($"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}"),
                3 => FormattableString.Invariant ($"{version.Major}.{version.Minor}.{version.Build}"),
                2 => FormattableString.Invariant ($"{version.Major}.{version.Minor}"),
                1 => FormattableString.Invariant ($"{version.Major}"),
                _ => null // 0 以外にならない
            };
        }

        public static string DateTimeToRoundtripString (DateTime value)
        {
            return value.ToString ("O");
        }

        public static DateTime ParseRoundtripDateTimeString (string value)
        {
            return DateTime.Parse (value, null, DateTimeStyles.RoundtripKind);
        }

        public static bool TryParseRoundtripDateTimeString (string value, out DateTime result)
        {
            return DateTime.TryParse (value, null, DateTimeStyles.RoundtripKind, out result);
        }
    }
}
