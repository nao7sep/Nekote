using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // nEqualityComparer のコメントを参照

    // StringComparer にならい <string?> と Nullable にしている

    // StringComparer.cs
    // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/StringComparer.cs

    public class nAlphanumericComparer: Comparer <string?>
    {
        // CurrentCulture と CurrentCultureIgnoreCase を用意しない
        // 「どのカルチャーか不詳だが、とりあえず現在のカルチャーだ」という実装にはリスクがあるため
        // StringComparer.Create で容易に作れるので、名前と中身の一致するものを個別に用意

        public static readonly nAlphanumericComparer InvariantCulture = new nAlphanumericComparer (StringComparer.InvariantCulture);

        public static readonly nAlphanumericComparer InvariantCultureIgnoreCase = new nAlphanumericComparer (StringComparer.InvariantCultureIgnoreCase);

        public static readonly nAlphanumericComparer Ordinal = new nAlphanumericComparer (StringComparer.Ordinal);

        public static readonly nAlphanumericComparer OrdinalIgnoreCase = new nAlphanumericComparer (StringComparer.OrdinalIgnoreCase);

        // 数字でない部分の比較に使われる
        // 大文字・小文字の区別をしないなどもこちらで指定

        // StringComparer.InvariantCulture などが StringComparer として宣言されている
        // IComparer または Comparer で扱う選択肢もあるが、ここではより具体的に

        public readonly StringComparer StringComparer;

        public nAlphanumericComparer (StringComparer stringComparer)
        {
            StringComparer = stringComparer;
        }

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
                    (int FirstIndex, int Length) iFindNumericPart (string value, int currentIndex)
                    {
                        for (int temp = currentIndex; temp < value.Length; temp ++)
                        {
                            if (nChar.IsSupportedDigit (value [temp]))
                            {
                                for (int tempAlt = temp + 1; tempAlt < value.Length; tempAlt ++)
                                {
                                    if (nChar.IsSupportedDigit (value [tempAlt]) == false)
                                        return (temp, tempAlt - temp);
                                }

                                return (temp, value.Length - temp);
                            }
                        }

                        // 見付かれば、FirstIndex は必ず0以上に、Length は必ず1以上になる
                        // 「長さ0のものが見付かった」は、ここでは起こらない
                        return (-1, 0);
                    }

                    int xCurrentIndex1 = 0,
                        xCurrentIndex2 = 0;

                    while (true)
                    {
                        (int FirstIndex, int Length)
                            xNumericPart1 = iFindNumericPart (value1, xCurrentIndex1),
                            xNumericPart2 = iFindNumericPart (value2, xCurrentIndex2);

                        // Substring を避けたいが、StringComparer.Compare が現時点では Span 系の引数を取らないので仕方ない

                        string xStringPart1 = xNumericPart1.FirstIndex >= 0 ?
                                value1.Substring (xCurrentIndex1, xNumericPart1.FirstIndex - xCurrentIndex1) : value1.Substring (xCurrentIndex1),
                            xStringPart2 = xNumericPart2.FirstIndex >= 0 ?
                                value2.Substring (xCurrentIndex2, xNumericPart2.FirstIndex - xCurrentIndex2) : value2.Substring (xCurrentIndex2);

                        int xResult = StringComparer.Compare (xStringPart1, xStringPart2);

                        if (xResult != 0)
                            return xResult;

                        if (xNumericPart1.Length > 0)
                        {
                            if (xNumericPart2.Length > 0)
                            {
                                xResult = nString.CompareNumericStrings (
                                    value1.AsSpan (xNumericPart1.FirstIndex, xNumericPart1.Length),
                                    value2.AsSpan (xNumericPart2.FirstIndex, xNumericPart2.Length));

                                if (xResult != 0)
                                    return xResult;

                                xCurrentIndex1 = xNumericPart1.FirstIndex + xNumericPart1.Length;
                                xCurrentIndex2 = xNumericPart2.FirstIndex + xNumericPart2.Length;
                            }

                            else return 1;
                        }

                        else
                        {
                            if (xNumericPart2.Length > 0)
                                return -1;

                            else return 0;
                        }
                    }
                }
            }
        }
    }
}
