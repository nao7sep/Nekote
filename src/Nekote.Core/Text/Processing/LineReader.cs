using System;
using System.Collections.Generic;

namespace Nekote.Core.Text.Processing
{
    public sealed class LineReader
    {
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

        public RawLineReader RawLineReader => _rawLineReader;
        public LineProcessor LineProcessor => _lineProcessor;
        public EmptyLineDefinition EmptyLineDefinition => _emptyLineDefinition;
        public LeadingEmptyLineHandling LeadingEmptyLineHandling => _leadingEmptyLineHandling;
        public InterstitialEmptyLineHandling InterstitialEmptyLineHandling => _interstitialEmptyLineHandling;
        public TrailingEmptyLineHandling TrailingEmptyLineHandling => _trailingEmptyLineHandling;

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

            _buffer = new char[_rawLineReader.SourceText.Length];
            _totalCharsWritten = 0;
            _pendingLines = new Queue<(int Start, int Length)>();
            _seenNonEmptyLine = false;
        }

        public void Reset()
        {
            _rawLineReader.Reset();
            _totalCharsWritten = 0;
            _pendingLines.Clear();
            _seenNonEmptyLine = false;
        }

        public bool ReadLine(out ReadOnlySpan<char> line)
        {
            if (_pendingLines.Count > 0)
            {
                line = DequeuePendingLine();
                return true;
            }

            ReadOnlySpan<char> DequeuePendingLine()
            {
                var (start, length) = _pendingLines.Dequeue();
                return _buffer.AsSpan(start, length);
            }

            bool ReadAndProcessLine(out int start, out int length, out ReadOnlySpan<char> processedLine, out bool isEmptyLine)
            {
                if (!_rawLineReader.ReadLine(out var rawLine))
                {
                    start = default;
                    length = default;
                    processedLine = default;
                    isEmptyLine = default;
                    return false;
                }

                if (!_lineProcessor.TryProcess(rawLine, _buffer.AsSpan(_totalCharsWritten), out int charsWritten))
                    throw new InvalidOperationException("Line processing failed due to insufficient buffer capacity.");

                start = _totalCharsWritten;
                length = charsWritten;
                processedLine = _buffer.AsSpan(start, length);
                _totalCharsWritten += charsWritten;

                isEmptyLine = _emptyLineDefinition switch
                {
                    EmptyLineDefinition.IsEmpty => StringHelper.IsEmpty(processedLine),
                    EmptyLineDefinition.IsWhitespace => StringHelper.IsWhiteSpace(processedLine),
                    _ => throw new InvalidOperationException($"Unsupported {nameof(EmptyLineDefinition)} value: {_emptyLineDefinition}."),
                };

                return true;
            }

            if (!ReadAndProcessLine(out var start, out var length, out var processedLine, out var isEmptyLine))
            {
                line = default;
                return false;
            }

            if (isEmptyLine)
            {
                _pendingLines.Enqueue((start, length));

                while (ReadAndProcessLine(out var nextStart, out var nextLength, out var nextProcessedLine, out var nextIsEmptyLine))
                {
                    if (nextIsEmptyLine)
                    {
                        _pendingLines.Enqueue((nextStart, nextLength));
                    }
                    else
                    {
                        if (!_seenNonEmptyLine)
                        {
                            _seenNonEmptyLine = true;

                            switch (_leadingEmptyLineHandling)
                            {
                                case LeadingEmptyLineHandling.Keep:
                                    _pendingLines.Enqueue((nextStart, nextLength));
                                    line = DequeuePendingLine();
                                    return true;
                                case LeadingEmptyLineHandling.Ignore:
                                    _pendingLines.Clear();
                                    line = nextProcessedLine;
                                    return true;
                                default:
                                    throw new InvalidOperationException($"Unsupported {nameof(LeadingEmptyLineHandling)} value: {_leadingEmptyLineHandling}.");
                            }
                        }
                        else
                        {
                            switch (_interstitialEmptyLineHandling)
                            {
                                case InterstitialEmptyLineHandling.Keep:
                                    _pendingLines.Enqueue((nextStart, nextLength));
                                    line = DequeuePendingLine();
                                    return true;
                                case InterstitialEmptyLineHandling.CollapseToOne:
                                    _pendingLines.Clear();
                                    _pendingLines.Enqueue((nextStart, nextLength));
                                    line = ReadOnlySpan<char>.Empty;
                                    return true;
                                case InterstitialEmptyLineHandling.Ignore:
                                    _pendingLines.Clear();
                                    line = nextProcessedLine;
                                    return true;
                                default:
                                    throw new InvalidOperationException($"Unsupported {nameof(InterstitialEmptyLineHandling)} value: {_interstitialEmptyLineHandling}.");
                            }
                        }
                    }
                }

                if (!_seenNonEmptyLine)
                {
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
                _seenNonEmptyLine = true;
                line = processedLine;
                return true;
            }
        }
    }
}
