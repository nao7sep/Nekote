using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace ConsoleTester
{
    internal static class iArrayTester
    {
        // *Compare* としたが、Equals 系の処理の所要時間を比較

        // 結論としては、MemoryExtensions.SequenceEqual が最速

        // MemoryExtensions.SequenceEqual Method (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.memoryextensions.sequenceequal

        // Enumerable.SequenceEqual も同じくらい速いが、内部的には MemoryExtensions.SequenceEqual を呼ぶだけ
        // MemoryExtensions のものと異なり、範囲を指定できないため、こちらを使う理由はなさそう
        // Enumerable.Take により無理やり範囲を指定したところ、要素がコピーされるのか、劇的に遅くなった

        // Enumerable.SequenceEqual Method (System.Linq) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.sequenceequal

        // SequenceEqual.cs
        // https://source.dot.net/#System.Linq/System/Linq/SequenceEqual.cs

        // MemoryExtensions.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/MemoryExtensions.cs

        // Enumerable.Take Method (System.Linq) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.take

        // Enumerable.SequenceEqual については、とても遅くて使い物にならない印象があった
        // 古いコードでは、using (IEnumerator <TSource> e1 = first.GetEnumerator ()) などにおいて、
        //     while (e1.MoveNext ()) の中で if (!(e2.MoveNext () && comparer.Equals (e1.Current, e2.Current))) を見る実装になっていた

        // Enumerable.cs
        // https://referencesource.microsoft.com/#System.Core/System/Linq/Enumerable.cs

        // 次のページによると、IStructuralEquatable.Equals というものもある
        // 遅いそうだし、他でもあまり使われていないようだ
        // 有用なら使われるとの考え方により、今回のテストでは扱わない

        // Compare Arrays in C# - Code Maze
        // https://code-maze.com/csharp-compare-arrays/

        // IStructuralEquatable.Equals(Object, IEqualityComparer) Method (System.Collections) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.collections.istructuralequatable.equals

        // 2022年12月7日に数年前のノートである SV7 で実行しての結果 → 古いが、参考のために残す
        // nArray.Equals が MemoryExtensions.SequenceEqual より速いのは、他のプロセスの影響を受けてか

        // + で添え字を計算: 129.7964ms
        // ++ で添え字を進める: 146.9763ms
        // EqualityComparer.Default: 177.9745ms
        // Enumerable.SequenceEqual: 28.9544ms
        // Span.SequenceEqual: 27.3518ms
        // MemoryExtensions.SequenceEqual: 28.3833ms
        // nArray.Equals: 27.4096ms

        // 2022年12月9日に SV7 で再実行しての結果
        // iGenericTester.CompareSpeedsOfNullableAndBoxing の追加により定まったおまじないを適用
        // 二日前の実行時には、+ で添え字を計算する方が速かったり、オーバーヘッドを考えても EqualityComparer.Default が遅すぎたりに違和感があった
        // 理由が不明だが、こちらでも Stopwatch の Restart の前に Reset を呼ぶことで、より自然な計測結果が得られるようになった

        // + で添え字を計算: 178.5727ms
        // ++ で添え字を進める: 119.0396ms
        // EqualityComparer.Default: 142.5558ms
        // Enumerable.SequenceEqual: 31.5699ms
        // Span.SequenceEqual: 28.6523ms
        // MemoryExtensions.SequenceEqual: 28.35ms
        // nArray.Equals: 29.0902ms

        public static void CompareComparisonSpeeds ()
        {
            // オーバーヘッドの影響を抑えるため、長めの配列を少ない回数だけ比較

            const int xArrayLength = 1000000,
                xTestCount = 10,
                xComparisonCount = 100;

            Random xRandom = new Random ();

            int [] xValues1 = Enumerable.Range (0, xArrayLength).Select (x => xRandom.Next ()).ToArray (),
                xValues2 = (int []) xValues1.Clone ();

            int xFirstIndex = 0; // できれば最適化を避けたいため const にしない
            var xComparer = EqualityComparer <int>.Default;

            Stopwatch xStopwatch = new Stopwatch ();

            string [] xLabels = { "+ で添え字を計算", "++ で添え字を進める", "EqualityComparer.Default", "Enumerable.SequenceEqual", "Span.SequenceEqual", "MemoryExtensions.SequenceEqual", "nArray.Equals" };
            var xResults = Enumerable.Range (0, xLabels.Length).Select (x => (Index: x, Elapsed: new List <TimeSpan> ())).ToArray ();

            for (int temp = 0; temp < xTestCount; temp ++)
            {
                int xLabelIndex = 0;

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xComparisonCount; tempAlt ++)
                {
                    for (int tempAlt1 = 0; tempAlt1 < xArrayLength; tempAlt1 ++)
                    {
                        if (xValues1 [xFirstIndex + tempAlt1] != xValues2 [xFirstIndex + tempAlt1])
                            break;
                    }
                }

                xResults [xLabelIndex ++].Elapsed.Add (xStopwatch.Elapsed);

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xComparisonCount; tempAlt ++)
                {
                    int xIndex1 = xFirstIndex,
                        xIndex2 = xFirstIndex,
                        xLastIndex1 = xFirstIndex + xArrayLength - 1;

                    while (xIndex1 <= xLastIndex1)
                    {
                        if (xValues1 [xIndex1 ++] != xValues2 [xIndex2 ++])
                            break;
                    }
                }

                xResults [xLabelIndex ++].Elapsed.Add (xStopwatch.Elapsed);

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xComparisonCount; tempAlt ++)
                {
                    int xIndex1 = xFirstIndex,
                        xIndex2 = xFirstIndex,
                        xLastIndex1 = xFirstIndex + xArrayLength - 1;

                    while (xIndex1 <= xLastIndex1)
                    {
                        if (xComparer.Equals (xValues1 [xIndex1 ++], xValues2 [xIndex2 ++]) == false)
                            break;
                    }
                }

                xResults [xLabelIndex ++].Elapsed.Add (xStopwatch.Elapsed);

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xComparisonCount; tempAlt ++)
                    Enumerable.SequenceEqual (xValues1, xValues2);

                xResults [xLabelIndex ++].Elapsed.Add (xStopwatch.Elapsed);

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xComparisonCount; tempAlt ++)
                    xValues1.AsSpan (xFirstIndex, xArrayLength).SequenceEqual (xValues2.AsSpan (xFirstIndex, xArrayLength));

                xResults [xLabelIndex ++].Elapsed.Add (xStopwatch.Elapsed);

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xComparisonCount; tempAlt ++)
                    MemoryExtensions.SequenceEqual (xValues1.AsSpan (xFirstIndex, xArrayLength), xValues2.AsSpan (xFirstIndex, xArrayLength));

                xResults [xLabelIndex ++].Elapsed.Add (xStopwatch.Elapsed);

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xComparisonCount; tempAlt ++)
                    nArray.Equals (xValues1, xFirstIndex, xValues2, xFirstIndex, xArrayLength);

                xResults [xLabelIndex ++].Elapsed.Add (xStopwatch.Elapsed);

                // =============================================================================

                nConsole.WriteProcessingMessage ("計測中");
            }

            Console.WriteLine ();
            Console.WriteLine (string.Join (Environment.NewLine, xResults.Select (x => $"{xLabels [x.Index]}: {nTimeSpan.Average (x.Elapsed).TotalMilliseconds}ms")));
        }
    }
}
