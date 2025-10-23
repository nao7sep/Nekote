using System;
using System.Collections.Generic;

namespace Nekote.Core.Text
{
    /// <summary>
    /// テキストから行を順次読み取るためのリーダークラスです。
    /// stringではなくReadOnlyMemory<char>を使用することで、元の文字列データのコピーを作成せずに
    /// 参照のみを保持し、メモリ使用量を削減します。
    /// ReadOnlySpan<char>はスタック上の構造体であり、フィールドとして保存できないため、
    /// 各ReadLine呼び出し時に必要な部分のみをSliceで切り出して返します。
    /// このクラスは前方向のみの読み取りをサポートし、スレッドセーフではありません。
    /// </summary>
    public sealed class RawLineReader
    {
        /// <summary>
        /// 読み取り対象のソーステキストです。
        /// stringの代わりにReadOnlyMemory<char>を使用することで、
        /// 元のデータをコピーせずに参照のみを保持します。
        /// </summary>
        private readonly ReadOnlyMemory<char> _sourceText;

        /// <summary>
        /// 現在の読み取り位置（文字インデックス）です。
        /// 次に読み取る文字の位置を示します。
        /// </summary>
        private int _position;

        /// <summary>
        /// 指定されたソーステキストでRawLineReaderの新しいインスタンスを初期化します。
        /// string、char[]、Memory<char>は暗黙的にReadOnlyMemory<char>に変換されるため、
        /// これらの型を直接渡すことができます。
        /// </summary>
        /// <param name="sourceText">読み取り対象のテキスト</param>
        public RawLineReader(ReadOnlyMemory<char> sourceText)
        {
            _sourceText = sourceText;
            _position = 0;
        }

        /// <summary>
        /// 読み取り対象のソーステキストを取得します。
        /// </summary>
        public ReadOnlyMemory<char> SourceText => _sourceText;

        /// <summary>
        /// 現在の読み取り位置を取得します。
        /// </summary>
        public int Position => _position;

        /// <summary>
        /// 読み取り位置を先頭にリセットします。
        /// </summary>
        public void Reset()
        {
            _position = 0;
        }

        /// <summary>
        /// 次の行を読み取ります。
        /// CRLF（\r\n、Windows）、LF（\n、Unix/Linux/現代のmacOS）、
        /// CR（\r、古いMac OS、現在は非推奨）の改行形式に対応します。
        /// .NET標準のStringReader.ReadLineと同様に、テキストが改行文字で終わる場合でも
        /// 最後に空の行は返されません。
        /// </summary>
        /// <param name="line">読み取った行の内容（改行文字は含まれません）</param>
        /// <returns>行が正常に読み取れた場合はtrue、テキストの終端に達した場合はfalse</returns>
        public bool ReadLine(out ReadOnlySpan<char> line)
        {
            // テキストの終端に達している場合は読み取り終了
            if (_position >= _sourceText.Length)
            {
                line = default;
                return false;
            }

            // 現在位置から残りのテキストを取得
            ReadOnlySpan<char> remainingSpan = _sourceText.Span.Slice(_position);

            // 改行文字（\rまたは\n）を検索
            // IndexOfAnyは.NETランタイムによって最適化されており、大きなテキストでも効率的に動作します
            int newlineIndex = remainingSpan.IndexOfAny('\r', '\n');

            // 改行文字が見つからない場合（最後の行）
            if (newlineIndex == -1)
            {
                line = remainingSpan;
                _position = _sourceText.Length; // 位置を終端に設定
                return true;
            }

            // 改行文字の前までを行として取得
            line = remainingSpan.Slice(0, newlineIndex);

            // CRLF（\r\n）の場合は2文字分、それ以外は1文字分位置を進める
            if (remainingSpan[newlineIndex] == '\r' && newlineIndex + 1 < remainingSpan.Length && remainingSpan[newlineIndex + 1] == '\n')
            {
                // CRLF（Windows形式）の場合
                _position += newlineIndex + 2;
            }
            else
            {
                // LF（Unix形式）またはCR（Mac形式）の場合
                _position += newlineIndex + 1;
            }

            return true;
        }
    }
}
