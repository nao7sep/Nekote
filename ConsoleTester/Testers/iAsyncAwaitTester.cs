using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace ConsoleTester
{
    internal static class iAsyncAwaitTester
    {
        // async/await でないメソッド
        // あえて using 内に Task.Run を置き、iStartWritingToFile から抜けたあとに Task の実行がどうなるかを調べる
        // return で抜けては、実行中の Task にお構いなく FileStream.Dispose が呼ばれ、WriteByte が落ちる

        private static Task iStartWritingToFile ()
        {
            using (FileStream xStream = new FileStream (Path.GetTempFileName (), FileMode.Open, FileAccess.Write, FileShare.Read))
            {
                Task xTask = Task.Run (() =>
                {
                    for (int temp = 0; temp < 10; temp ++)
                    {
                        try
                        {
                            xStream.WriteByte (0);
                            Console.WriteLine ("iStartWritingToFile: 書き込みました。");
                        }

                        catch (Exception xException)
                        {
                            Console.WriteLine ($"iStartWritingToFile: 書き込みに失敗しました。{Environment.NewLine}\x20\x20\x20\x20{xException.GetType ().Name}: {xException.Message}");
                        }

                        Thread.Sleep (10);
                    }
                });

                // 10ミリ秒ごとの書き込みを10回
                // 50ミリ秒待ってからの return なので、最初の5回くらいは書き込みに成功する

                Thread.Sleep (50);

                return xTask;
            }
        }

        // 同じことを async/await で行う
        // await は、現在の認識では、最初に現れたときに「ほな、あとは頼んまっさ」のおまじない
        // Task をメソッドの呼び出し元に渡すことが可能で、その後はそのメソッドの終わりまでの処理を逐次実行する
        // このメソッドにおいては、最初の5回くらいは Sleep 中に書き込まれ、直後の await で呼び出し元に Task が渡り、
        //     このメソッドの方は、Task の処理が終わるまで await のところで待ち、それからの Dispose なので最後まで書き込める

        private static async Task iStartWritingToFileAsync ()
        {
            using (FileStream xStream = new FileStream (Path.GetTempFileName (), FileMode.Open, FileAccess.Write, FileShare.Read))
            {
                Task xTask = Task.Run (() =>
                {
                    for (int temp = 0; temp < 10; temp ++)
                    {
                        try
                        {
                            xStream.WriteByte (0);
                            Console.WriteLine ("iStartWritingToFileAsync: 書き込みました。");
                        }

                        catch
                        {
                            Console.WriteLine ("iStartWritingToFileAsync: 書き込みに失敗しました。");
                        }

                        Thread.Sleep (10);
                    }
                });

                Thread.Sleep (50);

                await xTask;
            }
        }

        // このメソッドにより、await で Task を返しているつもりでも、本当に返っているのは「Task を監督しているタスク」のようなものだと分かる
        // yield return のようなイメージだったが、完全に一つの Task しか返ってこないため
        // 「ほな、あとは頼んまっさ」により、Task という、作業員への直通電話回線が得られたように見えるが、
        //     本当に得られているのは、現場で一つ以上の Task を見守る現場監督の方への直通電話回線と考えてよさそう

        // millisecondsTimeout については後述する

        // async/await なしで書くと、ID の出力が並列処理になって数字が混ざる
        // await は残りの処理を逐次実行するため、こちらでは混ざらない

        private static async Task iStartMultipleTasksAsync (int millisecondsTimeout)
        {
            for (int temp = 0; temp < 3; temp ++)
            {
                Task xTask = Task.Run (() =>
                {
                    int xId = temp;

                    for (int tempAlt = 0; tempAlt < 3; tempAlt ++)
                    {
                        Console.WriteLine (xId);
                        Thread.Sleep (10);
                    }
                });

                Thread.Sleep (millisecondsTimeout);

                await xTask;
            }
        }

        private static async Task iStartThrowingExceptionAsync ()
        {
            await Task.Run (() =>
            {
                throw new Exception ();
            });
        }

        // 挙動を見たいだけなので、単一のメソッドに全て入れていく
        // 今後、何か調べたいことがあっても、ここに入れることを検討する

        // Mac での動作を確認した

        public static void TestEverything ()
        {
            // iStartWritingToFile の結果

            // iStartWritingToFile: 書き込みました。
            // iStartWritingToFile: 書き込みました。
            // iStartWritingToFile: 書き込みました。
            // iStartWritingToFile: 書き込みました。
            // TestEverything: iStartWritingToFile から抜けました。
            // iStartWritingToFile: 書き込みに失敗しました。
            //     ObjectDisposedException: Cannot access a closed Stream.
            // iStartWritingToFile: 書き込みに失敗しました。
            //     ObjectDisposedException: Cannot access a closed Stream.
            // iStartWritingToFile: 書き込みに失敗しました。
            //     ObjectDisposedException: Cannot access a closed Stream.
            // iStartWritingToFile: 書き込みに失敗しました。
            //     ObjectDisposedException: Cannot access a closed Stream.
            // iStartWritingToFile: 書き込みに失敗しました。
            //     ObjectDisposedException: Cannot access a closed Stream.
            // iStartWritingToFile: 書き込みに失敗しました。
            //     ObjectDisposedException: Cannot access a closed Stream.

            Task xTask = iStartWritingToFile ();
            Console.WriteLine ("TestEverything: iStartWritingToFile から抜けました。");
            xTask.Wait ();
            Console.WriteLine ();

            // iStartWritingToFileAsync の結果

            // iStartWritingToFileAsync: 書き込みました。
            // iStartWritingToFileAsync: 書き込みました。
            // iStartWritingToFileAsync: 書き込みました。
            // iStartWritingToFileAsync: 書き込みました。
            // iStartWritingToFileAsync: 書き込みました。
            // TestEverything: iStartWritingToFileAsync から抜けました。
            // iStartWritingToFileAsync: 書き込みました。
            // iStartWritingToFileAsync: 書き込みました。
            // iStartWritingToFileAsync: 書き込みました。
            // iStartWritingToFileAsync: 書き込みました。
            // iStartWritingToFileAsync: 書き込みました。

            xTask = iStartWritingToFileAsync ();
            Console.WriteLine ("TestEverything: iStartWritingToFileAsync から抜けました。");
            xTask.Wait ();
            Console.WriteLine ();

            // iStartMultipleTasksAsync (30) の結果

            // 0
            // 0
            // 0
            // TestEverything: iStartMultipleTasksAsync (30) から抜けました。
            // 1
            // 1
            // 1
            // 2
            // 2
            // 2

            xTask = iStartMultipleTasksAsync (30);
            Console.WriteLine ("TestEverything: iStartMultipleTasksAsync (30) から抜けました。");
            xTask.Wait ();
            Console.WriteLine ();

            // iStartMultipleTasksAsync (50) の結果

            // 0
            // 0
            // 0
            // 1
            // 1
            // 1
            // 2
            // 2
            // 2
            // TestEverything: iStartMultipleTasksAsync (50) から抜けました。

            // await は、基本的には「初回登場時に return のように機能するもの」と考えてよいだろう
            // しかし、await 後も処理が続き、await も何度でも登場できることから、「メソッドを抜ける」というイメージを強く持ちすぎるのは危険な気がする

            // このメソッドの場合、待ち時間が短ければ、現場監督がまだ一人目を見ている頃にメソッドを抜けることができる
            // しかし、50ミリ秒待てば、間違いなく一つ目の Task は終わっていて、「残りの処理を逐次実行」のフェーズに移行している
            // その場合、「メソッドを抜ける」という処理までもが、「ほな、あとは頼んまっさ」で代行される処理のリストの方に入るのかもしれない
            // 何度実行しても、30ミリ秒のときのように数字の表示中にメソッドを抜けることはできなかった

            // 次のページにも、await stops execution of lines after it until the async task after it completes とある
            // 50ミリ秒のときにメソッドを抜けるのが最後になるのは、「待ちすぎて一つ目の Task が終わり、lines after it の方に『抜ける』という処理が入るから」との考えで様子見
            // それはつまり、await は、非同期処理を行うに値する大きさの処理にこそ適用し、その処理の最初の Task の開始後、速やかに登場するべきということ

            // c# - How does async await work when if there are multiple awaits in a method which aren't dependent on eachother? - Stack Overflow
            // https://stackoverflow.com/questions/73884534/how-does-async-await-work-when-if-there-are-multiple-awaits-in-a-method-which-ar

            xTask = iStartMultipleTasksAsync (50);
            Console.WriteLine ("TestEverything: iStartMultipleTasksAsync (50) から抜けました。");
            xTask.Wait ();
            Console.WriteLine ();

            // iStartThrowingExceptionAsync の結果

            // TestEverything: iStartThrowingExceptionAsync から抜けました。
            // TestEverything: 例外をキャッチしました。
            //     AggregateException: One or more errors occurred. (Exception of type 'System.Exception' was thrown.)

            // 呼ばれてすぐに例外を投げる async メソッドのテスト

            // try/catch なしではプログラムが落ちたので Wait に適用したところ、AggregateException が飛んできた
            // 直前の WriteLine あたりのタイミングで飛ぶ可能性も想定し、あえて Wait だけを try に入れている
            // 10回ほど試した限り、それで問題なかった

            // マルチスレッドで例外を扱うとややこしい
            // async/await の有無に関係なく、Task を扱うときには、できるだけ例外が飛ばないように作り、
            //     失敗も含めての結果を Task <...> から戻り値で得られるようにする
            // その上で、fire-and-forget 的な並列処理をできるだけ避け、
            //     並列処理を開始するコードの近辺に大きめの try/catch を置く

            xTask = iStartThrowingExceptionAsync ();
            Console.WriteLine ("TestEverything: iStartThrowingExceptionAsync から抜けました。");

            try
            {
                xTask.Wait ();
            }

            catch (Exception xException)
            {
                Console.WriteLine ($"TestEverything: 例外をキャッチしました。{Environment.NewLine}\x20\x20\x20\x20{xException.GetType ().Name}: {xException.Message}");
            }
        }
    }
}
