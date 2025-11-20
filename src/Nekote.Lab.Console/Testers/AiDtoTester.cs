using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nekote.Lab.Console.Testers
{
    /// <summary>
    /// AI 関連の DTO とコンバーターの使用状況を検証するためのクラス。
    /// </summary>
    public class AiDtoTester
    {
        /// <summary>
        /// 指定された AI プロバイダのディレクトリパス。
        /// </summary>
        private readonly string _aiDirectoryPath;

        /// <summary>
        /// <see cref="AiDtoTester"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="aiDirectoryPath">AI プロバイダのディレクトリパス。</param>
        public AiDtoTester(string aiDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(aiDirectoryPath))
            {
                throw new ArgumentException("AI directory path cannot be null or whitespace.", nameof(aiDirectoryPath));
            }

            if (!Path.IsPathFullyQualified(aiDirectoryPath))
            {
                throw new ArgumentException("AI directory path must be a fully qualified path.", nameof(aiDirectoryPath));
            }

            _aiDirectoryPath = aiDirectoryPath;
        }

        /// <summary>
        /// 全ての DTO とコンバーターの使用状況を分析し、コンソールに結果を表示します。
        /// </summary>
        public void AnalyzeDtoAndConverterUsage()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("=== AI DTO and Converter Usage Analysis ===");
            System.Console.WriteLine();

            // Gemini と OpenAI のディレクトリを検証します。
            var geminiInfrastructureDirectoryPath = Path.Combine(_aiDirectoryPath, "Infrastructure", "Gemini");
            var openAiInfrastructureDirectoryPath = Path.Combine(_aiDirectoryPath, "Infrastructure", "OpenAI");

            if (!Directory.Exists(geminiInfrastructureDirectoryPath))
            {
                System.Console.WriteLine($"ERROR: Gemini directory not found: {geminiInfrastructureDirectoryPath}");
                return;
            }

            if (!Directory.Exists(openAiInfrastructureDirectoryPath))
            {
                System.Console.WriteLine($"ERROR: OpenAI directory not found: {openAiInfrastructureDirectoryPath}");
                return;
            }

            // DTO とコンバーターのファイル名を収集します。
            var geminiDtoDirectoryPath = Path.Combine(geminiInfrastructureDirectoryPath, "Dtos");
            var geminiConverterDirectoryPath = Path.Combine(geminiInfrastructureDirectoryPath, "Converters");
            var openAiDtoDirectoryPath = Path.Combine(openAiInfrastructureDirectoryPath, "Dtos");
            var openAiConverterDirectoryPath = Path.Combine(openAiInfrastructureDirectoryPath, "Converters");

            var typeNamesWithoutExtensions = new List<string>();

            if (Directory.Exists(geminiDtoDirectoryPath))
            {
                typeNamesWithoutExtensions.AddRange(CollectFileNamesWithoutExtensions(geminiDtoDirectoryPath));
            }

            if (Directory.Exists(geminiConverterDirectoryPath))
            {
                typeNamesWithoutExtensions.AddRange(CollectFileNamesWithoutExtensions(geminiConverterDirectoryPath));
            }

            if (Directory.Exists(openAiDtoDirectoryPath))
            {
                typeNamesWithoutExtensions.AddRange(CollectFileNamesWithoutExtensions(openAiDtoDirectoryPath));
            }

            if (Directory.Exists(openAiConverterDirectoryPath))
            {
                typeNamesWithoutExtensions.AddRange(CollectFileNamesWithoutExtensions(openAiConverterDirectoryPath));
            }

            System.Console.WriteLine($"Found {typeNamesWithoutExtensions.Count} DTO and converter files.");
            System.Console.WriteLine();

            // 全ての C# ファイルの内容を読み込みます。
            var allCSharpFilePaths = Directory.GetFiles(_aiDirectoryPath, "*.cs", SearchOption.AllDirectories);
            var allFileContents = allCSharpFilePaths
                .Select(filePath => ReadFileWithoutComments(filePath))
                .ToList();

            System.Console.WriteLine($"Searching through {allCSharpFilePaths.Length} C# files in {_aiDirectoryPath}");
            System.Console.WriteLine();

            // 各型名の出現回数をカウントします。
            var usageResults = new List<DtoUsageResult>();

            foreach (var typeNameWithoutExtension in typeNamesWithoutExtensions)
            {
                var occurrenceCount = CountOccurrences(allFileContents, typeNameWithoutExtension);
                usageResults.Add(new DtoUsageResult
                {
                    TypeName = typeNameWithoutExtension,
                    OccurrenceCount = occurrenceCount
                });
            }

            // 結果を表示します。
            DisplayResults(usageResults);
        }

        /// <summary>
        /// 指定されたディレクトリ内の全てのファイル名（拡張子なし）を収集します。
        /// </summary>
        /// <param name="directoryPath">ディレクトリのパス。</param>
        /// <returns>ファイル名のリスト（拡張子なし）。</returns>
        private List<string> CollectFileNamesWithoutExtensions(string directoryPath)
        {
            return Directory.GetFiles(directoryPath, "*.cs")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .ToList();
        }

        /// <summary>
        /// ファイルの内容を読み込み、コメントを除去します。
        /// 注: このメソッドは // コメントのみを処理します。/* */ 形式のコメントはコードベースに存在しないため対応していません。
        /// </summary>
        /// <param name="filePath">ファイルのパス。</param>
        /// <returns>コメントを除去した内容。</returns>
        private string ReadFileWithoutComments(string filePath)
        {
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            var processedLines = new List<string>();

            foreach (var line in lines)
            {
                // // が見つかった場合、それ以降を除去します。
                var commentIndex = line.IndexOf("//", StringComparison.Ordinal);

                if (commentIndex != -1)
                {
                    // コメントより前の部分のみを保持します。
                    processedLines.Add(line.Substring(0, commentIndex));
                }
                else
                {
                    // コメントがない場合は行全体を保持します。
                    processedLines.Add(line);
                }
            }

            return string.Join(Environment.NewLine, processedLines);
        }

        /// <summary>
        /// 全てのファイル内容において、指定された型名の出現回数をカウントします。
        /// </summary>
        /// <param name="fileContents">ファイル内容のリスト。</param>
        /// <param name="typeName">検索する型名。</param>
        /// <returns>出現回数。</returns>
        private int CountOccurrences(List<string> fileContents, string typeName)
        {
            var totalCount = 0;

            foreach (var fileContent in fileContents)
            {
                // 単純な文字列マッチで出現回数をカウントします。
                // これは、型名がコメント内や文字列リテラル内に現れる場合もカウントしますが、
                // 未使用の型を特定するという目的には十分です。
                var startIndex = 0;

                while (true)
                {
                    var foundIndex = fileContent.IndexOf(typeName, startIndex, StringComparison.Ordinal);

                    if (foundIndex == -1)
                    {
                        break;
                    }

                    totalCount++;
                    startIndex = foundIndex + typeName.Length;
                }
            }

            return totalCount;
        }

        /// <summary>
        /// 分析結果をコンソールに表示します。
        /// </summary>
        /// <param name="usageResults">使用状況の結果リスト。</param>
        private void DisplayResults(List<DtoUsageResult> usageResults)
        {
            // 出現回数でソートし、同じ出現回数の場合は型名でソートします（昇順）。
            var sortedResults = usageResults
                .OrderBy(result => result.OccurrenceCount)
                .ThenBy(result => result.TypeName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            System.Console.WriteLine("=== Results (sorted by occurrence count) ===");
            System.Console.WriteLine();

            var unusedCount = 0;
            var inconsistentCount = 0;
            var usedCount = 0;

            foreach (var result in sortedResults)
            {
                var status = string.Empty;
                var useColor = false;

                if (result.OccurrenceCount == 0)
                {
                    // ファイル名と実際の定義が一致していない可能性があります。
                    status = "[INCONSISTENT: File name does not match any type name]";
                    useColor = true;
                    inconsistentCount++;
                }
                else if (result.OccurrenceCount == 1)
                {
                    // 定義のみで、使用されていません。
                    status = "[UNUSED: Only the definition exists]";
                    useColor = true;
                    unusedCount++;
                }
                else
                {
                    // 使用されています。
                    status = "[USED]";
                    usedCount++;
                }

                // 色付きで表示します。
                if (useColor)
                {
                    var originalColor = System.Console.ForegroundColor;
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine($"{result.TypeName}: {result.OccurrenceCount} occurrences {status}");
                    System.Console.ForegroundColor = originalColor;
                }
                else
                {
                    System.Console.WriteLine($"{result.TypeName}: {result.OccurrenceCount} occurrences {status}");
                }
            }

            // サマリーを表示します。
            System.Console.WriteLine();
            System.Console.WriteLine("=== Summary ===");
            System.Console.WriteLine($"Total types analyzed: {usageResults.Count}");
            System.Console.WriteLine($"Used types: {usedCount}");
            System.Console.WriteLine($"Unused types (definition only): {unusedCount}");
            System.Console.WriteLine($"Inconsistent types (file name mismatch): {inconsistentCount}");
        }

        /// <summary>
        /// DTO とコンバーターの使用状況を表すクラス。
        /// </summary>
        private class DtoUsageResult
        {
            /// <summary>
            /// 型名を取得または設定します。
            /// </summary>
            public string TypeName { get; set; } = string.Empty;

            /// <summary>
            /// 出現回数を取得または設定します。
            /// </summary>
            public int OccurrenceCount { get; set; }
        }
    }
}
