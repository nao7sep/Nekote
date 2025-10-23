using System;
using System.Collections.Generic;

namespace Nekote.Core.Text
{
    /// <summary>
    /// <see cref="ReadOnlyMemory{T}"/> から生のテキスト行を前方専用で読み取ります。
    /// </summary>
    /// <remarks>
    /// このクラスは、<see cref="System.IO.StringReader"/> に似ていますが、スパンベースの操作に最適化されています。
    /// </remarks>
    public sealed class RawLineReader
    {
        /// <summary>
        /// 読み取り対象のソーステキスト。
        /// </summary>
        private readonly ReadOnlyMemory<char> _sourceText;

        /// <summary>
        /// <see cref="ReadLine"/> メソッドが次に読み取る行の開始インデックス。
        /// </summary>
        private int _position;

        /// <summary>
        /// <see cref="RawLineReader"/> の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="sourceText">読み取るテキストを含むメモリ。</param>
        public RawLineReader(ReadOnlyMemory<char> sourceText)
        {
            _sourceText = sourceText;
            _position = 0;
        }

        /// <summary>
        /// 読み取り対象のソーステキスト。
        /// </summary>
        public ReadOnlyMemory<char> SourceText => _sourceText;

        /// <summary>
        /// 現在の読み取り位置。
        /// </summary>
        public int Position => _position;

        /// <summary>
        /// 読み取り位置をテキストの先頭に戻します。
        /// </summary>
        public void Reset()
        {
            _position = 0;
        }

        /// <summary>
        /// テキストから次の行を読み取ります。
        /// <para>行末に改行があっても空行は返さず、<see cref="System.IO.StringReader.ReadLine"/> と同じ挙動です。</para>
        /// </summary>
        /// <param name="line">
        /// 読み取られた行（改行文字を除く）のスパン。
        /// テキストの終端に達した場合は、defaultスパンになります。
        /// </param>
        /// <returns>行が正常に読み取られた場合は true、テキストの終端に達した場合は false。</returns>
        public bool ReadLine(out ReadOnlySpan<char> line)
        {
            if (_position >= _sourceText.Length)
            {
                line = default;
                return false;
            }

            ReadOnlySpan<char> remainingSpan = _sourceText.Span.Slice(_position);
            int newlineIndex = remainingSpan.IndexOfAny('\r', '\n');

            if (newlineIndex == -1)
            {
                // 改行が見つからない場合、これが最後の行です。
                line = remainingSpan;
                _position = _sourceText.Length;
                return true;
            }

            line = remainingSpan.Slice(0, newlineIndex);

            // 見つかった改行文字が \r で、次に \n が続くか確認します (CRLF)
            if (remainingSpan[newlineIndex] == '\r' && newlineIndex + 1 < remainingSpan.Length && remainingSpan[newlineIndex + 1] == '\n')
            {
                // CRLF の場合は、2文字進めます。
                _position += newlineIndex + 2;
            }
            else
            {
                // CR または LF の場合は、1文字進めます。
                // Note: \n\r はサポートされません。\n が先に見つかった時点で改行と見なされ、
                // 次の \r は次の行の先頭文字として扱われます。
                // このシーケンスは非常に稀なため、意図的にサポートから除外しています。
                _position += newlineIndex + 1;
            }

            return true;
        }
    }
}
