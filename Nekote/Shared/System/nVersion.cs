using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nVersion
    {
        // 処理が Version.ToString と似ているため、名前を異ならせずに Ex を付けた
        // 今後、「何を生成するか」が特徴的なら、To*String という命名を検討

        /// <summary>
        /// maxFieldCount は inclusive。
        /// </summary>
        public static string? ToStringEx (this Version version, int maxFieldCount)
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
    }
}
