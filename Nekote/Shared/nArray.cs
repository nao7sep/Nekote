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

        // MemoryExtensions.SequenceEqual は、四つあるうち二つに where 句で ? 付きの IEquatable <...>? が指定されていて、
        //     残る二つは必要に応じて EqualityComparer <T>.Default を使う
        // MemoryExtensions.SequenceCompareTo は、二つあるうち二つともに IComparable <...>? が指定されている
        // string, TimeSpan, int など、主要な型が両方のインターフェースを実装しているため、Equals にも where 句を付けた

        // MemoryExtensions.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/MemoryExtensions.cs

        // いずれも MemoryExtensions のメソッドを呼ぶだけなので、なくてもよいかもしれない
        // しかし、IndexOf* などと比べて、これら二つは、BOM の判別など、ちょっとしたところでパッと使いたいことがある
        // 昔は遅すぎて避けた Enumerable.SequenceEqual が今では最速クラスであるなど、.NET 側の仕様変更に振り回されることも避けたい
        // といった点において、これらについては、配列における「常に最善の実装が保証されている演算子」のようなものとみなしている

        public static bool Equals <ElementType> (ElementType [] elements1, int firstIndex1, ElementType [] elements2, int firstIndex2, int length)
            where ElementType: IEquatable <ElementType>
        {
            // iArrayTester.CompareComparisonSpeeds の結果に基づき、実装を変更した
            return MemoryExtensions.SequenceEqual (elements1.AsSpan (firstIndex1, length), elements2.AsSpan (firstIndex2, length));
        }

        public static int Compare <ElementType> (ElementType [] elements1, int firstIndex1, int length1, ElementType [] elements2, int firstIndex2, int length2)
            where ElementType: IComparable <ElementType>
        {
            return MemoryExtensions.SequenceCompareTo (elements1.AsSpan (firstIndex1, length1), elements2.AsSpan (firstIndex2, length2));
        }

        // 以下、Array にも MemoryExtensions にもないものを中心に実装していく
        // 実装しないものについても、どれを使うべきかくらいは書いておく

        // Array Class (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.array

        // MemoryExtensions Class (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.memoryextensions

        // Contains, ContainsAny, ContainsAnyExcept, IndexOf, IndexOfAny, IndexOfAnyExcept, LastIndexOf, LastIndexOfAny, LastIndexOfAnyExcept を実装しない
        // MemoryExtensions に *IndexOf* が揃っていて、Contains* は IndexOf* で代用できる → 見落としたようだが、Contains も用意されている
        // .NET 4 の頃の知識で実装を考えたが、Core 2.1 で実装されたようである MemoryExtensions で足りると知った

        // Copy を実装しない → やはり実装する
        // Array.Copy で足りる

        // Copy を実装するのは、個人的に、「左に右を書き込む」という引数の順序が好きだから
        // たとえば Array.IndexOf は T [] array, T value となっていて、大きい左から小さい右を探す
        // また、File.Write* では、左にファイルパス、右に内容で、ここでも左に右を書き込んでいる
        // Copy も、大きい左に小さい右を書き込むイメージの方が自分には分かりやすい

        public static void Copy <ElementType> (ElementType [] destElements, int destFirstIndex, ElementType [] sourceElements, int sourceFirstIndex, int length)
        {
            Array.Copy (sourceElements, sourceFirstIndex, destElements, destFirstIndex, length);
        }

        // 対象がプリミティブ型なら Buffer.BlockCopy も選択肢
        // バイト列を、それが何か気にせずガバッとコピーするようで、参照の配列には向かないらしい

        // Buffer Class (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.buffer

        // Buffer.BlockCopy(Array, Int32, Array, Int32, Int32) Method (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.buffer.blockcopy

        // CopyBackward は、同じ配列内で複数の要素を後ろにズラすときに必要

        // c# - Copy an array backwards? Array.Copy? - Stack Overflow
        // https://stackoverflow.com/questions/2710899/copy-an-array-backwards-array-copy

        public static void CopyBackward <ElementType> (ElementType [] destElements, int destFirstIndex, ElementType [] sourceElements, int sourceFirstIndex, int length)
        {
            int xDestIndex = destFirstIndex + length - 1,
                xSourceIndex = sourceFirstIndex + length - 1;

            while (xDestIndex >= destFirstIndex)
                destElements [xDestIndex --] = sourceElements [xSourceIndex --];
        }

        // Fill を実装しない
        // 単一の要素を延々と書き込むなら Array.Fill で足りる

        // Repeat は、たとえば5文字の余白に ABC を書けるだけ書き、ABCAB を書く
        // Enumerable.Repeat があるが、これは単一の要素を繰り返すだけ

        // Enumerable.Repeat<TResult>(TResult, Int32) Method (System.Linq) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.repeat

        public static void Repeat <ElementType> (ElementType [] destElements, int destFirstIndex, int destLength, ElementType [] sourceElements, int sourceFirstIndex, int sourceLength)
        {
            if (sourceLength <= 0)
                throw new nArgumentException ();

            // たとえば8文字に3文字を [0] からリピートするなら、2回書けて、2回目の先頭は [0] + (2 - 1) 回目の最後までの長さ → 3
            // 同様に、残りの部分の先頭は、[0] + フルで書ける回数 * 書く文字列の長さ → 6

            int xDestIndex = destFirstIndex,
                xDestLastIndex = destFirstIndex + (destLength / sourceLength - 1) * sourceLength,
                xLastPartFirstIndex = destFirstIndex + destLength / sourceLength * sourceLength,
                xLastPartLength = destLength % sourceLength;

            while (xDestIndex <= xDestLastIndex)
            {
                Array.Copy (sourceElements, sourceFirstIndex, destElements, xDestIndex, sourceLength);
                xDestIndex += sourceLength;
            }

            if (xLastPartLength > 0)
                Array.Copy (sourceElements, sourceFirstIndex, destElements, xLastPartFirstIndex, xLastPartLength);
        }

        // Resize を実装しない
        // Array.Resize で足りる
        // firstIndex から length の範囲にアクセスできることを保証するメソッドも考えたが、
        //     newSize だけを指定する Array.Resize の方が使いやすそう

        // Array.Resize<T>(T[], Int32) Method (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.array.resize

        // Insert は、指示通りの移動と書き込みをするだけで、配列の容量を考慮しない
        // たまに、短い配列に、どうしてもこれだけはパッと挿入したい、のようなときがあるので実装
        // 容量を考えなくてよい List の方が優れるが、それでも非効率的な処理なので、より良い実装を考えるべき

        public static void Insert <ElementType> (ElementType [] destElements, int destFirstIndex, int destLength, ElementType [] sourceElements, int sourceFirstIndex, int sourceLength)
        {
            CopyBackward (destElements, destFirstIndex + sourceLength, destElements, destFirstIndex, destLength);
            Array.Copy (sourceElements, sourceFirstIndex, destElements, destFirstIndex, sourceLength);
        }

        // Remove も、Insert と同様、たまに局所的に使うならよいが、効率を考えること
        // 左へ詰めた分で右にごみデータが残るので注意

        public static void Remove <ElementType> (ElementType [] elements, int firstIndex, int length, int removingLength)
        {
            Array.Copy (elements, firstIndex + removingLength, elements, firstIndex, length - removingLength);
        }

        // Swap を実装しない
        // 簡単な処理なので3行でなく1行で書きたいが、
        //     ループで何度も行われることなのでオーバーヘッドを削る

        // Reverse, Sort を実装しない
        // いずれも Array と MemoryExtensions の両方にある

        // Rotate は、FILO と LIFO を行いたいが LinkedList を使うほどでないところに役立つか
        // 電光掲示板をやるなら、1文字1ノードで LinkedList を使うより効率的かもしれない

        public static void Rotate <ElementType> (ElementType [] elements, int firstIndex, int length, int rotatingRightLength)
        {
            int xRotatingLength = rotatingRightLength % length;

            if (xRotatingLength < 0)
                xRotatingLength += length;

            ElementType [] xBuffer = new ElementType [xRotatingLength];
            Array.Copy (elements, firstIndex + length - xRotatingLength, xBuffer, 0, xRotatingLength);
            Insert (elements, firstIndex, length - xRotatingLength, xBuffer, 0, xRotatingLength);
        }

        // Shuffle においては、暗号に求められるような精度は不要なので、Fisher–Yates shuffle を実装
        // 次のページの The modern algorithm の一つ目にならった

        // Fisher–Yates shuffle - Wikipedia
        // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle

        /// <summary>
        /// random を指定しなければ、Random.Shared が使われる。
        /// </summary>
        public static void Shuffle <ElementType> (ElementType [] elements, int firstIndex, int length, Random? random = null)
        {
            int xLastIndex = firstIndex + 1;
            Random xRandom = random ?? Random.Shared;

            for (int temp = firstIndex + length - 1; temp >= xLastIndex; temp --)
            {
                // maxValue は、The exclusive upper bound of the random number returned とのこと

                // Random.Next Method (System) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.random.next

                int xIndex = xRandom.Next (firstIndex, temp + 1);

#pragma warning disable IDE0180
                ElementType xElement = elements [xIndex];
#pragma warning restore IDE0180
                elements [xIndex] = elements [temp];
                elements [temp] = xElement;
            }
        }

        // Replace, ReplaceAny, ReplaceAnyExcept を実装しない
        // ほぼ文字列にしか適用されない処理
        // 文字列においては、正規表現の方が有用なときもあるし、A → X、B → Y と、二つ以上の組み合わせを一度に処理したいときもある
        // そういったことをできるだけ多くのパターン、実装したところで、置換の処理をライブラリーに任せきることはできない
    }
}
