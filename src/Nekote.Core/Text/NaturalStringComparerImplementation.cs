using System;
using System.Collections.Generic;
using System.Globalization;

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

        public override int GetHashCode(string obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            return GetHashCode(obj.AsSpan());
        }

        /// <summary>
        /// 2つの文字スパンを自然順で比較します。
        /// </summary>
        /// <remarks>
        /// アルゴリズム戦略:
        /// 1. GraphemeReaderを使用して、文字列を文字単位ではなく書記素クラスタ単位で反復処理します。
        ///    これにより、サロゲートペア（絵文字など）や結合文字が正しく1つの単位として扱われます。
        /// 2. 文字列を「数値チャンク」と「非数値チャンク」に分割します。
        /// 3. 対応するチャンク同士を比較します。
        ///    - 両方が数値チャンクの場合、数値として比較します（例: 2は10より小さい）。
        ///    - 両方が非数値チャンクの場合、指定された基本コンパレータ（_baseComparer）を使用して文字列として比較します。
        /// 4. チャンクの種類が異なる場合（例: 数字と文字）、比較を基本コンパレータに委ね、一貫した順序付けを保証します。
        /// 5. 非数値チャンクの比較では、一方がもう一方の接頭辞であるケースを正しく処理します。
        ///    （例: "file"と"file.txt"）共通部分を比較した後、残りの部分の比較を続行します。
        /// </remarks>
        public override int Compare(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
        {
            var xReader = new GraphemeReader(new string(x));
            var yReader = new GraphemeReader(new string(y));

            while (!xReader.IsEndOfText && !yReader.IsEndOfText)
            {
                var isDigitX = IsCurrentGraphemeDigit(xReader);
                var isDigitY = IsCurrentGraphemeDigit(yReader);

                if (isDigitX && isDigitY)
                {
                    // シナリオ1: 両方の現在位置が数字。
                    // この場合、両方の文字列から完全な数値チャンク（連続する数字の並び）を抽出し、
                    // それらを数値として比較します。例えば "2" と "10" を比較する場合、
                    // "2" は "10" より小さいと正しく判断されます。
                    // これが自然順ソートの核となるロジックです。
                    var xStartPos = xReader.Position;
                    while (!xReader.IsEndOfText && IsCurrentGraphemeDigit(xReader))
                    {
                        xReader.Advance();
                    }
                    var xChunk = xReader.Slice(xStartPos, xReader.Position - xStartPos);

                    var yStartPos = yReader.Position;
                    while (!yReader.IsEndOfText && IsCurrentGraphemeDigit(yReader))
                    {
                        yReader.Advance();
                    }
                    var yChunk = yReader.Slice(yStartPos, yReader.Position - yStartPos);

                    var result = CompareNumeric(xChunk, yChunk);
                    if (result != 0) return result;
                }
                else if (!isDigitX && !isDigitY)
                {
                    // シナリオ2: 両方の現在位置が非数字。
                    // ここでの課題は、"file.txt" と "file1.txt" のようなケースを正しく扱うことです。
                    // もし単純に非数値チャンク全体（この例では "file.txt" と "file"）を比較すると、
                    // "file" が小さいと判断され、"file1.txt" が "file.txt" の前に来てしまい、誤った順序になります。
                    //
                    // この問題を回避するため、ここでは両方の非数値チャンクの「最短の長さ」ぶんだけを比較します。
                    // "file.txt" と "file1.txt" の例では、"file" 同士を比較し、結果は等価（0）になります。
                    // このブロックの処理でリーダーが共通部分の末尾に進むため、ループの次のイテレーションでは
                    // 状況が「非数字 vs 数字」（".txt" vs "1.txt"）に変わり、そこで最終的な順序が決定されます。
                    var xStartPos = xReader.Position;
                    while (!xReader.IsEndOfText && !IsCurrentGraphemeDigit(xReader))
                    {
                        xReader.Advance();
                    }
                    var xChunkGraphemeCount = xReader.Position - xStartPos;

                    var yStartPos = yReader.Position;
                    while (!yReader.IsEndOfText && !IsCurrentGraphemeDigit(yReader))
                    {
                        yReader.Advance();
                    }
                    var yChunkGraphemeCount = yReader.Position - yStartPos;

                    var minGraphemeCount = Math.Min(xChunkGraphemeCount, yChunkGraphemeCount);

                    var xCommonPrefix = xReader.Slice(xStartPos, minGraphemeCount);
                    var yCommonPrefix = yReader.Slice(yStartPos, minGraphemeCount);
                    var result = _baseComparer.Compare(xCommonPrefix.ToString(), yCommonPrefix.ToString());
                    if (result != 0) return result;

                    // 共通接頭辞が等しい場合、リーダーの位置を共通部分の末尾まで進めて、
                    // 次のチャンクの比較を継続できるようにします。
                    xReader.Position = xStartPos + minGraphemeCount;
                    yReader.Position = yStartPos + minGraphemeCount;
                }
                else
                {
                    // シナリオ3: 一方が数字で、もう一方が非数字。
                    // この時点で、2つの文字列は根本的に異なる構造を持つことが確定します。
                    // 例えば "a1" と "aa" を比較する場合、最初の 'a' は共通ですが、
                    // 次に '1' と 'a' が比較されます。
                    //
                    // このような場合、残りの部分文字列全体を単純に比較するのが最も安全で確実です。
                    // これにより、Unicodeの将来のバージョンで複数のコードポイントから成る新しい数字が導入されたとしても、
                    // アルゴリズムの堅牢性が保たれます。
                    var xRest = xReader.Slice(xReader.Position, xReader.Count - xReader.Position);
                    var yRest = yReader.Slice(yReader.Position, yReader.Count - yReader.Position);
                    return _baseComparer.Compare(xRest.ToString(), yRest.ToString());
                }
            }

            // 一方の文字列がもう一方の末尾に到達した場合、短い方が小さいと見なされます。
            return xReader.IsEndOfText == yReader.IsEndOfText ? 0 : (xReader.IsEndOfText ? -1 : 1);
        }

        public override bool Equals(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
        {
            return Compare(x, y) == 0;
        }

        /// <summary>
        /// 指定した文字スパンのハッシュコードを返します。
        /// </summary>
        /// <remarks>
        /// このハッシュコード計算は、Compareメソッドのロジックと一貫性があります。
        /// 文字列を同じように数値チャンクと非数値チャンクに分割し、各チャンクのハッシュ値を計算して結合します。
        /// これにより、Compareが0（等しい）を返す2つの文字列は、同じハッシュコードを持つことが保証されます。
        /// </remarks>
        public override int GetHashCode(ReadOnlySpan<char> obj)
        {
            var hashCode = new HashCode();
            var reader = new GraphemeReader(new string(obj));

            while (!reader.IsEndOfText)
            {
                var isDigit = IsCurrentGraphemeDigit(reader);
                var startPos = reader.Position;

                while (!reader.IsEndOfText && IsCurrentGraphemeDigit(reader) == isDigit)
                {
                    reader.Advance();
                }
                var chunk = reader.Slice(startPos, reader.Position - startPos);

                if (isDigit)
                {
                    // 数値チャンクの場合、先行ゼロをトリムして数値としてハッシュ値を追加します。
                    // これにより "01" と "1" が同じハッシュ値を持つようになります。
                    var spanToHash = _normalize ? Normalize(chunk) : chunk;
                    var trimmedSpan = spanToHash.TrimStart('0');
                    if (long.TryParse(trimmedSpan, out var numericValue))
                    {
                        hashCode.Add(numericValue);
                    }
                    else
                    {
                        // longに収まらない巨大な数の場合は文字列としてハッシュコードを計算します。
                        hashCode.Add(trimmedSpan.ToString());
                    }
                }
                else
                {
                    // 非数値チャンクの場合、基本コンパレータを使用してハッシュ値を追加します。
                    hashCode.Add(chunk.ToString(), _baseComparer);
                }
            }
            return hashCode.ToHashCode();
        }

        /// <summary>
        /// 書記素クラスタが数字として扱われるべきかどうかを判断します。
        /// </summary>
        private bool IsCurrentGraphemeDigit(GraphemeReader reader)
        {
            if (reader.IsEndOfText)
            {
                return false;
            }

            // 自然順ソートの目的では、書記素クラスタの最初の文字が数字であれば、
            // そのクラスタ全体を「数字」として扱います。
            // 複数の文字から成る書記素クラスタ（例：絵文字）が数字として解釈されることは
            // ほとんどないため、このヒューリスティックは効率的かつ実用的です。
            var grapheme = reader.PeekAsSpan();
            if (grapheme.IsEmpty) return false;

            char c = grapheme[0];

            // ASCII数字は常に数字として扱われます。
            if (c >= '0' && c <= '9')
            {
                return true;
            }

            // 正規化が有効な場合にのみ、全角数字を数字として扱います。
            if (_normalize && c >= '０' && c <= '９')
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 文字列の数値部分を自然順で比較します。
        /// </summary>
        /// <remarks>
        /// このメソッドは、2つの文字列スパンを数値として比較します。
        /// 例：「2」は「10」より小さい。
        ///
        /// 先行するゼロは無視されます（例：「01」と「1」は等しい）。
        /// まず有効な桁数で比較し、桁数が同じ場合は辞書順で比較します。
        /// </remarks>
        private int CompareNumeric(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
        {
            ReadOnlySpan<char> xToCompare = x;
            ReadOnlySpan<char> yToCompare = y;

            if (_normalize)
            {
                xToCompare = Normalize(x);
                yToCompare = Normalize(y);
            }

            var trimX = xToCompare.TrimStart('0');
            var trimY = yToCompare.TrimStart('0');

            if (trimX.Length != trimY.Length)
            {
                return trimX.Length.CompareTo(trimY.Length);
            }

            return trimX.SequenceCompareTo(trimY);
        }

        /// <summary>
        /// 数字スパンを正規化します（例：全角を半角に）。
        /// </summary>
        /// <remarks>
        /// このメソッドは、パフォーマンスのために、全角数字が含まれている場合にのみ新しいメモリ割り当てを行います。
        /// </remarks>
        private static ReadOnlySpan<char> Normalize(ReadOnlySpan<char> span)
        {
            bool hasFullWidth = false;
            for (int index = 0; index < span.Length; index++)
            {
                if (span[index] >= '０' && span[index] <= '９')
                {
                    hasFullWidth = true;
                    break;
                }
            }

            if (!hasFullWidth)
            {
                return span;
            }

            var buffer = new char[span.Length];
            for (int index = 0; index < span.Length; index++)
            {
                char currentChar = span[index];
                if (currentChar >= '０' && currentChar <= '９')
                {
                    buffer[index] = (char)('0' + (currentChar - '０'));
                }
                else
                {
                    buffer[index] = currentChar;
                }
            }
            return buffer;
        }
    }
}
