using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace ConsoleTester
{
    internal static class iTester
    {
        // 1次元目に各テスト、2次元目に所要時間が入っている nMultiArray を想定
        // 決め打ちの仕様になるが、今後も同様のテストを行うだろうから、コードを共通化

        public static string FormatLabelsAndElapsedTimes (string [] labels, nMultiArray <TimeSpan> elapsed)
        {
            // <TimeSpan> なので、y.Value は Nullable でない
            // ElementType? Value としての宣言だが、値型だとうまく処理される
            return string.Join (Environment.NewLine, Enumerable.Range (0, labels.Length).Select (x => $"{labels [x]}: {nTimeSpan.Average (elapsed.GetSubarray (x).Subarrays.Select (y => y.Value)).TotalMilliseconds}ms"));
        }
    }
}
