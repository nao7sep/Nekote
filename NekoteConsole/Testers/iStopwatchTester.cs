using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace NekoteConsole
{
    // nConcurrentStopwatch は、処理としてはシンプルだが、内部に Task を持ち、「放っておくと自動的に止まる」という特徴があるため、ステート管理が少しややこしい
    // また、nStopwatchEntry があっても nConcurrentStopwatch.CurrentEntryElapsedTime を nConcurrentStopwatch.CurrentEntry.ElapsedTime とできない理由（＝処理が異なる）などがあり、
    //     nConcurrentStopwatch 側にフィールドが多いことによる、何となくの分かりにくさがずっと残る
    // シンプルにするため、たとえば AutoPauser クラスを用意し、自動中断関連のコードをそちらに詰め込むなども考えたが、
    //     nConcurrentStopwatch からのアクセスの階層（？）が深まることが lock との兼ね合いに影響したり、オーバーヘッドによる（微々たる）誤差につながったりするのを懸念した
    // Task を持ち、lock を多用するクラスなので、どうしても、マルチスレッド的なややこしさは出てしまう
    // それをクラス側の設計の見直しにより改善するのが自分には難しいので、せめて、使い方の分かるテストコードを用意しておく

    internal static class iStopwatchTester
    {
        public static void TestEverything ()
        {
            // TagType は何でもよいので、ここでは前半のみで StartUtc + ElapsedTime を入れておく
            nConcurrentStopwatch <DateTime> xStopwatch = new nConcurrentStopwatch <DateTime> ();

            // =============================================================================

            // 古いデータを入れ、TotalElapsedTime をテスト

            xStopwatch.AddPreviousEntry (new nStopwatchEntry <DateTime>
            {
                Guid = Guid.NewGuid (),
                Name = "Previous Entry #1",
                StartUtc = DateTime.UtcNow.AddHours (-2),
                ElapsedTime = TimeSpan.FromMinutes (Random.Shared.Next (1, 60))
            });

            xStopwatch.PreviousEntries [0].Tag = xStopwatch.PreviousEntries [0].StartUtc + xStopwatch.PreviousEntries [0].ElapsedTime;
            Console.WriteLine (xStopwatch.PreviousEntries [0].Tag.ToRoundtripString ()); // → 2023-01-05T08:36:33.1920998Z

            xStopwatch.AddPreviousEntry (DateTime.UtcNow.AddHours (-1), TimeSpan.FromMinutes (Random.Shared.Next (1, 60)));
            Console.WriteLine (xStopwatch.TotalElapsedTime.ToRoundtripString ()); // → 01:24:00

            // =============================================================================

            // 3秒で自動中断するようにしてから計測を開始
            // 3秒くらいで計測が止まるのを確認

            Console.WriteLine (xStopwatch.AutoPausingTask.Status); // → Running

            xStopwatch.AutoPausingInterval = TimeSpan.FromSeconds (3);

            xStopwatch.Start ();

            while (xStopwatch.IsRunning)
                Thread.Sleep (xStopwatch.AutoPausingThreadSleepTimeout);

            Console.WriteLine (xStopwatch.PreviousEntries [xStopwatch.PreviousEntries.Count - 1].ElapsedTime.ToRoundtripString ()); // → 00:00:03.0428646

            // =============================================================================

            // 3秒ごとに自動中断する設定において2秒ごとのノックを6秒間行う
            // 直後、まだ計測しているものを Stop で終了
            // 6秒くらいになれば成功

            // また、自動中断されたものを Resume した場合に過去データにおいて最後の2件の GUID が一致するのを確認

            xStopwatch.Resume ();

            for (int temp = 0; temp < 3; temp ++)
            {
                Thread.Sleep (2000);
                xStopwatch.Knock (resumes: false);
            }

            xStopwatch.Stop ();

            Console.WriteLine (xStopwatch.PreviousEntries [xStopwatch.PreviousEntries.Count - 1].ElapsedTime.ToRoundtripString ()); // → 00:00:06.0341369

            Console.WriteLine (xStopwatch.PreviousEntries [xStopwatch.PreviousEntries.Count - 1].Guid == xStopwatch.PreviousEntries [xStopwatch.PreviousEntries.Count - 2].Guid); // → True

            // =============================================================================

            // スレッド数が更新されているのを確認
            // 瞬間的に Task を止めるのでなく、止めるフラグを立てて待つ実装
            // そのうち止まるのを Thread.Sleep で少し長めに待つ

            Console.WriteLine (nConcurrentStopwatch.ThreadCount); // → 1
            Console.WriteLine (nLibrary.ThreadCount); // → 1

            xStopwatch.Dispose ();
            Thread.Sleep (xStopwatch.AutoPausingThreadSleepTimeout * 2);

            Console.WriteLine (nConcurrentStopwatch.ThreadCount); // → 0
            Console.WriteLine (nLibrary.ThreadCount); // → 0

            // =============================================================================

            // トータルの経過時間が正しいのを確認
            // Dispose 後も取得できる

            Console.WriteLine (xStopwatch.TotalElapsedTime.ToRoundtripString ()); // → 01:24:09.0770015
        }
    }
}
