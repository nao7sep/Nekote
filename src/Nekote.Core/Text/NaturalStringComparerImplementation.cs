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
        private readonly bool _normalize;

        /// <summary>
        /// NaturalStringComparerの実装を初期化します。
        /// </summary>
        /// <param name="baseComparer">テキスト部分の比較に使用する基本的な文字列コンパレータ。</param>
        /// <param name="normalize">比較前にUnicode正規化（例：全角数字を半角に変換）を行うかどうか。</param>
        internal NaturalStringComparerImplementation(StringComparer baseComparer, bool normalize)
        {
            if (baseComparer is null)
            {
                throw new ArgumentNullException(nameof(baseComparer));
            }
            _baseComparer = baseComparer;
            _normalize = normalize;
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

        /// <summary>
        /// 指定した文字列のハッシュコードを返します。
        /// </summary>
        /// <param name="obj">ハッシュコードを取得する対象の文字列。</param>
        /// <returns>指定した文字列のハッシュコード。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> が null の場合。</exception>
        public override int GetHashCode(string obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
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

                if (isDigitX != isDigitY)
                {
                    // ケース1：数字と非数字の比較（例：'a' vs '1'、'-' vs '1'）。
                    // 以前の最適化（数字 < 非数字）は、記号（例：'-'）に対して不正確でした。
                    // 正確性を保証するため、基本コンパレータに単一文字の比較を委ねます。
                    // これが、文字列全体としての最初の相違点となるため、この結果が最終的な結果となります。
                    return _baseComparer.Compare(x.Slice(posX, 1).ToString(), y.Slice(posY, 1).ToString());
                }

                // この時点で、両方の文字が数字であるか、両方とも非数字であることがわかっています。
                if (isDigitX) // ケース2：両方とも数字
                {
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
                else // ケース3：両方とも非数字
                {
                    // ここでは、文字単位ではなく、テキストチャンク全体を抽出して比較します。
                    // これは、カルチャ依存の比較（Ordinal以外）を正しく処理するために不可欠です。
                    // StringComparerは、複数の文字を単一の照合要素として扱うことがあります
                    // （例：合成文字 "e" + "´" -> "é"、サロゲートペアで表現される絵文字など）。
                    // そのため、コンパレータには完全なコンテキスト（チャンク全体）を渡す必要があります。
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

        /// <summary>
        /// 指定した文字スパンのハッシュコードを返します。
        /// </summary>
        /// <param name="obj">ハッシュコードを取得する対象の文字スパン。</param>
        /// <returns>指定した文字スパンのハッシュコード。</returns>
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
                    var numSpan = obj.Slice(start, pos - start);

                    ReadOnlySpan<char> spanToHash = _normalize ? Normalize(numSpan) : numSpan;

                    // 先頭のゼロをトリムして、数値的な値を正規化します。
                    // これにより、「file 1」と「file 01」が同じハッシュ貢献を持つようになります。
                    // 特殊なケースとして、"0"や"00"は空スパン""になります。この場合、long.TryParseは失敗し、
                    // フォールバックとして空文字列のハッシュコードが使われます。
                    // これにより、全てのゼロ値（"0", "00"など）が同じハッシュコードを持つことが保証されます。
                    var trimmedSpan = spanToHash.TrimStart('0');

                    if (long.TryParse(trimmedSpan, out long num))
                    {
                        hashCode.Add(num);
                    }
                    else
                    {
                        // longに収まらない巨大な数の場合は文字列としてハッシュコードを計算します。
                        hashCode.Add(trimmedSpan.ToString());
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
        private int CompareNumeric(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
        {
            ReadOnlySpan<char> xToCompare = x;
            ReadOnlySpan<char> yToCompare = y;

            if (_normalize)
            {
                xToCompare = Normalize(x);
                yToCompare = Normalize(y);
            }

            // このメソッドは、2つの文字列スパンを数値として比較します。
            // 例：「2」は「10」より小さい。

            // 先頭のゼロをトリムして、数値的な値を表現する部分を取得します。
            // これにより、「01」と「1」が同じ数値として扱われます。
            // 特殊なケースとして、"0"や"000"のような文字列は空のスパン""になります。
            // この場合、その長さは0となり、どの正の数の有効桁数（1以上）よりも小さくなるため、
            // 続く長さの比較によって正しく「0」が他の全ての正の数より小さいと判断されます。
            var trimX = xToCompare.TrimStart('0');
            var trimY = yToCompare.TrimStart('0');

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

        /// <summary>
        /// 数字スパンを正規化します（例：全角を半角に）。
        /// </summary>
        private static ReadOnlySpan<char> Normalize(ReadOnlySpan<char> span)
        {
            // 最初に全角数字があるかチェックして、不要な割り当てを避けます
            bool hasFullWidth = false;
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] >= '０' && span[i] <= '９')
                {
                    hasFullWidth = true;
                    break;
                }
            }

            // 全角数字がない場合は元のスパンをそのまま返します
            if (!hasFullWidth)
            {
                return span;
            }

            // 全角数字がある場合のみバッファを割り当てて正規化します
            var buffer = new char[span.Length];
            for (int i = 0; i < span.Length; i++)
            {
                char c = span[i];
                if (c >= '０' && c <= '９')
                {
                    buffer[i] = (char)('0' + (c - '０'));
                }
                else
                {
                    buffer[i] = c;
                }
            }
            return buffer;
        }
    }
}
