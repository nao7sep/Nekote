using System;
using System.Collections.Generic;
using Nekote.Core.Environment;

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
        /// <remarks>
        /// 注意: このメソッドは意図的に IsNullOrEmpty を使用しています。IsNullOrWhiteSpace を使用すると、
        /// 空白文字のみの文字列をチェックする NullIfWhiteSpace メソッドとの目的が重複してしまうためです。
        /// 両メソッドは異なる用途を持ち、使い分けが必要です。
        /// </remarks>
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
        /// <remarks>
        /// 注: このメソッドは <see cref="ReadOnlySpan{T}.IsEmpty"/> プロパティの単純なラッパーですが、
        /// <see cref="IsWhiteSpace(ReadOnlySpan{char})"/> とペアで使用するために意図的に提供されています。
        /// これにより、一貫したAPI設計と使いやすさが向上します。
        /// </remarks>
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
            foreach (var character in value)
            {
                if (!char.IsWhiteSpace(character))
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
            if (lines is null)
            {
                throw new ArgumentNullException(nameof(lines));
            }
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
            if (lines is null)
            {
                throw new ArgumentNullException(nameof(lines));
            }
            var newline = sequence switch
            {
                NewlineSequence.Lf => "\n",
                NewlineSequence.CrLf => "\r\n",
                NewlineSequence.PlatformDefault => PlatformInfo.NewLine,
                _ => throw new ArgumentOutOfRangeException(nameof(sequence), sequence, "Invalid NewlineSequence value."),
            };
            return string.Join(newline, lines);
        }

        /// <summary>
        /// 指定された文字列を行ごとに効率的に列挙します。
        /// RawLineReaderを使用してメモリ効率的な行の読み取りを行います。
        /// </summary>
        /// <param name="text">列挙する文字列。</param>
        /// <returns>行を列挙する <see cref="IEnumerable{String}"/>。</returns>
        public static IEnumerable<string> EnumerateLines(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            return EnumerateLines(text.AsMemory());
        }

        /// <summary>
        /// 指定されたメモリ領域を行ごとに効率的に列挙します。
        /// RawLineReaderを使用してメモリ効率的な行の読み取りを行います。
        /// このメソッドは遅延実行を使用し、巨大なテキストを処理する際のメモリ効率を高めます。
        /// </summary>
        /// <param name="text">列挙するテキストを含むメモリ領域。</param>
        /// <returns>行を列挙する <see cref="IEnumerable{String}"/>。</returns>
        public static IEnumerable<string> EnumerateLines(ReadOnlyMemory<char> text)
        {
            if (text.IsEmpty)
            {
                yield break;
            }

            var reader = new RawLineReader(text);

            while (reader.ReadLine(out var line))
            {
                yield return new string(line);
            }
        }

        /// <summary>
        /// 指定された文字列を行ごとに分割します。
        /// RawLineReaderを使用してメモリ効率的な行の読み取りを行います。
        /// </summary>
        /// <param name="text">分割する文字列。</param>
        /// <returns>分割された行を含む文字列の配列。</returns>
        public static string[] SplitLines(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            return SplitLines(text.AsMemory());
        }

        /// <summary>
        /// 指定されたメモリ領域を行ごとに分割します。
        /// パフォーマンス最適化のため、RawLineReaderを直接使用して実装されています。
        /// </summary>
        /// <param name="text">分割するテキストを含むメモリ領域。</param>
        /// <returns>分割された行を含む文字列の配列。</returns>
        public static string[] SplitLines(ReadOnlyMemory<char> text)
        {
            if (text.IsEmpty)
            {
                return Array.Empty<string>();
            }

            var lines = new List<string>();
            var reader = new RawLineReader(text);

            while (reader.ReadLine(out var line))
            {
                lines.Add(new string(line));
            }

            return lines.ToArray();
        }
    }
}
