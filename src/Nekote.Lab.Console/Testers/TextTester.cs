using System;
using System.Diagnostics;
using System.Text;
using Nekote.Core.Text;
using Nekote.Core.Text.Processing;

namespace Nekote.Lab.Console.Testers
{
    /// <summary>
    /// テキスト関連のコードをテスト・実行するためのクラス。
    /// </summary>
    public class TextTester
    {
        /// <summary>
        /// TextProcessor.Reformat メソッドの速度テストを実行します。
        /// デフォルト設定での様々なエッジケースを含む包括的なテストを行います。
        /// </summary>
        /// <param name="testDurationMilliseconds">テストを実行する時間（ミリ秒）。</param>
        public void SpeedTestReformat(int testDurationMilliseconds)
        {
            DisplayTestHeader();

            var sampleText = PrepareSampleText();
            DisplaySampleCharacteristics(sampleText);

            var reformattedSample = ExecuteAndDisplaySample(sampleText);

            RunPerformanceTest(sampleText, testDurationMilliseconds);
        }

        /// <summary>
        /// テストのヘッダーを表示します。
        /// </summary>
        private void DisplayTestHeader()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("=== TextProcessor.Reformat Speed Test ===");
        }

        /// <summary>
        /// テスト用の複雑なサンプルテキストを準備します。
        /// </summary>
        /// <returns>エッジケースを含むサンプルテキスト。</returns>
        private string PrepareSampleText()
        {
            // デフォルト設定でテストする複雑なサンプルテキストを準備します。
            // LineReaderConfiguration.Default の動作をテストするため、以下の要素を含みます：
            // - 先頭の空行（無視される）
            // - 行頭の空白文字（保持される）
            // - 行末の空白文字（トリムされる）
            // - タブ文字とスペースの混在
            // - 行内の連続する空白文字（保持される）
            // - 空白のみの行（空行として扱われる - Unicode空白文字のみの行を含む）
            // - Unicode空白文字
            // - 連続する空行（1行に集約される）
            // - 異なる改行文字（\r\n, \n, \r）
            // - 末尾の空行（無視される）

            // エディタによる自動的な末尾空白除去やエンコーディング問題を回避するため、StringBuilder を使用します。
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("\r\n"); // 先頭の空行
            stringBuilder.Append("\n");   // 先頭の空行
            stringBuilder.Append("    Leading empty lines should be ignored.\r");
            stringBuilder.Append("This line has trailing spaces and tabs.").Append("    \t\r\n"); // 末尾に空白とタブ
            stringBuilder.Append(" \t Mixed tabs and spaces at start\twith internal   multiple   spaces.\n");
            stringBuilder.Append("        Deep indentation preserved").Append("    \n"); // 末尾に空白
            stringBuilder.Append("  \r\n"); // 空白のみの行
            stringBuilder.Append("  \t  \n"); // 空白とタブのみの行
            stringBuilder.Append("\u2003\u2003\r"); // Unicode 空白 (EM SPACE) のみの行
            stringBuilder.Append("    \n"); // 空白のみの行
            stringBuilder.Append("Multiple empty lines above (whitespace-only lines count as empty).\r\n");
            stringBuilder.Append("Line with\ttabs\tinside\ttext.\n");
            stringBuilder.Append("Line with various Unicode spaces: ").Append("\u00A0\u2000\u2001\u2002\u2003\r");
            stringBuilder.Append("Mixed line endings test:\r\nCRLF line\nLF line\rCR line\n");
            stringBuilder.Append("Final content line with trailing whitespace.").Append("   \r\n"); // 末尾に空白
            stringBuilder.Append("\n");   // 末尾の空行
            stringBuilder.Append("\r\n"); // 末尾の空行
            stringBuilder.Append("");      // 末尾の空行
            return stringBuilder.ToString();
        }

        /// <summary>
        /// サンプルテキストの特徴をコンソールに表示します。
        /// </summary>
        /// <param name="sampleText">表示するサンプルテキスト。</param>
        private void DisplaySampleCharacteristics(string sampleText)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Sample Text Characteristics:");
            System.Console.WriteLine($"- Original length: {sampleText.Length} characters");
            System.Console.WriteLine($"- Original line count: {StringHelper.SplitLines(sampleText).Length}");
            System.Console.WriteLine("- Contains: Leading/trailing empty lines, mixed whitespace, Unicode whitespace, various line endings");
        }

        /// <summary>
        /// サンプルテキストを1回実行して結果をプレビュー表示します。
        /// </summary>
        /// <param name="sampleText">実行するサンプルテキスト。</param>
        /// <returns>フォーマット済みのサンプルテキスト。</returns>
        private string ExecuteAndDisplaySample(string sampleText)
        {
            var reformattedSample = TextProcessor.Reformat(sampleText);
            DisplayReformattedSample(reformattedSample);
            return reformattedSample;
        }

        /// <summary>
        /// フォーマット済みサンプルの結果を表示します。
        /// </summary>
        /// <param name="reformattedSample">フォーマット済みのサンプルテキスト。</param>
        private void DisplayReformattedSample(string reformattedSample)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Reformatted Result Preview (with [EOL] at line endings):");
            System.Console.WriteLine("--- Start ---");

            // 各行の末尾を明示的に表示するため、行ごとに分割して [EOL] マーカーを追加します。
            var lines = StringHelper.SplitLines(reformattedSample);
            foreach (var line in lines)
            {
                System.Console.WriteLine($"{line}[EOL]");
            }

            System.Console.WriteLine("--- End ---");
            System.Console.WriteLine($"Reformatted length: {reformattedSample.Length} characters");
            System.Console.WriteLine($"Reformatted line count: {lines.Length}");
        }

        /// <summary>
        /// 指定された時間でパフォーマンステストを実行し、結果を表示します。
        /// </summary>
        /// <param name="sampleText">テストに使用するサンプルテキスト。</param>
        /// <param name="testDurationMilliseconds">テストを実行する時間（ミリ秒）。</param>
        private void RunPerformanceTest(string sampleText, int testDurationMilliseconds)
        {
            DisplayPerformanceTestInfo(testDurationMilliseconds);
            var result = ExecutePerformanceTest(sampleText, testDurationMilliseconds);
            DisplayPerformanceResults(result, sampleText.Length, testDurationMilliseconds);
        }

        /// <summary>
        /// パフォーマンステストの情報を表示します。
        /// </summary>
        /// <param name="testDurationMilliseconds">テストを実行する時間（ミリ秒）。</param>
        private void DisplayPerformanceTestInfo(int testDurationMilliseconds)
        {
            System.Console.WriteLine();
            System.Console.WriteLine($"Running test for {testDurationMilliseconds} ms using a comprehensive edge case text...");
            System.Console.WriteLine("Starting speed test...");
        }

        /// <summary>
        /// パフォーマンステストを実行します。
        /// </summary>
        /// <param name="sampleText">テストに使用するサンプルテキスト。</param>
        /// <param name="testDurationMilliseconds">テストを実行する時間（ミリ秒）。</param>
        /// <returns>パフォーマンステストの結果。</returns>
        private PerformanceTestResult ExecutePerformanceTest(string sampleText, int testDurationMilliseconds)
        {
            var stopwatch = new Stopwatch();
            var testDuration = TimeSpan.FromMilliseconds(testDurationMilliseconds);
            var iterations = 0;

            stopwatch.Start();

            while (stopwatch.Elapsed < testDuration)
            {
                _ = TextProcessor.Reformat(sampleText);
                iterations++;
            }

            stopwatch.Stop();

            return new PerformanceTestResult
            {
                TotalMilliseconds = stopwatch.Elapsed.TotalMilliseconds,
                Iterations = iterations
            };
        }

        /// <summary>
        /// パフォーマンステストの結果を表示します。
        /// </summary>
        /// <param name="result">パフォーマンステストの結果。</param>
        /// <param name="sampleTextLength">サンプルテキストの文字数。</param>
        /// <param name="testDurationMilliseconds">テストを実行する時間（ミリ秒）。</param>
        private void DisplayPerformanceResults(PerformanceTestResult result, int sampleTextLength, int testDurationMilliseconds)
        {
            var averageMilliseconds = result.Iterations > 0 ? result.TotalMilliseconds / result.Iterations : 0;

            System.Console.WriteLine();
            System.Console.WriteLine("=== Performance Results ===");
            System.Console.WriteLine($"Total time: {result.TotalMilliseconds:F2} ms ({testDurationMilliseconds} ms)");
            System.Console.WriteLine($"Iterations: {result.Iterations:N0}");
            System.Console.WriteLine($"Average time per iteration: {averageMilliseconds:F4} ms");

            if (result.TotalMilliseconds > 0)
            {
                System.Console.WriteLine($"Operations per second: {result.Iterations / (result.TotalMilliseconds / 1000):F0}");
                System.Console.WriteLine($"Throughput: {(long)sampleTextLength * result.Iterations / (result.TotalMilliseconds / 1000) / 1024 / 1024:F2} MB/s");
            }
        }

        /// <summary>
        /// パフォーマンステストの結果を表すクラス。
        /// </summary>
        private class PerformanceTestResult
        {
            /// <summary>
            /// 合計実行時間（ミリ秒）を取得または設定します。
            /// </summary>
            public double TotalMilliseconds { get; set; }

            /// <summary>
            /// 反復回数を取得または設定します。
            /// </summary>
            public int Iterations { get; set; }
        }
    }
}
