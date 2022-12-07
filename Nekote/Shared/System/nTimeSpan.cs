using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nTimeSpan
    {
        // iValueTypeTester.TestCalculatingAverageOfLongValues の結果に基づき、以下、decimal を積極的に使う
        // decimal から long に戻すときには、キャストの Math.Truncate 的な挙動による値の下振れを防ぐために decimal.Round を行う

        // Math.Truncate Method (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.math.truncate

        public static TimeSpan Average (IEnumerable <TimeSpan> values)
        {
            return new TimeSpan ((long) decimal.Round (values.Average (x => (decimal) x.Ticks)));
        }
    }
}
