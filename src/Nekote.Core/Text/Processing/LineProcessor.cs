using System;
using System.Text;

namespace Nekote.Core.Text.Processing
{
    /// <summary>
    /// 個々の行の空白を処理するためのルールを定義し、実行します。
    /// このクラスは不変（immutable）であり、スレッドセーフです。
    /// </summary>
    public sealed class LineProcessor
    {
        /// <summary>
        /// 先頭の空白を保持し、行内の空白も保持し、末尾の空白のみをトリムするデフォルトの動作。
        /// </summary>
        public static LineProcessor Default { get; } = new LineProcessor(
            LeadingWhitespaceBehavior.Keep,
            InternalWhitespaceBehavior.Keep,
            TrailingWhitespaceBehavior.Trim);

        /// <summary>
        /// すべての空白をトリムし、行内の空白を単一スペースにまとめる、最も積極的な正規化。
        /// </summary>
        public static LineProcessor Aggressive { get; } = new LineProcessor(
            LeadingWhitespaceBehavior.Trim,
            InternalWhitespaceBehavior.CollapseToOneSpace,
            TrailingWhitespaceBehavior.Trim);

        /// <summary>
        /// 空白を一切変更しない不動のプロセッサ。
        /// </summary>
        public static LineProcessor Passthrough { get; } = new LineProcessor(
            LeadingWhitespaceBehavior.Keep,
            InternalWhitespaceBehavior.Keep,
            TrailingWhitespaceBehavior.Keep);

        private readonly LeadingWhitespaceBehavior _leadingBehavior;
        private readonly InternalWhitespaceBehavior _internalBehavior;
        private readonly TrailingWhitespaceBehavior _trailingBehavior;

        /// <summary>
        /// 空白処理の動作を指定して、<see cref="LineProcessor"/> の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="leadingBehavior">先頭の空白の処理方法。</param>
        /// <param name="internalBehavior">行内の空白の処理方法。</param>
        /// <param name="trailingBehavior">末尾の空白の処理方法。</param>
        public LineProcessor(
            LeadingWhitespaceBehavior leadingBehavior,
            InternalWhitespaceBehavior internalBehavior,
            TrailingWhitespaceBehavior trailingBehavior)
        {
            _leadingBehavior = leadingBehavior;
            _internalBehavior = internalBehavior;
            _trailingBehavior = trailingBehavior;
        }

        /// <summary>
        /// 指定されたルールに基づいて、入力行スパンを処理し、新しい文字列を返します。
        /// このメソッドは、結果として新しい文字列を割り当てます。
        /// </summary>
        /// <param name="line">処理対象の行を表すスパン。</param>
        /// <returns>処理済みの文字列。</returns>
        public string Process(ReadOnlySpan<char> line)
        {
            ReadOnlySpan<char> trimmedSpan = TrimSpan(line);

            // 内部空白の処理方法に応じて分岐
            switch (_internalBehavior)
            {
                case InternalWhitespaceBehavior.Keep:
                    return trimmedSpan.ToString();
                case InternalWhitespaceBehavior.CollapseToOneSpace:
                    return CollapseInternalWhitespace(trimmedSpan);
                default:
                    // 未定義の列挙値が指定された場合は例外をスロー
                    throw new InvalidOperationException($"An undefined {nameof(InternalWhitespaceBehavior)} value was used.");
            }
        }

        /// <summary>
        /// 指定されたルールに基づいて、入力行スパンを処理し、結果を書き込み先スパンに書き込みます。
        /// このメソッドは、ヒープ割り当てを試みません。
        /// </summary>
        /// <param name="line">処理対象の行を表すスパン。</param>
        /// <param name="destination">処理結果を書き込むスパン。</param>
        /// <param name="charsWritten">
        /// 処理が成功した場合に、<paramref name="destination"/> に書き込まれた文字数。
        /// </param>
        /// <returns>
        /// 処理が成功し、結果が <paramref name="destination"/> に収まった場合は true。
        /// <paramref name="destination"/> が小さすぎる場合は false。
        /// </returns>
        public bool TryProcess(ReadOnlySpan<char> line, Span<char> destination, out int charsWritten)
        {
            ReadOnlySpan<char> trimmedSpan = TrimSpan(line);

            // 内部空白の処理方法に応じて分岐
            switch (_internalBehavior)
            {
                case InternalWhitespaceBehavior.Keep:
                    if (trimmedSpan.Length > destination.Length)
                    {
                        charsWritten = 0;
                        return false;
                    }
                    trimmedSpan.CopyTo(destination);
                    charsWritten = trimmedSpan.Length;
                    return true;
                case InternalWhitespaceBehavior.CollapseToOneSpace:
                    return TryCollapseInternalWhitespace(trimmedSpan, destination, out charsWritten);
                default:
                    // 未定義の列挙値が指定された場合は例外をスロー
                    throw new InvalidOperationException($"An undefined {nameof(InternalWhitespaceBehavior)} value was used.");
            }
        }

        /// <summary>
        /// 先頭と末尾のトリミングルールをスパンに適用します。割り当ては行いません。
        /// </summary>
        private ReadOnlySpan<char> TrimSpan(ReadOnlySpan<char> line)
        {
            ReadOnlySpan<char> result = line;

            // 先頭の空白処理
            switch (_leadingBehavior)
            {
                case LeadingWhitespaceBehavior.Keep:
                    // 何もしない
                    break;
                case LeadingWhitespaceBehavior.Trim:
                    result = result.TrimStart();
                    break;
                default:
                    throw new InvalidOperationException($"An undefined {nameof(LeadingWhitespaceBehavior)} value was used.");
            }

            // 末尾の空白処理
            switch (_trailingBehavior)
            {
                case TrailingWhitespaceBehavior.Keep:
                    // 何もしない
                    break;
                case TrailingWhitespaceBehavior.Trim:
                    result = result.TrimEnd();
                    break;
                default:
                    throw new InvalidOperationException($"An undefined {nameof(TrailingWhitespaceBehavior)} value was used.");
            }

            return result;
        }

        /// <summary>
        /// スパン内の連続する空白を単一のスペースに置き換えます。（割り当てあり）
        /// </summary>
        private static string CollapseInternalWhitespace(ReadOnlySpan<char> span)
        {
            if (span.IsEmpty)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder(span.Length);
            bool inWhitespace = false;

            foreach (char c in span)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!inWhitespace)
                    {
                        stringBuilder.Append(' ');
                        inWhitespace = true;
                    }
                }
                else
                {
                    stringBuilder.Append(c);
                    inWhitespace = false;
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// スパン内の連続する空白を単一のスペースに置き換え、結果をdestinationに書き込みます。（非割り当て）
        /// </summary>
        private static bool TryCollapseInternalWhitespace(ReadOnlySpan<char> span, Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;
            bool inWhitespace = false;

            foreach (char c in span)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (inWhitespace)
                    {
                        continue;
                    }

                    if (charsWritten >= destination.Length)
                    {
                        charsWritten = 0;
                        return false;
                    }

                    destination[charsWritten] = ' ';
                    charsWritten++;
                    inWhitespace = true;
                }
                else
                {
                    if (charsWritten >= destination.Length)
                    {
                        charsWritten = 0;
                        return false;
                    }

                    destination[charsWritten] = c;
                    charsWritten++;
                    inWhitespace = false;
                }
            }

            return true;
        }
    }
}
