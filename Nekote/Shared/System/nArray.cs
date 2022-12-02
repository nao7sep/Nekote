﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nArray
    {
        // 配列を扱うにおいて、.NET の機能だけでは、簡単なことが意外とできなかったり、複数の方法があって速度が大きく異なったりがある
        // 以下、最速を狙うわけでないがそう遅くもない実装を揃えていく

        // 多重定義地獄にしない
        // たとえば配列の firstIndex と length を配列から自動設定するなど
        // このクラスのメソッドは、どうしても配列をさわらないといけないところで稀に使われるもの
        // 使用頻度が低いため、多重定義を用意するコストに利益が見合わない

        // 個人的な好みにより、引数の変更を避ける
        // 大きなメソッドの実装において、後半でまた引数から「元の値」を取りたいことがある
        // 引数は不変だとみなせる方が、チェックするべきことが減る

        public static bool Equals <ValueType> (ValueType [] values1, int firstIndex1, ValueType [] values2, int firstIndex2, int length)
        {
            int xFirstIndex1 = firstIndex1,
                xFirstIndex2 = firstIndex2,
                xLastIndex1 = firstIndex1 + length - 1;

            var xComparer = EqualityComparer <ValueType>.Default;

            while (xFirstIndex1 <= xLastIndex1)
            {
                if (xComparer.Equals (values1 [xFirstIndex1 ++], values2 [xFirstIndex2 ++]) == false)
                    return false;
            }

            return true;
        }
    }
}