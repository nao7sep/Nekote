using System;
using System.Collections.Generic;

namespace Nekote.Core.Text
{
    /// <summary>
    /// 文字列操作に関するヘルパーメソッドを提供します。
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// 指定された文字列が null または空の場合に null を返します。
        /// それ以外の場合は元の文字列を返します。
        /// </summary>
        /// <param name="value">確認する文字列。</param>
        /// <returns>null または空の場合は null、それ以外の場合は元の文字列。</returns>
        public static string? NullIfEmpty(string? value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }

        /// <summary>
        /// 指定された文字列が null、空、または空白文字のみで構成されている場合に null を返します。
        /// それ以外の場合は元の文字列を返します。
        /// </summary>
        /// <param name="value">確認する文字列。</param>
        /// <returns>null、空、または空白の場合は null、それ以外の場合は元の文字列。</returns>
        public static string? NullIfWhiteSpace(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        /// <summary>
        /// 指定された <see cref="ReadOnlySpan{T}"/> が空かどうかを判定します。
        /// </summary>
        /// <param name="value">確認する文字列スパン。</param>
        /// <returns>空の場合は true、それ以外の場合は false。</returns>
        public static bool IsEmpty(ReadOnlySpan<char> value)
        {
            // 空の場合 true を返します。
            return value.IsEmpty;
        }

        /// <summary>
        /// 指定された <see cref="ReadOnlySpan{T}"/> が空、または空白文字のみで構成されているかどうかを判定します。
        /// </summary>
        /// <param name="value">確認する文字列スパン。</param>
        /// <returns>空または空白のみの場合は true、それ以外の場合は false。</returns>
        public static bool IsWhiteSpace(ReadOnlySpan<char> value)
        {
            // 空の場合 true を返します。
            if (value.IsEmpty)
            {
                return true;
            }
            // 空白文字のみの場合 true を返します。
            foreach (var c in value)
            {
                if (!char.IsWhiteSpace(c))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 指定された行のシーケンスを、プラットフォームのデフォルトの改行シーケンスを使用して単一の文字列に結合します。
        /// </summary>
        /// <param name="lines">結合する行のシーケンス。</param>
        /// <returns>結合された文字列。</returns>
        public static string JoinLines(IEnumerable<string> lines)
        {
            return JoinLines(lines, NewlineSequence.PlatformDefault);
        }

        /// <summary>
        /// 指定された行のシーケンスを、指定された改行シーケンスを使用して単一の文字列に結合します。
        /// </summary>
        /// <param name="lines">結合する行のシーケンス。</param>
        /// <param name="sequence">使用する改行シーケンス。</param>
        /// <returns>結合された文字列。</returns>
        public static string JoinLines(IEnumerable<string> lines, NewlineSequence sequence)
        {
            var newline = sequence switch
            {
                NewlineSequence.Lf => "\n",
                NewlineSequence.CrLf => "\r\n",
                NewlineSequence.PlatformDefault => Environment.NewLine,
                _ => throw new ArgumentOutOfRangeException(nameof(sequence), sequence, "Invalid NewlineSequence value."),
            };
            return string.Join(newline, lines);
        }
    }
}
