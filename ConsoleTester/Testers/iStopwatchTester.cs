using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace ConsoleTester
{
    internal static class iStopwatchTester
    {
        // 自動中断機能、それはオンのまま日時を遠い未来にしての別スレッドによる Reset の二つをテスト
        // いずれでも nStopwatch.ThreadCount が0になるのを確認できる

        // それだけだと、なくてもよい、あまりにも簡単なテストコードだが、
        //     LINQ の deferred query execution の挙動が興味深いので、あえて未対処のコードを残す

        public static void TestThreadsCompletion ()
        {
            Random xRandom = new Random ();

            var xData = Enumerable.Range (0, 100).Select (x =>
            {
                nStopwatch xStopwatch = new nStopwatch ();

                xStopwatch.AutoPauses = true;
                xStopwatch.AutoPausingInterval = TimeSpan.FromSeconds (1 + x / 10);
                xStopwatch.AutoPausingThreadSleepTimeout = TimeSpan.FromMilliseconds (1 + x);

                Task? xResettingTask = null;

                // 二つに一つ
                if ((x & 1) == 1)
                {
                    xResettingTask = Task.Run (() =>
                    {
                        // AutoPauses をさわらず、自動中断日時を遠い未来に
                        xStopwatch.NextAutoPausingUtc = DateTime.MaxValue;

                        Thread.Sleep (xRandom.Next (1000, 10000));

                        xStopwatch.Reset ();
                    });
                }

                // AutoPausingTask にインスタンスが用意される
                xStopwatch.Start ();

                return (Stopwatch: xStopwatch, AutoPausingTask: xStopwatch.AutoPausingTask, ResettingTask: xResettingTask);
            });

            // この時点では deferred query execution により Range も Select も実行されない
            // ここで xData.Count を実行すると、手元のパソコンでは30秒近く処理が止まる
            // Select 内の Task.Run も、xStopwatch.Start による Task.Run も、
            //     「もう Task のインスタンスはできていますから、スレッドプールに空きができ次第、実行されます」でなく、
            //     スレッドプールがいっぱいだと、そもそも Task.Run が呼び出し元に処理を戻さないようである

            // xData.Count ();

            bool xContinuesDisplayingInfo = true;

            void iDisplayInfo ()
            {
                // パソコンによっては100以上のスレッドを同時実行できるかもしれないので、二つの半角空白を出力
                Console.Write ($"\rnStopwatch.ThreadCount: {nStopwatch.ThreadCount}\x20\x20");
            }

            Task xDisplayingTask = Task.Run (() =>
            {
                while (xContinuesDisplayingInfo)
                {
                    iDisplayInfo ();
                    Thread.Sleep (100);
                }
            });

            // 自分の想定は、一度目の ToArray により deferred query execution が起こり、
            //     そのときに生成・キャッシュされたデータが、Where だけなら2行目で再利用されるというもの
            // 自分なら、ToArray のために Select を実行したまでのデータを xData にキャッシュさせ、
            //     今度は Where なら、xData までのデータは共通なのだから、キャッシュされたものを使うように実装する

            // しかし、.NET の実装では、immediate query execution 系の ToArray によるデータは、
            //     あくまでそれをその場で別の変数に取った場合のみ、最終の形として他でも利用できるようだ
            // つまり、自分が試した限り、
            //     ResultOfToArray.CachedResultOfSelect.CachedResultOfSelect.CachedResultOfRange
            //     のようにはなっていない

            // 対処は容易で、xData.ToArray を呼んで変数に取ればよい
            // そこからの Select や Where なら、Task.Run などまで再実行されることはない

            // nStopwatch の Interlocked.Decrement (ref ThreadCount) をなくしてみた
            // nStopwatch.ThreadCount が200まで上がり、二度の実行になっているのを確認できた

            Task.WaitAll (xData.Select (x => x.AutoPausingTask!).ToArray ());
            Task.WaitAll (xData.Where (x => x.ResettingTask != null).Select (x => x.ResettingTask!).ToArray ());

            xContinuesDisplayingInfo = false;
            xDisplayingTask.Wait ();

            // deferred query execution の実装に関する自分の想定の誤りにより、これも0になるとは限らない
            // AutoPausingTask が全て終わってから一応 ResettingTask も全て終わるのを待つイメージの実装だったが、
            //     Range や直後の Select まで二度実装されたあと、ResettingTask しか見ないので、
            //     NextAutoPausingUtc は null になったがまだ while ループを出ていない AutoPausingTask がカウントされる

            iDisplayInfo ();
            Console.WriteLine ();
        }
    }
}
