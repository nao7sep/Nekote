using System;
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

        // where 句で struct に限り、ジェネリックの型名を ValueType としていたが、Shuffle などは参照の配列にも役立つ
        // 今後、主に値型を扱う配列には ValueType/values を使い、参照もよく扱うなら ElementType/elements を使う

        public static bool Equals <ElementType> (ElementType [] elements1, int firstIndex1, ElementType [] elements2, int firstIndex2, int length)
        {
            // iArrayTester.CompareSpeedsOfComparingArrays の結果に基づき、実装を変更した
            return elements1.AsSpan (firstIndex1, length).SequenceEqual (elements2.AsSpan (firstIndex2, length));
        }
    }
}
