using System;
using System.Collections.Generic;

namespace Nekote.Core.Text
{
    /// <summary>
    /// NaturalStringComparer の内部実装。
    /// </summary>
    internal sealed class NaturalStringComparerImplementation : NaturalStringComparer
    {
        private readonly StringComparer _baseComparer;

        internal NaturalStringComparerImplementation(StringComparer baseComparer)
        {
            if (baseComparer is null)
            {
                throw new ArgumentNullException(nameof(baseComparer));
            }
            _baseComparer = baseComparer;
        }

        public override int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }
            if (x is null)
            {
                return -1;
            }
            if (y is null)
            {
                return 1;
            }

            return Compare(x.AsSpan(), y.AsSpan());
        }

        public override bool Equals(string? x, string? y)
        {
            return Compare(x, y) == 0;
        }

        public override int GetHashCode(string obj)
        {
            return GetHashCode(obj.AsSpan());
        }

        public override int Compare(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
        {
            // アルゴリズムの戦略：
            // 文字列をスキャンし、文字チャンクと数字チャンクに分割して比較します。
            int posX = 0;
            int posY = 0;

            // 両方の文字列に比較するコンテンツが残っている間、ループを続けます。
            while (posX < x.Length && posY < y.Length)
            {
                bool isDigitX = char.IsDigit(x[posX]);
                bool isDigitY = char.IsDigit(y[posY]);

                if (isDigitX && isDigitY)
                {
                    // ケース1：両方のチャンクが数字です。
                    // 数字チャンク全体を抽出し、数値として比較します。
                    int numStartX = posX;
                    while (posX < x.Length && char.IsDigit(x[posX]))
                    {
                        posX++;
                    }
                    var numSpanX = x.Slice(numStartX, posX - numStartX);

                    int numStartY = posY;
                    while (posY < y.Length && char.IsDigit(y[posY]))
                    {
                        posY++;
                    }
                    var numSpanY = y.Slice(numStartY, posY - numStartY);

                    int result = CompareNumeric(numSpanX, numSpanY);
                    if (result != 0)
                    {
                        return result;
                    }
                }
                else if (!isDigitX && !isDigitY)
                {
                    // ケース2：両方のチャンクが非数字（テキスト）です。
                    // テキストチャンクを抽出し、指定された基本のStringComparerで比較します。
                    int strStartX = posX;
                    while (posX < x.Length && !char.IsDigit(x[posX]))
                    {
                        posX++;
                    }
                    var strSpanX = x.Slice(strStartX, posX - strStartX);

                    int strStartY = posY;
                    while (posY < y.Length && !char.IsDigit(y[posY]))
                    {
                        posY++;
                    }
                    var strSpanY = y.Slice(strStartY, posY - strStartY);

                    int result = _baseComparer.Compare(strSpanX.ToString(), strSpanY.ToString());
                    if (result != 0)
                    {
                        return result;
                    }
                }
                else
                {
                    // ケース3：数字とテキストの比較。
                    // 一般的な規則に従い、数字は常にテキストより小さい（先に来る）と見なします。
                    return isDigitX ? -1 : 1;
                }
            }

            // 一方の文字列がもう一方のプレフィックスである場合（例："file" vs "file1"）。
            // すべてのチャンクが等しかった後、コンテンツが残っている方が大きいと見なされます。
            if (posX >= x.Length && posY >= y.Length)
            {
                // 両方の文字列を同時に使い切った場合、それらは等しいです。
                return 0;
            }

            // 短い方が小さいと見なされます。
            return posX >= x.Length ? -1 : 1;
        }

        public override bool Equals(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
        {
            return Compare(x, y) == 0;
        }

        public override int GetHashCode(ReadOnlySpan<char> obj)
        {
            // このハッシュコード計算は、Equalsメソッドと一貫性があります。
            // つまり、Equalsがtrueを返す2つの文字列は、同じハッシュコードを生成します。
            var hashCode = new HashCode();
            int pos = 0;

            while (pos < obj.Length)
            {
                if (char.IsDigit(obj[pos]))
                {
                    // 数字チャンクを処理します。
                    int start = pos;
                    while (pos < obj.Length && char.IsDigit(obj[pos]))
                    {
                        pos++;
                    }
                    // 先頭のゼロをトリムして、数値的な値を正規化します。
                    // これにより、「file 1」と「file 01」が同じハッシュ貢献を持つようになります。
                    // 特殊なケースとして、"0"や"00"は空スパン""になります。この場合、long.TryParseは失敗し、
                    // フォールバックとして空文字列のハッシュコードが使われます。
                    // これにより、全てのゼロ値（"0", "00"など）が同じハッシュコードを持つことが保証されます。
                    var numSpan = obj.Slice(start, pos - start).TrimStart('0');

                    if (long.TryParse(numSpan, out long num))
                    {
                        hashCode.Add(num);
                    }
                    else
                    {
                        // longに収まらない巨大な数の場合は文字列としてハッシュコードを計算します。
                        hashCode.Add(numSpan.ToString());
                    }
                }
                else
                {
                    // テキストチャンクを処理します。
                    int start = pos;
                    while (pos < obj.Length && !char.IsDigit(obj[pos]))
                    {
                        pos++;
                    }
                    var strSpan = obj.Slice(start, pos - start);
                    hashCode.Add(strSpan.ToString(), _baseComparer);
                }
            }
            return hashCode.ToHashCode();
        }

        /// <summary>
        /// 文字列の数値部分を自然順で比較します。
        /// </summary>
        private static int CompareNumeric(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
        {
            // このメソッドは、2つの文字列スパンを数値として比較します。
            // 例：「2」は「10」より小さい。

            // 先頭のゼロをトリムして、数値的な値を表現する部分を取得します。
            // これにより、「01」と「1」が同じ数値として扱われます。
            // 特殊なケースとして、"0"や"000"のような文字列は空のスパン""になります。
            // この場合、その長さは0となり、どの正の数の有効桁数（1以上）よりも小さくなるため、
            // 続く長さの比較によって正しく「0」が他の全ての正の数より小さいと判断されます。
            var trimX = x.TrimStart('0');
            var trimY = y.TrimStart('0');

            // まず、有効な桁数で比較します。桁数が多い方が数値的に大きいです。
            // 例：「100」（長さ3）は「99」（長さ2）より大きい。
            // 例：「1」（長さ1）は「0」（トリム後長さ0）より大きい。
            if (trimX.Length != trimY.Length)
            {
                return trimX.Length.CompareTo(trimY.Length);
            }

            // 桁数が同じ場合、単純な辞書順比較で十分です。
            // 例：「20」は「10」より大きい。
            return trimX.SequenceCompareTo(trimY);
        }
    }
}
