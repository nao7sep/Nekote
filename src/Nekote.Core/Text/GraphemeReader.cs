using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Nekote.Core.Text
{
    /// <summary>
    /// 文字列を、ユーザーが認識する1文字の単位（書記素クラスタ）で安全に読み取るためのリーダーを提供します。
    /// このクラスは IReadOnlyList<string> を実装しており、インデクサ、foreachループ、LINQをサポートします。
    /// サロゲートペア（絵文字など）や結合文字（アクセント記号など）を単一の単位として正しく扱います。
    /// </summary>
    public sealed class GraphemeReader : IReadOnlyList<string>
    {
        private readonly ReadOnlyMemory<char> _source;
        private readonly int[] _graphemeIndexes;
        private int _position;

        /// <summary>
        /// GraphemeReader クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="source">読み取る対象の文字列。</param>
        public GraphemeReader(string source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            _source = source.AsMemory();
            _graphemeIndexes = ParseGraphemeBoundaries(source.AsSpan());
            Position = 0;
        }

        /// <summary>
        /// GraphemeReader クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="source">読み取る対象の文字スパン。</param>
        public GraphemeReader(ReadOnlySpan<char> source)
        {
            _source = source.ToArray().AsMemory();
            _graphemeIndexes = ParseGraphemeBoundaries(source);
            Position = 0;
        }

        /// <summary>
        /// 読み取り対象の元の文字スパンを取得します。
        /// </summary>
        public ReadOnlySpan<char> Source => _source.Span;

        /// <summary>
        /// 書記素クラスタ単位での文字列の長さを取得します。
        /// </summary>
        /// <example>
        /// "a👍c" の場合、Count は 3 です。
        /// </example>
        public int Count => _graphemeIndexes.Length;

        /// <summary>
        /// 書記素クラスタ単位での現在の読み取り位置を取得または設定します。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">設定値が 0 未満または <see cref="Count"/> より大きいです。</exception>
        public int Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _position = value;
            }
        }

        /// <summary>
        /// 読み取りが文字列の末尾に到達したかどうかを示す値を取得します。
        /// </summary>
        public bool IsEndOfText => Position >= Count;

        /// <summary>
        /// 指定したインデックスにある書記素クラスタを取得します。
        /// </summary>
        /// <param name="index">取得する書記素クラスタのインデックス。</param>
        /// <returns>指定されたインデックスの書記素クラスタ。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> が 0 未満または <see cref="Count"/> 以上です。</exception>
        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return GetGraphemeAt(index);
            }
        }

        /// <summary>
        /// 現在の位置から次の書記素クラスタを読み取り、リーダーの位置を1つ進めます。
        /// </summary>
        /// <returns>読み取られた書記素クラスタ。末尾に到達している場合は null。</returns>
        public string? Read()
        {
            if (IsEndOfText)
            {
                return null;
            }
            var grapheme = GetGraphemeAt(Position);
            Position++;
            return grapheme;
        }

        /// <summary>
        /// 現在の位置にある書記素クラスタを、リーダーの位置を進めずに読み取ります。
        /// </summary>
        /// <returns>現在の書記素クラスタ。末尾に到達している場合は null。</returns>
        public string? Peek()
        {
            if (IsEndOfText)
            {
                return null;
            }
            return GetGraphemeAt(Position);
        }

        /// <summary>
        /// 現在の位置から次の書記素クラスタをスパンとして読み取り、リーダーの位置を1つ進めます。
        /// </summary>
        /// <returns>読み取られた書記素クラスタを表すスパン。末尾に到達している場合は <see cref="ReadOnlySpan{Char}.Empty"/>。</returns>
        public ReadOnlySpan<char> ReadAsSpan()
        {
            if (IsEndOfText)
            {
                return ReadOnlySpan<char>.Empty;
            }
            var graphemeSpan = GetGraphemeSpanAt(Position);
            Position++;
            return graphemeSpan;
        }

        /// <summary>
        /// 現在の位置にある書記素クラスタをスパンとして、リーダーの位置を進めずに読み取ります。
        /// </summary>
        /// <returns>現在の書記素クラスタを表すスパン。末尾に到達している場合は <see cref="ReadOnlySpan{Char}.Empty"/>。</returns>
        public ReadOnlySpan<char> PeekAsSpan()
        {
            if (IsEndOfText)
            {
                return ReadOnlySpan<char>.Empty;
            }
            return GetGraphemeSpanAt(Position);
        }

        /// <summary>
        /// リーダーを指定した数の書記素クラスタだけ進めます。
        /// </summary>
        /// <param name="elementCount">進める書記素クラスタの数。デフォルトは1です。</param>
        public void Advance(int elementCount = 1)
        {
            Position += elementCount;
        }

        /// <summary>
        /// 指定した範囲の書記素クラスタを単一の文字列として取得します。
        /// </summary>
        /// <param name="startGraphemeIndex">範囲の開始書記素インデックス。</param>
        /// <param name="graphemeCount">範囲に含まれる書記素クラスタの数。</param>
        /// <returns>指定された範囲の書記素クラスタから成る文字列。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startGraphemeIndex"/> または <paramref name="graphemeCount"/> が無効です。</exception>
        public string Substring(int startGraphemeIndex, int graphemeCount)
        {
            return Slice(startGraphemeIndex, graphemeCount).ToString();
        }

        /// <summary>
        /// 指定した範囲の書記素クラスタを単一のスパンとして取得します。
        /// </summary>
        /// <param name="startGraphemeIndex">範囲の開始書記素インデックス。</param>
        /// <param name="graphemeCount">範囲に含まれる書記素クラスタの数。</param>
        /// <returns>指定された範囲の書記素クラスタから成るスパン。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startGraphemeIndex"/> または <paramref name="graphemeCount"/> が無効です。</exception>
        public ReadOnlySpan<char> Slice(int startGraphemeIndex, int graphemeCount)
        {
            if (startGraphemeIndex < 0 || startGraphemeIndex > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startGraphemeIndex));
            }
            if (graphemeCount < 0 || startGraphemeIndex + graphemeCount > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(graphemeCount));
            }
            if (graphemeCount == 0)
            {
                return ReadOnlySpan<char>.Empty;
            }

            var charStartIndex = _graphemeIndexes[startGraphemeIndex];
            var endGraphemeIndex = startGraphemeIndex + graphemeCount;
            var charEndIndex = (endGraphemeIndex < Count) ? _graphemeIndexes[endGraphemeIndex] : _source.Length;

            return _source.Span.Slice(charStartIndex, charEndIndex - charStartIndex);
        }

        /// <summary>
        /// 書記素クラスタのシーケンスを反復処理する列挙子を返します。
        /// </summary>
        /// <returns>シーケンスを反復処理するために使用できる <see cref="IEnumerator{T}"/>。</returns>
        public IEnumerator<string> GetEnumerator()
        {
            for (var graphemeIndex = 0; graphemeIndex < Count; graphemeIndex++)
            {
                yield return GetGraphemeAt(graphemeIndex);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 指定したインデックスの書記素クラスタを文字列として取得します。
        /// </summary>
        /// <param name="graphemeIndex">取得する書記素クラスタのインデックス。</param>
        /// <returns>指定されたインデックスの書記素クラスタ。</returns>
        private string GetGraphemeAt(int graphemeIndex)
        {
            var startIndex = _graphemeIndexes[graphemeIndex];
            var length = GetGraphemeLength(graphemeIndex);
            return _source.Span.Slice(startIndex, length).ToString();
        }

        /// <summary>
        /// 指定したインデックスの書記素クラスタをスパンとして取得します。
        /// </summary>
        /// <param name="graphemeIndex">取得する書記素クラスタのインデックス。</param>
        /// <returns>指定されたインデックスの書記素クラスタを表すスパン。</returns>
        private ReadOnlySpan<char> GetGraphemeSpanAt(int graphemeIndex)
        {
            var startIndex = _graphemeIndexes[graphemeIndex];
            var length = GetGraphemeLength(graphemeIndex);
            return _source.Span.Slice(startIndex, length);
        }

        /// <summary>
        /// 指定したインデックスの書記素クラスタの文字数（char単位の長さ）を取得します。
        /// </summary>
        /// <param name="graphemeIndex">書記素クラスタのインデックス。</param>
        /// <returns>書記素クラスタの文字数。</returns>
        private int GetGraphemeLength(int graphemeIndex)
        {
            var startIndex = _graphemeIndexes[graphemeIndex];
            var nextIndex = (graphemeIndex + 1 < Count) ? _graphemeIndexes[graphemeIndex + 1] : _source.Length;
            return nextIndex - startIndex;
        }

        /// <summary>
        /// 指定されたテキストスパンを解析し、書記素クラスタの境界位置（char単位のインデックス）の配列を生成します。
        /// </summary>
        /// <param name="text">解析対象のテキストスパン。</param>
        /// <returns>書記素クラスタの開始位置を表すインデックスの配列。</returns>
        private static int[] ParseGraphemeBoundaries(ReadOnlySpan<char> text)
        {
            if (text.IsEmpty)
            {
                return Array.Empty<int>();
            }

            var boundaries = new List<int>();
            var textString = text.ToString();
            var enumerator = StringInfo.GetTextElementEnumerator(textString);

            while (enumerator.MoveNext())
            {
                boundaries.Add(enumerator.ElementIndex);
            }

            return boundaries.ToArray();
        }
    }
}
