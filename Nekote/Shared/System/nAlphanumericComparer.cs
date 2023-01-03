using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // このクラスは、"1a01" == "01a1" だったり "2" < "10" だったりの比較を行う

    // "File-2.txt" > "File-10.txt" を避けるのに不可欠なので、そういう機能が .NET にあるべきだが、パッと探した限り見付からなかった
    // それどころか、他者のやり方をググっても、Parse と Parse の結果を比較しろだとか、
    //     オーバーフローの回避のために BigInteger を使えだとか、そのくらいの情報しか自分には見付からなかった

    // 手元のいくつかのプログラムで試してみた
    // Windows の File Explorer, Visual Studio Code では、このクラスと同様のソートが行われた
    // Chrome と Firefox のブックマークの並び替え、GitKraken, SmartGit, Visual Studio ではダメだった
    // ブックマークにおいては「ページ2」と「ページ10」のようなことが頻繁に起こるわけで、それでも未対応なのが不思議

    // 実装についてのコメントは Compare メソッドのところに

    // IComparer でなく Comparer をベースとする理由については、nEqualityComparer のコメントを参照
    // ジェネリックの型指定は、StringComparer にならい、<string?> と Nullable にしている

    // StringComparer.cs
    // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/StringComparer.cs

    public class nAlphanumericComparer: Comparer <string?>
    {
        // CurrentCulture と CurrentCultureIgnoreCase を用意しない
        // 「どのカルチャーか不詳だが、とりあえず現在のカルチャーだ」という実装にはリスクがあるため
        // StringComparer においても、少なくとも自分は、CurrentCulture* を使ったことがないかもしれないくらい使わない
        // StringComparer.Create により簡単に作れるので、名前と内容の一致する comparer を個別に用意するべき
        // そのうち ja-JP の分については、nJaJpCulture クラスが既にあるため、そちらに追加しておく

        public static readonly nAlphanumericComparer InvariantCulture = new nAlphanumericComparer (StringComparer.InvariantCulture);

        public static readonly nAlphanumericComparer InvariantCultureIgnoreCase = new nAlphanumericComparer (StringComparer.InvariantCultureIgnoreCase);

        public static readonly nAlphanumericComparer Ordinal = new nAlphanumericComparer (StringComparer.Ordinal);

        public static readonly nAlphanumericComparer OrdinalIgnoreCase = new nAlphanumericComparer (StringComparer.OrdinalIgnoreCase);

        // 数字でない部分の比較に使われる
        // 大文字・小文字の区別をしないなどもこちらで指定

        // StringComparer.InvariantCulture などは、StringComparer として宣言されている
        // IComparer または Comparer により扱う選択肢もあるが、ここではより具体的に

        public readonly StringComparer StringComparer;

        public nAlphanumericComparer (StringComparer stringComparer)
        {
            StringComparer = stringComparer;
        }

        // ここでの「数値」の定義を書いておく
        // いろいろと考えたが、「半角または全角の数字が一つ以上連続するもの」とした

        // 正負の符号、桁区切りのカンマ、ドット、空白、小数点以下は、いずれも無視される
        // 符号については、正負を意味しないのに誤認されることが多発するため
        // 桁区切りについては、国や地域によりルールが大きく異なるためと、
        //     符号と同じく、桁区切りを意味しないのに誤認されることが多発するため
        // 小数点以下については、これも国や地域によりルールが異なるためと、
        //     ファイル名など、このメソッドにより扱われることが主に想定されるものに小数点以下までが含まれることがあまり考えられないため

        public override int Compare (string? value1, string? value2)
        {
            if (value1 == null)
            {
                if (value2 == null)
                    return 0;

                else return -1;
            }

            else
            {
                if (value2 == null)
                    return 1;

                else
                {
                    // 指定された位置から数字を探し、見付かれば、その開始位置と長さを返す
                    // なければ、FirstIndex が負になるので、必ずそれにより判別
                    // その場合の Length は、形としては不定と見なされる

                    (int FirstIndex, int Length) iFindNumericPart (string value, int currentIndex)
                    {
                        // 予想以上に遅かったので、string.IndexOfAny を使うように
                        // 詳細は、iStringTester.CompareStringComparisonSpeeds のところに

                        // for (int temp = currentIndex; temp < value.Length; temp ++)
                        {
                            int temp = value.IndexOfAny (nChar.SupportedDigits, currentIndex);

                            // if (nChar.IsSupportedDigit (value [temp]))
                            if (temp >= 0)
                            {
                                for (int tempAlt = temp + 1; tempAlt < value.Length; tempAlt ++)
                                {
                                    if (nChar.IsSupportedDigit (value [tempAlt]) == false)
                                        return (temp, tempAlt - temp);
                                }

                                return (temp, value.Length - temp);
                            }
                        }

                        return (-1, default);
                    }

                    // それぞれの文字列を独立的に読み進める
                    // たとえば、a1b ... と a01b ... においては、a が文字モードの比較で一致し、1 と 01 が数値モードの比較で一致する
                    // 二つの文字列の次の読み取り位置は、含まれる数値の0詰めによりズレていく

                    int xCurrentIndex1 = 0,
                        xCurrentIndex2 = 0;

                    while (true)
                    {
                        (int FirstIndex, int Length)
                            xNumericPart1 = iFindNumericPart (value1, xCurrentIndex1),
                            xNumericPart2 = iFindNumericPart (value2, xCurrentIndex2);

                        // ここで考えるべき状況は三つ

                        // 1. いずれかの文字列または両方に数字が見付からない → ループのこの回も次回以降も絶対に数値モードでの比較にならないので、両方の残り部分を文字列モードで比較して結果を返す
                        // 2. 両方に数字が見付かったが、（相対的な）出現位置が異なる → 先に現れる方の数値の一文字目の数字が他方の（数字でない）文字と絶対に不一致なので、1と同様に処理
                        // 3. 両方に数字が見付かり、出現位置も同じ → そこまでを文字列モードで比較し、数値を数値モードで比較し、双方のインデックスを更新し、ループの次の回へ

                        if ((xNumericPart1.FirstIndex < 0 || xNumericPart2.FirstIndex < 0) ||
                            ((xNumericPart1.FirstIndex - xCurrentIndex1) != (xNumericPart2.FirstIndex - xCurrentIndex2)))
                        {
                            // Substring を避けたいが、StringComparer.Compare が現時点では Span 系の引数を取らないので仕方ない

                            // xCurrentIndex* == 0 だと切り抜きが行われずに this が返されるのを確認した

                            // String.Manipulation.cs
                            // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/String.Manipulation.cs

                            return StringComparer.Compare (value1.Substring (xCurrentIndex1), value2.Substring (xCurrentIndex2));
                        }

                        else
                        {
                            string xStringPart1 = value1.Substring (xCurrentIndex1, xNumericPart1.FirstIndex - xCurrentIndex1),
                                xStringPart2 = value2.Substring (xCurrentIndex2, xNumericPart2.FirstIndex - xCurrentIndex2);

                            int xResult = StringComparer.Compare (xStringPart1, xStringPart2);

                            if (xResult != 0)
                                return xResult;

                            xResult = nString.CompareNumericStrings (
                                value1.AsSpan (xNumericPart1.FirstIndex, xNumericPart1.Length),
                                value2.AsSpan (xNumericPart2.FirstIndex, xNumericPart2.Length));

                            if (xResult != 0)
                                return xResult;

                            xCurrentIndex1 = xNumericPart1.FirstIndex + xNumericPart1.Length;
                            xCurrentIndex2 = xNumericPart2.FirstIndex + xNumericPart2.Length;
                        }
                    }
                }
            }
        }
    }
}
