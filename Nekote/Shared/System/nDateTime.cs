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
        public static string ToRoundtripString (DateTime value)
        {
            return value.ToString ("O");
        }

        public static DateTime ParseRoundtripString (string value)
        {
            return DateTime.Parse (value, null, DateTimeStyles.RoundtripKind);
        }

        public static bool TryParseRoundtripString (string value, out DateTime result)
        {
            return DateTime.TryParse (value, null, DateTimeStyles.RoundtripKind, out result);
        }
    }
}
