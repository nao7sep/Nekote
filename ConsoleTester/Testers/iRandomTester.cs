using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace ConsoleTester
{
    internal static class iRandomTester
    {
        // 乱数生成の速度を比べてみた

        // 数年前のノートである SV7 での結果
        // テストの内容については、コードがあるので省略
        // Chrome, VS, VSC などが走っていての軽いテストなので参考程度に

        // Random.Next: 234.9984ms
        // lock + Random.Next: 1874.7954ms
        // Random.Shared.Next: 756.3111ms
        // Random.NextBytes: 13.3175ms
        // RandomNumberGenerator.GetBytes: 32.0578ms
        // Guid.NewGuid: 7583.3231ms

        // Next などは1億回の所要時間
        // 何か間違えたかと思うほど高速だが、たとえば iArrayTester.CompareComparisonSpeeds では
        //     長さ100万の配列を100回比較（つまり双方からの1億回の読み出しおよび照合）で100ミリ秒台なので、妥当な所要時間

        // lock はやはり重たく、Next のたびに1億回という悪条件では大きな影響を及ぼした

        // Random.Shared が良好
        // これは ThreadSafeRandom 型のインスタンスであり、ザッと見た限り lock 系のものに頼らない

        // Random.Shared Property (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.random.shared

        // Random.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Random.cs

        // Random を引数なしで new した場合のアルゴリズムは、xoshiro256** algorithm というものになるそうだ
        // ソースに This implementation is used on 64-bit when no seed is specified and an instance of the base Random class is constructed とある

        // Random.Xoshiro256StarStarImpl.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Random.Xoshiro256StarStarImpl.cs

        // xoshiro/xoroshiro generators and the PRNG shootout
        // https://prng.di.unimi.it/

        // RandomNumberGenerator.GetInt32 は、.NET 7 から利用可能
        // 今のところ LTS の .NET 6 で開発しているため、今回は GetBytes で代用
        // それとの比較のために Random.NextBytes の速度も調べている

        // RandomNumberGenerator.GetInt32 Method (System.Security.Cryptography) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator.getint32

        // RandomNumberGenerator.GetBytes Method (System.Security.Cryptography) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator.getbytes

        // Random.NextBytes Method (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.random.nextbytes

        // xNumberCount を xBufferLength で割るため、回数の少なさにより lock ありのテストは不要
        // やってみたが、13ミリ秒台前半が13ミリ秒台後半になる程度の違いしかなかった

        // 最後に Guid.NewGuid についても調べた
        // Select で Guid.NewGuid のフィールド（？）を取り、それをキーとして Sort してのシャッフルを行うコードが散見されるため
        // 予想通りかなり重たいため、よほどの理由がない限り、これをシャッフルに使うのは避けるべき

        // Guid.NewGuid Method (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.guid.newguid

        public static void CompareGenerationSpeeds ()
        {
            const int xTestCount = 10,
                xNumberCount = 100000000, // 乱数の数
                xBufferLength = 10000, // 生成するバイト列の長さ
                xBufferFillingCount = xNumberCount / xBufferLength; // 割り切れるように

            Stopwatch xStopwatch = new Stopwatch ();
            string [] xLabels = { "Random.Next", "lock + Random.Next", "Random.Shared.Next", "Random.NextBytes", "RandomNumberGenerator.GetBytes", "Guid.NewGuid" };
            TimeSpan [,] xElapsed = new TimeSpan [xLabels.Length, xTestCount]; // たまには多次元配列も

            object xLocker = new object ();
            byte [] xBuffer = new byte [xBufferLength];

            Random xRandom = new Random ();
            using RandomNumberGenerator xGenerator = RandomNumberGenerator.Create ();

            for (int temp = 0; temp < xTestCount; temp ++)
            {
                int xIndex = 0;

                // =============================================================================

                // Reset する理由については iGenericTester.CompareSpeedsOfNullableAndBoxing のコメントに

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xNumberCount; tempAlt ++)
                    xRandom.Next ();

                xElapsed [xIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xNumberCount; tempAlt ++)
                {
                    lock (xLocker)
                    {
                        xRandom.Next ();
                    }
                }

                xElapsed [xIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xNumberCount; tempAlt ++)
                    Random.Shared.Next ();

                xElapsed [xIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xBufferFillingCount; tempAlt ++)
                    xRandom.NextBytes (xBuffer);

                xElapsed [xIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xBufferFillingCount; tempAlt ++)
                    xGenerator.GetBytes (xBuffer);

                xElapsed [xIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xNumberCount; tempAlt ++)
                    Guid.NewGuid ();

                xElapsed [xIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                nConsole.WriteProcessingMessage ("計測中");
            }

            Console.WriteLine ();
            Console.WriteLine (string.Join (Environment.NewLine, Enumerable.Range (0, xLabels.Length).Select (x => $"{xLabels [x]}: {nTimeSpan.Average (Enumerable.Range (0, xTestCount).Select (y => xElapsed [x, y])).TotalMilliseconds}ms")));
        }
    }
}
