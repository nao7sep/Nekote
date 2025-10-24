using System;
using System.Diagnostics;
using Nekote.Core.Text;

namespace Nekote.Lab.Console.Testers
{
    /// <summary>
    /// テキスト関連のコードをテスト・実行するためのクラス。
    /// </summary>
    public class TextTester
    {
        /// <summary>
        /// StringHelper.Reformat メソッドの速度テストを実行します。
        /// デフォルト設定での様々なエッジケースを含む包括的なテストを行います。
        /// </summary>
        /// <param name="testDurationSeconds">テストを実行する時間（秒）。</param>
        public void SpeedTestReformat(int testDurationSeconds)
        {
            // デフォルト設定でテストする複雑なサンプルテキストを準備します。
            // LineReaderConfiguration.Default の動作をテストするため、以下の要素を含みます：
            // - 先頭の空行（無視される）
            // - 行末の空白文字（トリムされる）
            // - 行頭・行内の空白文字（保持される）
            // - 連続する空行（1行に集約される）
            // - 末尾の空行（無視される）
            // - 異なる改行文字（\r\n, \n, \r）
            // - タブ文字とスペースの混在
            // - Unicode空白文字
            // - 空白のみの行（空行として扱われる）

            // エディタによる自動的な末尾空白除去を回避するため、文字列を動的に構築します。
            var sampleText = string.Join("\n", new[]
            {
                "", // 先頭の空行
                "", // 先頭の空行
                "    Leading empty lines should be ignored.",
                "This line has trailing spaces and tabs." + "    \t", // 末尾に空白とタブ
                "\tMixed tabs and spaces at start\twith internal   multiple   spaces.",
                "        Deep indentation preserved" + "    ", // 末尾に空白
                "  ", // 空白のみの行
                "  \t  ", // 空白とタブのみの行
                "    ", // 空白のみの行
                "Multiple empty lines above (whitespace-only lines count as empty).",
                "Line with\ttabs\tinside\ttext.",
                "Line with various Unicode spaces: " + "\u00A0\u2000\u2001\u2002\u2003",
                "Mixed line endings test:\r\nCRLF line\nLF line\rCR line",
                "Final content line with trailing whitespace." + "   ", // 末尾に空白
                "", // 末尾の空行
                "", // 末尾の空行
                ""  // 末尾の空行
            });

            var stopwatch = new Stopwatch();

            // StringHelper.Reformat の速度テストを開始します。
            System.Console.WriteLine("=== StringHelper.Reformat 速度テスト ===");
            System.Console.WriteLine($"包括的なエッジケーステキストを使用して {testDurationSeconds} 秒間テストを実行します...");
            System.Console.WriteLine();

            // サンプルテキストの特徴を表示します。
            System.Console.WriteLine("サンプルテキストの特徴:");
            System.Console.WriteLine($"- 元の長さ: {sampleText.Length} 文字");
            System.Console.WriteLine($"- 元の行数: {sampleText.Split('\n').Length}");
            System.Console.WriteLine("- 含まれる要素: 先頭/末尾の空行、混在する空白文字、Unicode空白、異なる改行文字");
            System.Console.WriteLine();

            // 1回だけ実行して結果を確認します。
            var reformattedSample = StringHelper.Reformat(sampleText);
            System.Console.WriteLine("再フォーマット結果のプレビュー（行末を [EOL] で表示）:");
            System.Console.WriteLine("--- 開始 ---");

            // 各行の末尾を明示的に表示するため、行ごとに分割して [EOL] マーカーを追加します。
            var lines = reformattedSample.Split(Environment.NewLine);
            for (int i = 0; i < lines.Length; i++)
            {
                System.Console.WriteLine($"{lines[i]}[EOL]");
            }

            System.Console.WriteLine("--- 終了 ---");
            System.Console.WriteLine($"再フォーマット後の長さ: {reformattedSample.Length} 文字");
            System.Console.WriteLine($"再フォーマット後の行数: {lines.Length}");
            System.Console.WriteLine();

            // 指定された時間だけテストを実行します。
            System.Console.WriteLine("速度テストを開始しています...");
            var testDuration = TimeSpan.FromSeconds(testDurationSeconds);
            var iterations = 0;

            stopwatch.Start();

            while (stopwatch.Elapsed < testDuration)
            {
                // StringHelper.Reformat メソッドを呼び出します。
                var reformattedText = StringHelper.Reformat(sampleText);
                iterations++;
            }

            stopwatch.Stop();

            // 結果をコンソールに出力します。
            var totalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            var averageMilliseconds = totalMilliseconds / iterations;

            System.Console.WriteLine("=== パフォーマンス結果 ===");
            System.Console.WriteLine($"実行時間: {totalMilliseconds:F2} ms ({testDurationSeconds} 秒間)");
            System.Console.WriteLine($"実行回数: {iterations:N0} 回");
            System.Console.WriteLine($"1回あたりの平均時間: {averageMilliseconds:F4} ms");
            System.Console.WriteLine($"1秒あたりの操作数: {iterations / (totalMilliseconds / 1000):F0}");
            System.Console.WriteLine($"スループット: {(sampleText.Length * iterations) / (totalMilliseconds / 1000) / 1024 / 1024:F2} MB/s");
        }
    }
}
