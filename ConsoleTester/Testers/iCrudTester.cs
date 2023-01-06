using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace ConsoleTester
{
    internal static class iCrudTester
    {
        public static void TestIniLikeFileBasedDataProviders ()
        {
            // Using type dynamic - C# Programming Guide | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/types/using-type-dynamic

            string xDataDirectoryPath = nApp.MapPath ("Data");

            if (Directory.Exists (xDataDirectoryPath))
                nDirectory.Delete (xDataDirectoryPath, isRecursive: true);

            dynamic [] xDataProviders =
            {
                new nGuidAndIniLikeFileBasedCrudDataProvider (nPath.Map (xDataDirectoryPath, "GuidBased")),
                new nUtcTicksAndIniLikeFileBasedCrudDataProvider (nPath.Map (xDataDirectoryPath, "UtcTicksBased")),
                new nYouTubeLikeKeyAndIniLikeFileBasedCrudDataProvider (nPath.Map (xDataDirectoryPath, "YouTubeLikeKeyBased"))
            };

            int xGeneratedEntryCount = 0;

            nStringDictionary iGenerateEntry ()
            {
                nStringDictionary xEntry = new nStringDictionary ();
                xEntry.SetString ("Name", FormattableString.Invariant ($"name-{++ xGeneratedEntryCount}"));
                return xEntry;
            }

            string iKeyToString (dynamic key)
            {
                Type xType = key.GetType ();

                if (xType == typeof (Guid))
                    return key.ToString ("D");

                else if (xType == typeof (long))
                    return $"{key.ToString (CultureInfo.InvariantCulture)}Z";

                else if (xType == typeof (string))
                    return (string) key; // キャストなしでも動くが、作法として

                else throw new nArgumentException ();
            }

            // =============================================================================

            // CRUD を一通り簡単に

            // d2e37b4c-8e84-44ad-8c91-bb65a6a23bef: name-1 → name-2
            // 0ad6d288-2f02-4f8d-ab30-55c738d50b71 が削除されます。
            // 638085920177094249Z: name-4 → name-5
            // 638085920177194686Z が削除されます。
            // 36Okgv7R35i: name-7 → name-8
            // 7T45TheqUZT が削除されます。

            for (int temp = 0; temp < 3; temp ++)
            {
                dynamic xDataProvider = xDataProviders [temp];

                dynamic xKey = xDataProvider.CreateEntry (iGenerateEntry ());
                Console.Write ($"{iKeyToString (xKey)}: {xDataProvider.ReadEntry (xKey).GetString ("Name")}");

                nStringDictionary xEntry = iGenerateEntry ();
                xDataProvider.SetKeyToEntry (xEntry, xKey);
                xDataProvider.UpdateEntry (xKey, xEntry);
                Console.WriteLine ($" → {xDataProvider.ReadEntry (xKey).GetString ("Name")}");

                xKey = xDataProvider.CreateEntry (iGenerateEntry ());
                Console.WriteLine ($"{iKeyToString (xKey)} が削除されます。");

                xDataProvider.DeleteEntry (xKey);
            }

            // =============================================================================

            // 全ての *.nini の名前と内容を表示

            nStringOptimizationOptions xOptions = new nStringOptimizationOptions { MinIndentationLength = 4 };

            var xFiles = Directory.GetFiles (xDataDirectoryPath, "*.*", SearchOption.AllDirectories)
                .OrderBy (x => x, nAlphanumericComparer.InvariantCultureIgnoreCase)
                .Select (y => (Name: Path.GetFileName (y), Contents: nStringOptimizer.Default.Optimize (nFile.ReadAllText (y), xOptions).Value));

            // d2e37b4c-8e84-44ad-8c91-bb65a6a23bef.nini:
            //     Name:name-2
            //     Key:d2e37b4c-8e84-44ad-8c91-bb65a6a23bef
            // 638085920177094249Z.nini:
            //     Name:name-5
            //     Key:638085920177094249Z
            // 36Okgv7R35i.nini:
            //     Name:name-8
            //     Key:36Okgv7R35i

            Console.WriteLine (string.Join (Environment.NewLine, xFiles.Select (x => $"{x.Name}:{Environment.NewLine}{x.Contents}")));

            // =============================================================================

            // 雑な速度テスト
            // それぞれのクラスで1万のエントリーを出力することと、それらを読み込むことの所要時間を計測
            // SSD の性能、中身の散らかり具合、OS の他のプロセスの忙しさといったものに大きく影響を受けるため参考程度に
            // dynamic によるオーバーヘッドもあるだろう

            for (int temp = 0; temp < 3; temp ++)
            {
                dynamic xDataProvider = xDataProviders [temp];

                Stopwatch xStopwatch = new Stopwatch ();
                xStopwatch.Start ();

                for (int tempAlt = 0; tempAlt < 10_000; tempAlt ++)
                    xDataProvider.CreateEntry (iGenerateEntry ());

                // Write + Write + WriteLine の出力
                // 10001 → 0 → 10001

                TimeSpan xElapsed = xStopwatch.Elapsed;
                Console.Write (xDataProvider.Count);

                xDataProvider.Clear ();
                Console.Write ($" → {xDataProvider.Count}");

                xStopwatch = new Stopwatch ();
                xStopwatch.Start ();

                // out _ だと型を推論できないと叱られる

                xDataProvider.TryLoadAllEntries (out List <string> _, out List <string> _);
                TimeSpan xElapsedAlt = xStopwatch.Elapsed;
                Console.WriteLine ($" → {xDataProvider.Count}");

                // 数年前のノートである SV7 での結果
                // バラつきがあったので3度行った

                // [0] 19360.3309ms, 2253.3873ms
                // [1] 21298.0244ms, 1401.9527ms
                // [2] 21453.0482ms, 2100.7348ms

                // [0] 21870.1675ms, 1418.5019ms
                // [1] 24915.7104ms, 1100.9416ms
                // [2] 22044.0123ms, 1154.1942ms

                // [0] 20860.0809ms, 1403.94ms
                // [1] 25027.1137ms, 1160.3835ms
                // [2] 21968.0047ms, 1897.2857ms

                // 簡単なエントリーの場合、書き込みに1エントリーあたり2ミリ秒、
                //     読み込みは1ミリ秒で5エントリーを少し超えるくらい
                // データベースを使うほどでない簡単なプログラムには十分に使える速度

                Console.WriteLine ($"[{temp}] {xElapsed.TotalMilliseconds}ms, {xElapsedAlt.TotalMilliseconds}ms");
            }
        }
    }
}
