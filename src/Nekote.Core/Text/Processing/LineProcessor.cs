using System;
using System.Text;

namespace Nekote.Core.Text.Processing
{
    /// <summary>
    /// テキスト行の空白文字処理を行うクラス。
    /// 行頭、行内、行末の空白文字の処理方法を設定可能で、様々な処理パターンを提供します。
    /// </summary>
    public sealed class LineProcessor
    {
        /// <summary>
        /// デフォルトの行処理インスタンス。
        /// 行頭と行内の空白文字は保持し、行末の空白文字のみトリムします。
        /// 一般的なテキスト処理に適した設定です。
        /// </summary>
        public static LineProcessor Default { get; } = new LineProcessor(
            LeadingWhitespaceBehavior.Keep,
            InternalWhitespaceBehavior.Keep,
            TrailingWhitespaceBehavior.Trim);

        /// <summary>
        /// 積極的な行処理インスタンス。
        /// 行頭と行末の空白文字をトリムし、行内の連続する空白文字を単一のスペースに圧縮します。
        /// テキストの正規化や圧縮が必要な場合に使用します。
        /// </summary>
        public static LineProcessor Aggressive { get; } = new LineProcessor(
            LeadingWhitespaceBehavior.Trim,
            InternalWhitespaceBehavior.CollapseToOneSpace,
            TrailingWhitespaceBehavior.Trim);

        /// <summary>
        /// パススルー行処理インスタンス。
        /// すべての空白文字を元のまま保持します。
        /// 元のテキストフォーマットを完全に保持したい場合に使用します。
        /// </summary>
        public static LineProcessor Passthrough { get; } = new LineProcessor(
            LeadingWhitespaceBehavior.Keep,
            InternalWhitespaceBehavior.Keep,
            TrailingWhitespaceBehavior.Keep);

        /// <summary>行頭の空白文字処理動作を定義するフィールド</summary>
        private readonly LeadingWhitespaceBehavior _leadingBehavior;

        /// <summary>行内の空白文字処理動作を定義するフィールド</summary>
        private readonly InternalWhitespaceBehavior _internalBehavior;

        /// <summary>行末の空白文字処理動作を定義するフィールド</summary>
        private readonly TrailingWhitespaceBehavior _trailingBehavior;

        /// <summary>
        /// 指定された空白文字処理動作でLineProcessorの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="leadingBehavior">行頭の空白文字処理動作</param>
        /// <param name="internalBehavior">行内の空白文字処理動作</param>
        /// <param name="trailingBehavior">行末の空白文字処理動作</param>
        public LineProcessor(
            LeadingWhitespaceBehavior leadingBehavior,
            InternalWhitespaceBehavior internalBehavior,
            TrailingWhitespaceBehavior trailingBehavior)
        {
            _leadingBehavior = leadingBehavior;
            _internalBehavior = internalBehavior;
            _trailingBehavior = trailingBehavior;
        }

        /// <summary>行頭の空白文字処理動作を取得します。</summary>
        public LeadingWhitespaceBehavior LeadingBehavior => _leadingBehavior;

        /// <summary>行内の空白文字処理動作を取得します。</summary>
        public InternalWhitespaceBehavior InternalBehavior => _internalBehavior;

        /// <summary>行末の空白文字処理動作を取得します。</summary>
        public TrailingWhitespaceBehavior TrailingBehavior => _trailingBehavior;

        /// <summary>
        /// 指定された行を設定された空白文字処理動作に従って処理し、結果を文字列として返します。
        /// 注意：行内空白文字の圧縮処理（CollapseToOneSpace）を行う場合、新しい文字列インスタンスが
        /// 割り当てられます。トリムのみの場合はメモリ割り当てを行わずにSpanの操作のみで処理されます。
        /// メモリ割り当てを避けたい場合は<see cref="TryProcess"/>メソッドを使用してください。
        /// </summary>
        /// <param name="line">処理対象の行</param>
        /// <returns>処理された行の文字列</returns>
        /// <exception cref="InvalidOperationException">未定義のInternalWhitespaceBehavior値が使用された場合</exception>
        public string Process(ReadOnlySpan<char> line)
        {
            // 行頭・行末の空白文字処理を適用
            ReadOnlySpan<char> trimmedSpan = TrimSpan(line);

            // 行内の空白文字処理動作に応じて処理を分岐
            switch (_internalBehavior)
            {
                case InternalWhitespaceBehavior.Keep:
                    // 行内の空白文字をそのまま保持
                    return trimmedSpan.ToString();
                case InternalWhitespaceBehavior.CollapseToOneSpace:
                    // 行内の連続する空白文字を単一のスペースに圧縮
                    return CollapseInternalWhitespace(trimmedSpan);
                default:
                    throw new InvalidOperationException($"An undefined {nameof(InternalWhitespaceBehavior)} value was used.");
            }
        }

        /// <summary>
        /// 指定された行を処理し、結果を指定されたSpanに書き込みます。
        /// 書き込み先のバッファが不足している場合や未定義の動作値が使用された場合はfalseを返します。
        /// 重要：このメソッドはいかなる処理においてもメモリ割り当てを行いません。
        /// 高性能が要求される場面や、メモリ割り当てを避けたい場合に使用してください。
        /// </summary>
        /// <param name="line">処理対象の行</param>
        /// <param name="destination">処理結果の書き込み先バッファ</param>
        /// <param name="charsWritten">実際に書き込まれた文字数</param>
        /// <returns>処理が成功した場合はtrue、バッファ不足や未定義の動作値の場合はfalse</returns>
        public bool TryProcess(ReadOnlySpan<char> line, Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;

            // 行頭・行末の空白文字処理を適用
            ReadOnlySpan<char> trimmedSpan = TrimSpan(line);

            // 行内の空白文字処理動作に応じて処理を分岐
            switch (_internalBehavior)
            {
                case InternalWhitespaceBehavior.Keep:
                    // 行内の空白文字をそのまま保持する場合
                    if (trimmedSpan.Length > destination.Length)
                    {
                        // バッファサイズが不足している場合は失敗を返す
                        return false;
                    }
                    trimmedSpan.CopyTo(destination);
                    charsWritten = trimmedSpan.Length;
                    return true;

                case InternalWhitespaceBehavior.CollapseToOneSpace:
                    // 行内の連続する空白文字を単一のスペースに圧縮する場合
                    return TryCollapseInternalWhitespace(trimmedSpan, destination, out charsWritten);

                default:
                    // 未定義の動作値の場合は失敗を返す
                    return false;
            }
        }

        /// <summary>
        /// 設定された行頭・行末の空白文字処理動作に従って、指定されたSpanをトリムします。
        /// </summary>
        /// <param name="line">処理対象の行</param>
        /// <returns>トリム処理が適用された行のSpan</returns>
        /// <exception cref="InvalidOperationException">未定義のLeadingWhitespaceBehaviorまたはTrailingWhitespaceBehavior値が使用された場合</exception>
        private ReadOnlySpan<char> TrimSpan(ReadOnlySpan<char> line)
        {
            ReadOnlySpan<char> result = line;

            // 行頭の空白文字処理動作に応じて処理
            switch (_leadingBehavior)
            {
                case LeadingWhitespaceBehavior.Keep:
                    // 行頭の空白文字を保持（何もしない）
                    break;
                case LeadingWhitespaceBehavior.Trim:
                    // 行頭の空白文字をトリム
                    result = result.TrimStart();
                    break;
                default:
                    throw new InvalidOperationException($"An undefined {nameof(LeadingWhitespaceBehavior)} value was used.");
            }

            // 行末の空白文字処理動作に応じて処理
            switch (_trailingBehavior)
            {
                case TrailingWhitespaceBehavior.Keep:
                    // 行末の空白文字を保持（何もしない）
                    break;
                case TrailingWhitespaceBehavior.Trim:
                    // 行末の空白文字をトリム
                    result = result.TrimEnd();
                    break;
                default:
                    throw new InvalidOperationException($"An undefined {nameof(TrailingWhitespaceBehavior)} value was used.");
            }

            return result;
        }

        /// <summary>
        /// 指定されたSpan内の連続する空白文字を単一のスペースに圧縮し、結果を文字列として返します。
        /// 注意：このメソッドはStringBuilderを使用して新しい文字列インスタンスを作成するため、
        /// メモリ割り当てが発生します。
        /// </summary>
        /// <param name="span">処理対象の文字Span</param>
        /// <returns>空白文字が圧縮された文字列</returns>
        private static string CollapseInternalWhitespace(ReadOnlySpan<char> span)
        {
            if (span.IsEmpty)
            {
                return string.Empty;
            }

            // 元の長さでStringBuilderを初期化（最大でも元の長さを超えない）
            var stringBuilder = new StringBuilder(span.Length);
            bool inWhitespace = false; // 現在空白文字の連続内にいるかを追跡

            foreach (char character in span)
            {
                if (char.IsWhiteSpace(character))
                {
                    // 空白文字の場合
                    if (!inWhitespace)
                    {
                        // 空白文字の連続の最初の文字の場合のみスペースを追加
                        stringBuilder.Append(' ');
                        inWhitespace = true;
                    }
                    // 既に空白文字の連続内にいる場合は何もしない（圧縮）
                }
                else
                {
                    // 非空白文字の場合
                    stringBuilder.Append(character);
                    inWhitespace = false; // 空白文字の連続を終了
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 指定されたSpan内の連続する空白文字を単一のスペースに圧縮し、結果を指定されたSpanに書き込みます。
        /// 書き込み先のバッファが不足している場合はfalseを返します。
        /// 重要：このメソッドはメモリ割り当てを一切行わず、提供されたバッファに直接書き込みます。
        /// </summary>
        /// <param name="span">処理対象の文字Span</param>
        /// <param name="destination">処理結果の書き込み先バッファ</param>
        /// <param name="charsWritten">実際に書き込まれた文字数</param>
        /// <returns>処理が成功した場合はtrue、バッファ不足の場合はfalse</returns>
        private static bool TryCollapseInternalWhitespace(ReadOnlySpan<char> span, Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;

            if (span.IsEmpty)
            {
                // 空のSpanの場合は成功を返す（書き込み文字数は0）
                return true;
            }

            int writeIndex = 0; // 書き込み位置のインデックス
            bool inWhitespace = false; // 現在空白文字の連続内にいるかを追跡

            foreach (char character in span)
            {
                if (char.IsWhiteSpace(character))
                {
                    // 空白文字の場合
                    if (!inWhitespace)
                    {
                        // 空白文字の連続の最初の文字の場合のみスペースを書き込み
                        if (writeIndex >= destination.Length)
                        {
                            // バッファサイズが不足している場合は失敗を返す
                            charsWritten = 0;
                            return false;
                        }
                        destination[writeIndex++] = ' ';
                        inWhitespace = true;
                    }
                    // 既に空白文字の連続内にいる場合は何もしない（圧縮）
                }
                else
                {
                    // 非空白文字の場合
                    if (writeIndex >= destination.Length)
                    {
                        // バッファサイズが不足している場合は失敗を返す
                        charsWritten = 0;
                        return false;
                    }
                    destination[writeIndex++] = character;
                    inWhitespace = false; // 空白文字の連続を終了
                }
            }

            charsWritten = writeIndex;
            return true;
        }
    }
}
