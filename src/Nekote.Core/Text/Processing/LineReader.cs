using System;
using System.Collections.Generic;

namespace Nekote.Core.Text.Processing
{
    /// <summary>
    /// <see cref="RawLineReader"/> から読み取った行に対して、より高度な処理を適用します。
    /// これには、行のトリミング、空行の定義、および先頭、中間、末尾の空行の取り扱いが含まれます。
    /// このクラスは、特定の構成に基づいて <see cref="LineProcessor"/> を使用して行を処理し、
    /// 最終的な行のシーケンスを生成します。
    /// </summary>
    /// <remarks>
    /// メモリ使用量に関する注意:
    /// このクラスは内部的にソーステキストと同じサイズのバッファを割り当てます。
    /// これは先読み機能と複雑な空行処理を実現するための設計上の選択です。
    /// 想定される用途は、ユーザー入力、Webフォーム、設定ファイルなどの小〜中規模のテキスト正規化です。
    ///
    /// 大容量ファイルを処理する場合や、多数のテキストを同時に処理する場合は、
    /// <see cref="RawLineReader"/> と <see cref="LineProcessor"/> を直接使用して、
    /// 行ごとのストリーミング処理を実装することを検討してください。
    /// この方法では先読み機能は利用できませんが、メモリ使用量を最小限に抑えることができます。
    /// </remarks>
    public sealed class LineReader
    {
        /// <summary>
        /// 指定された構成と <see cref="RawLineReader"/> を使用して <see cref="LineReader"/> の新しいインスタンスを作成します。
        /// </summary>
        /// <param name="configuration">使用する行読み取り構成。</param>
        /// <param name="rawLineReader">行のソースを提供する <see cref="RawLineReader"/>。</param>
        /// <returns>新しく作成された <see cref="LineReader"/> インスタンス。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rawLineReader"/> が null です。</exception>
        /// <exception cref="InvalidOperationException">サポートされていない <see cref="LineReaderConfiguration"/> 値が指定されました。</exception>
        public static LineReader Create(LineReaderConfiguration configuration, RawLineReader rawLineReader)
        {
            if (rawLineReader == null)
                throw new ArgumentNullException(nameof(rawLineReader));

            switch (configuration)
            {
                case LineReaderConfiguration.Default:
                    return new LineReader(
                        rawLineReader,
                        LineProcessor.Default,
                        EmptyLineDefinition.IsWhitespace,
                        LeadingEmptyLineHandling.Ignore,
                        InterstitialEmptyLineHandling.CollapseToOne,
                        TrailingEmptyLineHandling.Ignore);

                case LineReaderConfiguration.Aggressive:
                    return new LineReader(
                        rawLineReader,
                        LineProcessor.Aggressive,
                        EmptyLineDefinition.IsWhitespace,
                        LeadingEmptyLineHandling.Ignore,
                        InterstitialEmptyLineHandling.CollapseToOne,
                        TrailingEmptyLineHandling.Ignore);

                case LineReaderConfiguration.Passthrough:
                    return new LineReader(
                        rawLineReader,
                        LineProcessor.Passthrough,
                        EmptyLineDefinition.IsEmpty,
                        LeadingEmptyLineHandling.Keep,
                        InterstitialEmptyLineHandling.Keep,
                        TrailingEmptyLineHandling.Keep);

                default:
                    throw new InvalidOperationException($"Unsupported {nameof(LineReaderConfiguration)} value: {configuration}.");
            }
        }

        /// <summary>
        /// 指定された構成とソーステキストを使用して <see cref="LineReader"/> の新しいインスタンスを作成します。
        /// </summary>
        /// <param name="configuration">使用する行読み取り構成。</param>
        /// <param name="sourceText">処理するソーステキスト。</param>
        /// <returns>新しく作成された <see cref="LineReader"/> インスタンス。</returns>
        public static LineReader Create(LineReaderConfiguration configuration, ReadOnlyMemory<char> sourceText)
        {
            var rawLineReader = new RawLineReader(sourceText);
            return Create(configuration, rawLineReader);
        }

        private readonly RawLineReader _rawLineReader;
        private readonly LineProcessor _lineProcessor;
        private readonly EmptyLineDefinition _emptyLineDefinition;
        private readonly LeadingEmptyLineHandling _leadingEmptyLineHandling;
        private readonly InterstitialEmptyLineHandling _interstitialEmptyLineHandling;
        private readonly TrailingEmptyLineHandling _trailingEmptyLineHandling;

        private readonly char[] _buffer;
        private int _totalCharsWritten;
        private readonly Queue<(int Start, int Length)> _pendingLines;
        private bool _seenNonEmptyLine;

        /// <summary>
        /// このリーダーに関連付けられている <see cref="RawLineReader"/> を取得します。
        /// </summary>
        public RawLineReader RawLineReader => _rawLineReader;

        /// <summary>
        /// このリーダーで使用される <see cref="LineProcessor"/> を取得します。
        /// </summary>
        public LineProcessor LineProcessor => _lineProcessor;

        /// <summary>
        /// 空行を定義する方法を取得します。
        /// </summary>
        public EmptyLineDefinition EmptyLineDefinition => _emptyLineDefinition;

        /// <summary>
        /// 先頭の空行の処理方法を取得します。
        /// </summary>
        public LeadingEmptyLineHandling LeadingEmptyLineHandling => _leadingEmptyLineHandling;

        /// <summary>
        /// 中間の空行の処理方法を取得します。
        /// </summary>
        public InterstitialEmptyLineHandling InterstitialEmptyLineHandling => _interstitialEmptyLineHandling;

        /// <summary>
        /// 末尾の空行の処理方法を取得します。
        /// </summary>
        public TrailingEmptyLineHandling TrailingEmptyLineHandling => _trailingEmptyLineHandling;

        /// <summary>
        /// <see cref="LineReader"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="rawLineReader">行のソースを提供する <see cref="RawLineReader"/>。</param>
        /// <param name="lineProcessor">各行を処理するために使用される <see cref="LineProcessor"/>。</param>
        /// <param name="emptyLineDefinition">空行を定義する方法。</param>
        /// <param name="leadingEmptyLineHandling">先頭の空行の処理方法。</param>
        /// <param name="interstitialEmptyLineHandling">中間の空行の処理方法。</param>
        /// <param name="trailingEmptyLineHandling">末尾の空行の処理方法。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rawLineReader"/> または <paramref name="lineProcessor"/> が null です。</exception>
        public LineReader(
            RawLineReader rawLineReader,
            LineProcessor lineProcessor,
            EmptyLineDefinition emptyLineDefinition,
            LeadingEmptyLineHandling leadingEmptyLineHandling,
            InterstitialEmptyLineHandling interstitialEmptyLineHandling,
            TrailingEmptyLineHandling trailingEmptyLineHandling)
        {
            _rawLineReader = rawLineReader ?? throw new ArgumentNullException(nameof(rawLineReader));
            _lineProcessor = lineProcessor ?? throw new ArgumentNullException(nameof(lineProcessor));
            _emptyLineDefinition = emptyLineDefinition;
            _leadingEmptyLineHandling = leadingEmptyLineHandling;
            _interstitialEmptyLineHandling = interstitialEmptyLineHandling;
            _trailingEmptyLineHandling = trailingEmptyLineHandling;

            // バッファはソーステキスト全体と同じサイズで割り当てられる。
            // これは以下の理由により必要:
            // 1. 処理済みの行はこのバッファに順次追記され、_totalCharsWritten で位置を追跡する
            // 2. _pendingLines キューは複数の行のバッファ内位置 (Start, Length) を同時に保持する
            // 3. ReadLine が返す ReadOnlySpan<char> はこのバッファを直接参照するため、
            //    保留中または返却済みの行の内容はバッファ内で有効なまま保持される必要がある
            // 4. 最悪の場合 (Passthrough 構成で全体が空行のシーケンスなど)、
            //    テキスト全体をバッファに保持する必要がある
            //
            // 代替案として ArrayPool<char> を使用する設計も検討されたが、以下の理由により採用されなかった:
            // - 先読みロジックを維持するには、保留中の行を string として保存する必要がある
            // - ReadOnlySpan<char> を返す API を維持しながら ArrayPool を使用すると、
            //   返された span の生存期間が次の ReadLine() 呼び出しまでに制限され、API が危険になる
            // - 非常に長い行が出現した場合、バッファのサイズ調整（より大きなバッファの再レンタル）が必要になり、
            //   実装が複雑化する
            // - 小〜中規模のテキスト処理という想定用途では、全サイズバッファの割り当ては許容範囲内
            //
            // このクラスは小〜中規模のテキスト正規化 (ユーザー入力、Webフォームなど) を
            // 想定しており、大容量ファイル処理には適さない。
            _buffer = new char[_rawLineReader.SourceText.Length];
            _totalCharsWritten = 0;
            _pendingLines = new Queue<(int Start, int Length)>();
            _seenNonEmptyLine = false;
        }

        /// <summary>
        /// リーダーの状態をリセットして、最初から読み取りを再開できるようにします。
        /// </summary>
        public void Reset()
        {
            _rawLineReader.Reset();
            _totalCharsWritten = 0;
            _pendingLines.Clear();
            _seenNonEmptyLine = false;
        }

        /// <summary>
        /// テキストから次の行を読み取ります。
        /// </summary>
        /// <param name="line">このメソッドが戻るとき、次の行が含まれます。行が利用できない場合は空です。</param>
        /// <returns>行が正常に読み取られた場合は true。それ以外の場合は false。</returns>
        /// <exception cref="InvalidOperationException">
        /// 行の処理がバッファ容量不足で失敗した場合、またはサポートされていない列挙値が検出された場合にスローされます。
        /// </exception>
        public bool ReadLine(out ReadOnlySpan<char> line)
        {
            // 保留中の行キューに処理済みの行が残っている場合、まずそれを返す。
            // これは、先行読み込みや空行の集約などによって発生する。
            if (_pendingLines.Count > 0)
            {
                line = DequeuePendingLine();
                return true;
            }

            // 保留キューから次の行を取り出すヘルパーメソッド。
            ReadOnlySpan<char> DequeuePendingLine()
            {
                var (start, length) = _pendingLines.Dequeue();
                return _buffer.AsSpan(start, length);
            }

            // RawLineReaderから行を読み取り、LineProcessorで処理し、バッファに書き込むヘルパーメソッド。
            bool ReadAndProcessLine(out int start, out int length, out ReadOnlySpan<char> processedLine, out bool isEmptyLine)
            {
                // ソースから生の行を読み取る。
                if (!_rawLineReader.ReadLine(out var rawLine))
                {
                    start = default;
                    length = default;
                    processedLine = default;
                    isEmptyLine = default;
                    return false;
                }

                // 行プロセッサ（トリミングなど）を適用し、結果を内部バッファに書き込む。
                if (!_lineProcessor.TryProcess(rawLine, _buffer.AsSpan(_totalCharsWritten), out int charsWritten))
                    throw new InvalidOperationException("Line processing failed due to insufficient buffer capacity.");

                start = _totalCharsWritten;
                length = charsWritten;
                processedLine = _buffer.AsSpan(start, length);
                _totalCharsWritten += charsWritten;

                // 処理後の行が「空」と見なされるかどうかを定義に基づいて判断する。
                isEmptyLine = _emptyLineDefinition switch
                {
                    EmptyLineDefinition.IsEmpty => StringHelper.IsEmpty(processedLine),
                    EmptyLineDefinition.IsWhitespace => StringHelper.IsWhiteSpace(processedLine),
                    _ => throw new InvalidOperationException($"Unsupported {nameof(EmptyLineDefinition)} value: {_emptyLineDefinition}."),
                };

                return true;
            }

            // 次の行を読み込んで処理する。
            if (!ReadAndProcessLine(out var start, out var length, out var processedLine, out var isEmptyLine))
            {
                // テキストの終端に達した。
                line = default;
                return false;
            }

            // 読み取った行が空かどうかでロジックが分岐する。
            if (isEmptyLine)
            {
                // 読み取った空行をまず保留キューに追加する。
                _pendingLines.Enqueue((start, length));

                // 後続の行を先読みして、空行のシーケンスを処理する。
                while (ReadAndProcessLine(out var nextStart, out var nextLength, out var nextProcessedLine, out var nextIsEmptyLine))
                {
                    if (nextIsEmptyLine)
                    {
                        // 次の行も空行であれば、キューに追加してループを続ける。
                        _pendingLines.Enqueue((nextStart, nextLength));
                    }
                    else
                    {
                        // 空行ではない行が見つかった。これは空行シーケンスの終わりを意味する。
                        // これがテキスト内で最初に見つかった非空行かどうかで処理が異なる。
                        if (!_seenNonEmptyLine)
                        {
                            // まだ非空行を見ていない場合、これは「先頭の」空行シーケンスである。
                            _seenNonEmptyLine = true;

                            switch (_leadingEmptyLineHandling)
                            {
                                case LeadingEmptyLineHandling.Keep:
                                    // 空行を保持し、現在の非空行もキューに入れる。
                                    _pendingLines.Enqueue((nextStart, nextLength));
                                    // 最初の空行を返す。
                                    line = DequeuePendingLine();
                                    return true;
                                case LeadingEmptyLineHandling.Ignore:
                                    // 先頭の空行はすべて無視する。
                                    _pendingLines.Clear();
                                    // 現在の非空行を直接返す。
                                    line = nextProcessedLine;
                                    return true;
                                default:
                                    throw new InvalidOperationException($"Unsupported {nameof(LeadingEmptyLineHandling)} value: {_leadingEmptyLineHandling}.");
                            }
                        }
                        else
                        {
                            // すでに非空行を見ている場合、これは「中間の」空行シーケンスである。
                            switch (_interstitialEmptyLineHandling)
                            {
                                case InterstitialEmptyLineHandling.Keep:
                                    // すべての空行を保持する。
                                    _pendingLines.Enqueue((nextStart, nextLength));
                                    line = DequeuePendingLine();
                                    return true;
                                case InterstitialEmptyLineHandling.CollapseToOne:
                                    // 複数の空行を1つにまとめる。
                                    _pendingLines.Clear();
                                    _pendingLines.Enqueue((nextStart, nextLength));
                                    // 戻り値として単一の空行を返す。
                                    line = ReadOnlySpan<char>.Empty;
                                    return true;
                                case InterstitialEmptyLineHandling.Ignore:
                                    // 中間の空行はすべて無視する。
                                    _pendingLines.Clear();
                                    line = nextProcessedLine;
                                    return true;
                                default:
                                    throw new InvalidOperationException($"Unsupported {nameof(InterstitialEmptyLineHandling)} value: {_interstitialEmptyLineHandling}.");
                            }
                        }
                    }
                }

                // whileループが終了した場合、テキストの終端に達したことを意味する。
                // この時点でキューに残っているのは、先頭または末尾の空行のみ。
                if (!_seenNonEmptyLine)
                {
                    // テキスト全体が空行、または空だった場合。
                    switch (_leadingEmptyLineHandling)
                    {
                        case LeadingEmptyLineHandling.Keep:
                            line = DequeuePendingLine();
                            return true;
                        case LeadingEmptyLineHandling.Ignore:
                            _pendingLines.Clear();
                            line = default;
                            return false;
                        default:
                            throw new InvalidOperationException($"Unsupported {nameof(LeadingEmptyLineHandling)} value: {_leadingEmptyLineHandling}.");
                    }
                }
                else
                {
                    // これらは「末尾の」空行である。
                    switch (_trailingEmptyLineHandling)
                    {
                        case TrailingEmptyLineHandling.Keep:
                            line = DequeuePendingLine();
                            return true;
                        case TrailingEmptyLineHandling.Ignore:
                            _pendingLines.Clear();
                            line = default;
                            return false;
                        default:
                            throw new InvalidOperationException($"Unsupported {nameof(TrailingEmptyLineHandling)} value: {_trailingEmptyLineHandling}.");
                    }
                }
            }
            else
            {
                // 読み取った行が空でない場合、最もシンプルなケース。
                // 非空行を見たことを記録し、その行を返す。
                _seenNonEmptyLine = true;
                line = processedLine;
                return true;
            }
        }
    }
}
