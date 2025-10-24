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
                    throw new InvalidOperationException($"An undefined {nameof(LineReaderConfiguration)} value was used.");
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
        }
    }
}
