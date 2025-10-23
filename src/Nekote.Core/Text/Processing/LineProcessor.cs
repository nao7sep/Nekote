using System;
using System.Text;

namespace Nekote.Core.Text.Processing
{
    /// <summary>
    /// 一連の固定ルールに基づいて、単一行のテキストの空白を処理します。
    /// このクラスは不変で、スレッドセーフです。
    /// </summary>
    public sealed class LineProcessor
    {
        /// <summary>
        /// 一般的な使用例のための静的インスタンスを取得します。
        /// 先頭の空白を保持し、行中の空白を保持し、末尾の空白を削除します。
        /// </summary>
        public static readonly LineProcessor Default = new(LeadingWhitespaceBehavior.Keep, InternalWhitespaceBehavior.Keep, TrailingWhitespaceBehavior.Trim);

        /// <summary>
        /// 積極的な正規化のための静的インスタンスを取得します。
        /// 先頭の空白を削除し、行中の連続する空白を一つのスペースにまとめ、末尾の空白を削除します。
        /// </summary>
        public static readonly LineProcessor Aggressive = new(LeadingWhitespaceBehavior.Trim, InternalWhitespaceBehavior.CollapseToOneSpace, TrailingWhitespaceBehavior.Trim);

        /// <summary>
        /// 処理を一切行わない静的インスタンスを取得します。
        /// すべての空白を保持します。
        /// </summary>
        public static readonly LineProcessor Passthrough = new(LeadingWhitespaceBehavior.Keep, InternalWhitespaceBehavior.Keep, TrailingWhitespaceBehavior.Keep);

        private readonly LeadingWhitespaceBehavior _leadingBehavior;
        private readonly InternalWhitespaceBehavior _internalBehavior;
        private readonly TrailingWhitespaceBehavior _trailingBehavior;

        /// <summary>
        /// 新しい <see cref="LineProcessor"/> インスタンスを初期化します。
        /// </summary>
        /// <param name="leadingBehavior">行頭の空白の処理方法。</param>
        /// <param name="internalBehavior">行中の空白の処理方法。</param>
        /// <param name="trailingBehavior">行末の空白の処理方法。</param>
        public LineProcessor(LeadingWhitespaceBehavior leadingBehavior, InternalWhitespaceBehavior internalBehavior, TrailingWhitespaceBehavior trailingBehavior)
        {
            _leadingBehavior = leadingBehavior;
            _internalBehavior = internalBehavior;
            _trailingBehavior = trailingBehavior;
        }

        /// <summary>
        /// 指定された行スパンを処理し、結果の文字列を返します。
        /// このメソッドは常に新しい文字列を割り当てます。
        /// </summary>
        /// <param name="line">処理する単一行（改行文字を含まない）。</param>
        /// <returns>処理済みの文字列。</returns>
        public string Process(ReadOnlySpan<char> line)
        {
            var trimmedSpan = Trim(line);

            switch (_internalBehavior)
            {
                case InternalWhitespaceBehavior.Keep:
                    return trimmedSpan.ToString();

                case InternalWhitespaceBehavior.CollapseToOneSpace:
                {
                    if (trimmedSpan.IsEmpty)
                    {
                        return string.Empty;
                    }

                    var sb = new StringBuilder(trimmedSpan.Length);
                    var inWhitespace = false;
                    foreach (var c in trimmedSpan)
                    {
                        if (char.IsWhiteSpace(c))
                        {
                            if (!inWhitespace)
                            {
                                sb.Append(' ');
                                inWhitespace = true;
                            }
                        }
                        else
                        {
                            sb.Append(c);
                            inWhitespace = false;
                        }
                    }
                    return sb.ToString();
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(_internalBehavior));
            }
        }

        /// <summary>
        /// 指定された行スパンを処理し、結果を宛先スパンに書き込もうとします。
        /// このメソッドはアロケーションフリーです。
        /// </summary>
        /// <param name="line">処理する単一行（改行文字を含まない）。</param>
        /// <param name="destination">処理結果を書き込む宛先スパン。</param>
        /// <param name="charsWritten">宛先スパンに書き込まれた文字数。</param>
        /// <returns>処理が成功し、結果が宛先スパンに収まった場合は true。それ以外の場合は false。</returns>
        public bool TryProcess(ReadOnlySpan<char> line, Span<char> destination, out int charsWritten)
        {
            var trimmedSpan = Trim(line);

            switch (_internalBehavior)
            {
                case InternalWhitespaceBehavior.Keep:
                {
                    if (trimmedSpan.Length > destination.Length)
                    {
                        charsWritten = 0;
                        return false;
                    }
                    trimmedSpan.CopyTo(destination);
                    charsWritten = trimmedSpan.Length;
                    return true;
                }

                case InternalWhitespaceBehavior.CollapseToOneSpace:
                {
                    charsWritten = 0;
                    var inWhitespace = false;
                    foreach (var c in trimmedSpan)
                    {
                        if (char.IsWhiteSpace(c))
                        {
                            if (!inWhitespace)
                            {
                                if (charsWritten >= destination.Length)
                                {
                                    charsWritten = 0;
                                    return false;
                                }
                                destination[charsWritten++] = ' ';
                                inWhitespace = true;
                            }
                        }
                        else
                        {
                            if (charsWritten >= destination.Length)
                            {
                                charsWritten = 0;
                                return false;
                            }
                            destination[charsWritten++] = c;
                            inWhitespace = false;
                        }
                    }
                    return true;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(_internalBehavior));
            }
        }

        /// <summary>
        /// 設定された行頭および行末の空白ルールに基づいて、行スパンをトリミングします。
        /// </summary>
        /// <param name="line">トリミングする行。</param>
        /// <returns>トリミングされた行スパン。</returns>
        /// <remarks>
        /// このメソッドは、LineReader や ParagraphReader が行の空虚さを効率的に判断するために internal として公開されています。
        /// これにより、それらのクラスは完全な処理を行うことなく、行が空になるかどうかを事前に確認できます。
        /// </remarks>
        internal ReadOnlySpan<char> Trim(ReadOnlySpan<char> line)
        {
            int start = 0;
            switch (_leadingBehavior)
            {
                case LeadingWhitespaceBehavior.Keep:
                    break;
                case LeadingWhitespaceBehavior.Trim:
                    for (start = 0; start < line.Length && char.IsWhiteSpace(line[start]); start++) ;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_leadingBehavior));
            }

            int end = line.Length - 1;
            switch (_trailingBehavior)
            {
                case TrailingWhitespaceBehavior.Keep:
                    break;
                case TrailingWhitespaceBehavior.Trim:
                    for (end = line.Length - 1; end >= start && char.IsWhiteSpace(line[end]); end--) ;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_trailingBehavior));
            }

            return line[start..(end + 1)];
        }
    }
}
