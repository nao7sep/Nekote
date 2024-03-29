﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoteConsole
{
    // int など、ValueType を継承する構造体のテストに関するメソッドを集めていく

    // nValue を追加したので iValueTester への改名を考えたが、やめておく
    // 後ろに Tester が続く場合、前が ValueType だと「型」に起因することのテストとのニュアンスになり、
    //     Value だけだと「値」をテストすると
    // nValue の方は、扱うものが「値」なのだから、nValueType は適さない
    // どちらも最善の名前になっていて、何となく整合させることはない

    internal static class iValueTypeTester
    {
        // Mac での動作を確認した

        public static void CalculateAverageOfLongValues ()
        {
            // TimeSpan の平均の算出のために最初に書いたコード
            // 05:30:00 が出力される
            Console.WriteLine (new TimeSpan ((long) Math.Round (Enumerable.Range (1, 10).Select (x => new TimeSpan (hours: x, 0, 0)).Average (x => x.Ticks))));

            // long.MaxVae およびそれに近い値の平均を計算
            // OverflowException が投げられる
            // Console.WriteLine (Enumerable.Range (0, 3).Select (x => long.MaxValue - x).Average (x => x));

            // decimal にキャストすれば大丈夫
            // ちゃんと long.MaxValue - 1 の値が表示される

            // 128ビットのうち96ビットが整数とのことで、long の64ビットを超える
            // long.MaxValue に近い多数の long の値の平均も計算できる

            // TimeSpan の平均を取るにおいては、Round を使用
            // Math.Round と decimal.Round の二つがあり、前者が後者を呼ぶ

            // Decimal Struct (System) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.decimal

            // Math.cs
            // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Math.cs

            // Decimal.cs
            // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Decimal.cs

            Console.WriteLine (Enumerable.Range (0, 3).Select (x => long.MaxValue - x).Average (x => (decimal) x));
            Console.WriteLine (long.MaxValue);
        }
    }
}
