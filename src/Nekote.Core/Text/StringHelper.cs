using System;
using System.Collections.Generic;
using Nekote.Core.Text.Processing;

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

        // C# では、オプション引数のデフォルト値は呼び出し元のアセンブリにコンパイル時に埋め込まれます。
        // これは、ライブラリの将来のバージョンでデフォルト値を変更した場合に、再コンパイルされない限り
        // 古いクライアントが古いデフォルト値を使い続けることを意味します。
        // このようなバージョン間の意図しない動作を防ぐため、ここではメソッドのオーバーロードを使用しています。
        // このアプローチにより、デフォルト値の解決がライブラリ内にカプセル化され、より堅牢な API となります。

        /// <summary>
        /// 指定された文字列を、デフォルトの構成を使用して行ごとに効率的に列挙します。
        /// </summary>
        /// <param name="text">列挙する文字列。</param>
        /// <returns>行を列挙する <see cref="IEnumerable{String}"/>。</returns>
        public static IEnumerable<string> EnumerateLines(string text)
        {
            return EnumerateLines(text.AsMemory(), LineReaderConfiguration.Default);
        }

        /// <summary>
        /// 指定された構成を使用して、文字列を行ごとに効率的に列挙します。
        /// </summary>
        /// <param name="text">列挙する文字列。</param>
        /// <param name="configuration">使用する行読み取り構成。</param>
        /// <returns>行を列挙する <see cref="IEnumerable{String}"/>。</returns>
        public static IEnumerable<string> EnumerateLines(string text, LineReaderConfiguration configuration)
        {
            return EnumerateLines(text.AsMemory(), configuration);
        }

        /// <summary>
        /// 指定されたメモリ領域を、デフォルトの構成を使用して行ごとに効率的に列挙します。
        /// </summary>
        /// <param name="text">列挙するテキストを含むメモリ領域。</param>
        /// <returns>行を列挙する <see cref="IEnumerable{String}"/>。</returns>
        public static IEnumerable<string> EnumerateLines(ReadOnlyMemory<char> text)
        {
            return EnumerateLines(text, LineReaderConfiguration.Default);
        }

        /// <summary>
        /// 指定された構成を使用して、メモリ領域を行ごとに効率的に列挙します。
        /// このメソッドは遅延実行を使用し、巨大なテキストを処理する際のメモリ効率を高めます。
        /// </summary>
        /// <param name="text">列挙するテキストを含むメモリ領域。</param>
        /// <param name="configuration">使用する行読み取り構成。</param>
        /// <returns>行を列挙する <see cref="IEnumerable{String}"/>。</returns>
        public static IEnumerable<string> EnumerateLines(ReadOnlyMemory<char> text, LineReaderConfiguration configuration)
        {
            if (text.IsEmpty)
            {
                yield break;
            }

            var reader = LineReader.Create(configuration, text);

            while (reader.ReadLine(out var line))
            {
                yield return new string(line);
            }
        }

        /// <summary>
        /// 指定された文字列を、デフォルトの構成を使用して行ごとに分割します。
        /// </summary>
        /// <param name="text">分割する文字列。</param>
        /// <returns>分割された行を含む文字列の配列。</returns>
        public static string[] SplitLines(string text)
        {
            return SplitLines(text.AsMemory(), LineReaderConfiguration.Default);
        }

        /// <summary>
        /// 指定された構成を使用して、文字列を行ごとに分割します。
        /// </summary>
        /// <param name="text">分割する文字列。</param>
        /// <param name="configuration">使用する行読み取り構成。</param>
        /// <returns>分割された行を含む文字列の配列。</returns>
        public static string[] SplitLines(string text, LineReaderConfiguration configuration)
        {
            return SplitLines(text.AsMemory(), configuration);
        }

        /// <summary>
        /// 指定されたメモリ領域を、デフォルトの構成を使用して行ごとに分割します。
        /// </summary>
        /// <param name="text">分割するテキストを含むメモリ領域。</param>
        /// <returns>分割された行を含む文字列の配列。</returns>
        public static string[] SplitLines(ReadOnlyMemory<char> text)
        {
            return SplitLines(text, LineReaderConfiguration.Default);
        }

        /// <summary>
        /// 指定された構成を使用して、メモリ領域を行ごとに分割します。
        /// </summary>
        /// <param name="text">分割するテキストを含むメモリ領域。</param>
        /// <param name="configuration">使用する行読み取り構成。</param>
        /// <returns>分割された行を含む文字列の配列。</returns>
        public static string[] SplitLines(ReadOnlyMemory<char> text, LineReaderConfiguration configuration)
        {
            var lines = new List<string>();
            foreach (var line in EnumerateLines(text, configuration))
            {
                lines.Add(line);
            }
            return lines.ToArray();
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

        /// <summary>
        /// デフォルト構成を使用して、指定されたテキストを再フォーマットします。
        /// </summary>
        /// <param name="text">再フォーマットするテキスト。</param>
        /// <returns>再フォーマットされた文字列。</returns>
        public static string Reformat(string text)
        {
            return Reformat(text.AsMemory(), LineReaderConfiguration.Default, NewlineSequence.PlatformDefault);
        }

        /// <summary>
        /// 指定されたリーダー構成を使用して、テキストを再フォーマットします。
        /// </summary>
        /// <param name="text">再フォーマットするテキスト。</param>
        /// <param name="readerConfiguration">使用する行読み取り構成。</param>
        /// <returns>再フォーマットされた文字列。</returns>
        public static string Reformat(string text, LineReaderConfiguration readerConfiguration)
        {
            return Reformat(text.AsMemory(), readerConfiguration, NewlineSequence.PlatformDefault);
        }

        /// <summary>
        /// 指定された改行シーケンスを使用して、テキストを再フォーマットします。
        /// </summary>
        /// <param name="text">再フォーマットするテキスト。</param>
        /// <param name="newlineSequence">使用する改行シーケンス。</param>
        /// <returns>再フォーマットされた文字列。</returns>
        public static string Reformat(string text, NewlineSequence newlineSequence)
        {
            return Reformat(text.AsMemory(), LineReaderConfiguration.Default, newlineSequence);
        }

        /// <summary>
        /// 指定された構成を使用して、テキストを再フォーマットします。
        /// </summary>
        /// <param name="text">再フォーマットするテキスト。</param>
        /// <param name="readerConfiguration">使用する行読み取り構成。</param>
        /// <param name="newlineSequence">使用する改行シーケンス。</param>
        /// <returns>再フォーマットされた文字列。</returns>
        public static string Reformat(string text, LineReaderConfiguration readerConfiguration, NewlineSequence newlineSequence)
        {
            return Reformat(text.AsMemory(), readerConfiguration, newlineSequence);
        }

        /// <summary>
        /// デフォルト構成を使用して、指定されたテキストを再フォーマットします。
        /// </summary>
        /// <param name="text">再フォーマットするテキストを含むメモリ領域。</param>
        /// <returns>再フォーマットされた文字列。</returns>
        public static string Reformat(ReadOnlyMemory<char> text)
        {
            return Reformat(text, LineReaderConfiguration.Default, NewlineSequence.PlatformDefault);
        }

        /// <summary>
        /// 指定されたリーダー構成を使用して、テキストを再フォーマットします。
        /// </summary>
        /// <param name="text">再フォーマットするテキストを含むメモリ領域。</param>
        /// <param name="readerConfiguration">使用する行読み取り構成。</param>
        /// <returns>再フォーマットされた文字列。</returns>
        public static string Reformat(ReadOnlyMemory<char> text, LineReaderConfiguration readerConfiguration)
        {
            return Reformat(text, readerConfiguration, NewlineSequence.PlatformDefault);
        }

        /// <summary>
        /// 指定された改行シーケンスを使用して、テキストを再フォーマットします。
        /// </summary>
        /// <param name="text">再フォーマットするテキストを含むメモリ領域。</param>
        /// <param name="newlineSequence">使用する改行シーケンス。</param>
        /// <returns>再フォーマットされた文字列。</returns>
        public static string Reformat(ReadOnlyMemory<char> text, NewlineSequence newlineSequence)
        {
            return Reformat(text, LineReaderConfiguration.Default, newlineSequence);
        }

        /// <summary>
        /// 指定された構成を使用して、テキストを再フォーマットします。
        /// このメソッドは、中間的な文字列割り当てを回避するために <see cref="System.Text.StringBuilder"/> を使用して最適化されています。
        /// </summary>
        /// <param name="text">再フォーマットするテキストを含むメモリ領域。</param>
        /// <param name="readerConfiguration">使用する行読み取り構成。</param>
        /// <param name="newlineSequence">使用する改行シーケンス。</param>
        /// <returns>再フォーマットされた文字列。</returns>
        public static string Reformat(ReadOnlyMemory<char> text, LineReaderConfiguration readerConfiguration, NewlineSequence newlineSequence)
        {
            if (text.IsEmpty)
            {
                return string.Empty;
            }

            var newline = newlineSequence switch
            {
                NewlineSequence.Lf => "\n",
                NewlineSequence.CrLf => "\r\n",
                NewlineSequence.PlatformDefault => Environment.NewLine,
                _ => throw new ArgumentOutOfRangeException(nameof(newlineSequence), newlineSequence, "Invalid NewlineSequence value."),
            };

            var stringBuilder = new System.Text.StringBuilder(text.Length);
            var reader = LineReader.Create(readerConfiguration, text);
            var isFirstLine = true;

            while (reader.ReadLine(out var lineSpan))
            {
                if (!isFirstLine)
                {
                    stringBuilder.Append(newline);
                }
                stringBuilder.Append(lineSpan);
                isFirstLine = false;
            }

            return stringBuilder.ToString();
        }
    }
}
