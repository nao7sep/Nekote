using System;
using System.Collections.Generic;
using System.Text;

namespace Nekote.Core.Text.Processing
{
    /// <summary>
    /// 高度なテキスト処理機能を提供します。
    /// RawLineReader、LineProcessor、LineReaderを使用した複雑な行処理操作を含みます。
    /// </summary>
    public static class TextProcessor
    {
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

            var stringBuilder = new StringBuilder(text.Length);
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
