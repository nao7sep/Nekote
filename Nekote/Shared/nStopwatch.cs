using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // .NET の Stopwatch と異なり、こちらは、インスタンス外から構ってやらないとデフォルトでは3分ごとに計測が自動中断するクラス
    // ボタンを押すたびに3分間、水が流れる蛇口をイメージ
    // パソコンをさわっている時間の計測など、一時停止に人間のミスが関わってくるところに便利

    // DataType でクラスを、TagType で構造体を一つ扱える → 区別の必要性が乏しかったため where 句なしの TagType に統一
    // ストップウォッチは、何を計測するかによってはコレクション的な処理も担うことが考えられる
    // そういう処理が不要なら、nEmptyClass などを使用する、ジェネリックでない方を使う → 潰すだけなので object に変更

    // クラスを利用する側での lock をできるだけ減らすため、Locker を内包させ、lock を自動的に行う *_lock メソッドを揃えた → 野暮ったかったので廃止し、<summary> を付けた
    // 一つのプログラム内で nStopwatch のインスタンスを多数使うことは稀だろうが、デッドロックには注意が必要

    public class nStopwatch <TagType>: IDisposable
    {
        // lock についての考え方
        // これは、メンバー変数やプロパティーに横からアクセスしてくるループがずっと回るクラス
        // コストも気になるが、i で始まらない public なメソッドやプロパティーでは基本的に必ず lock

        public readonly object Locker = new object ();

        public readonly List <nStopwatchEntry <TagType>> PreviousEntries = new List <nStopwatchEntry <TagType>> ();

        /// <summary>
        /// 自動 lock。
        /// </summary>
        public void AddPreviousEntry (nStopwatchEntry <TagType> entry)
        {
            lock (Locker)
            {
                PreviousEntries.Add (entry);
            }
        }

        /// <summary>
        /// 自動 lock。
        /// </summary>
        public void AddPreviousEntry (DateTime startUtc, TimeSpan elapsedTime)
        {
            AddPreviousEntry (new nStopwatchEntry <TagType>
            {
                StartUtc = startUtc,
                ElapsedTime = elapsedTime
            });
        }

        // 基本的には、Start, Pause, Resume, Stop の四つで処理が完結する
        // Pause と Stop の違いは、現在進行中の計測に関するエントリーの GUID が次回に引き継がれるかどうか
        // Pause されたなら、PreviousEntries の方で、連続する二つ以上のエントリーの GUID が一致する

        // 古いコメント
        // Pause による GUID の引き継ぎについて
        // Pause は、現行エントリーの GUID が確定したときに、「次のエントリーの GUID にもこれを使ってくれ」としてその値を残す
        // その値は直後が Resume でも Start でも変更されず、その次が Pause でも Stop でもその回の GUID として必ず使われる
        // そのときまた Pause なら、同じ GUID がさらに次の回のために残される

        private Guid? mCurrentEntryGuid;

        public string? CurrentEntryName;

        // 計測中であり、現行エントリーがあるなら null でない
        public DateTime? CurrentEntryStartUtc;

        public bool IsRunning
        {
            get
            {
                // lock は不要
                return CurrentEntryStartUtc != null;
            }
        }

        /// <summary>
        /// 自動 lock。
        /// </summary>
        public TimeSpan CurrentEntryElapsedTime
        {
            get
            {
                lock (Locker)
                {
                    // null や TimeSpan.Zero も選択肢だが、計測中でないならステート認識のミス
                    // 呼び出し側でのチェックがめんどくさいが、ライブラリーはカッチリしているべき

                    if (IsRunning == false)
                        throw new nOperationException ();

                    return DateTime.UtcNow - CurrentEntryStartUtc!.Value;
                }
            }
        }

        /// <summary>
        /// 自動 lock。
        /// </summary>
        public TimeSpan TotalElapsedTime
        {
            get
            {
                lock (Locker)
                {
                    long xPreviousEntriesElapsedTimeTicks = PreviousEntries.Sum (x => x.ElapsedTime.Ticks);

                    if (IsRunning)
                        return TimeSpan.FromTicks (xPreviousEntriesElapsedTimeTicks + CurrentEntryElapsedTime.Ticks);

                    else return TimeSpan.FromTicks (xPreviousEntriesElapsedTimeTicks);
                }
            }
        }

        public TagType? CurrentEntryTag;

        // 自動中断機能について
        // デフォルトでオン
        // オン・オフの切り換えのたびに Task を作ったり止めたりだと状態管理で死ねるので、Task は回しっぱなし
        // 自動中断は、機能がオンで、計測中で、現在時刻が条件を満たしたときに行われる
        // Task 内のループには専用の KILL フラグとして mContinuesAutoPausing が用意される

        private bool mAutoPauses = true;

        /// <summary>
        /// 自動 lock。
        /// </summary>
        public bool AutoPauses
        {
            get
            {
                return mAutoPauses;
            }

            set
            {
                lock (Locker)
                {
                    if (mAutoPauses == false && value)
                    {
                        // 自動中断がオフからオンになる場合、NextAutoPausingUtc が古くてすぐに止まらないように更新
                        iUpdateNextAutoPausingUtc ();
                    }

                    else if (mAutoPauses && value == false)
                    {
                        // オンからオフになる場合、別スレッド内の、自動中断するべきか判断するコードで AutoPauses がチェックされるので特に何もしなくてよい
                    }

                    mAutoPauses = value;
                }
            }
        }

        private bool mContinuesAutoPausing;

        // 最初は Pause/Stop のたびに Task を作り直す実装にしたが、実装がややこしく、使用時にも注意の必要なクラスになった
        // そのため、インスタンスの生成時にコンストラクターで Task が作られ、そのまま回り続ける実装に変更した

        public readonly Task AutoPausingTask;

        public static readonly TimeSpan DefaultAutoPausingInterval = TimeSpan.FromMinutes (3);

        public TimeSpan AutoPausingInterval = DefaultAutoPausingInterval;

        public DateTime? NextAutoPausingUtc;

        private void iUpdateNextAutoPausingUtc ()
        {
            NextAutoPausingUtc = DateTime.UtcNow.Add (AutoPausingInterval);
        }

        public static readonly TimeSpan DefaultAutoPausingThreadSleepTimeout = TimeSpan.FromMilliseconds (100);

        public TimeSpan AutoPausingThreadSleepTimeout = DefaultAutoPausingThreadSleepTimeout;

        private static int mThreadCount = 0;

        /// <summary>
        /// 処理の複雑なプログラムなら、このプロパティーによりスレッド数の変遷をチェックする。
        /// </summary>
        public static int ThreadCount
        {
            get
            {
                return mThreadCount;
            }
        }

        private Task iRunAutoPausingTask ()
        {
            return Task.Run (() =>
            {
                try
                {
                    // 理由は不詳だが、nStopwatch. を省くと正しく動かない

                    // 追記
                    // インスタンスメソッドから静的フィールドにアクセスすることよりデリゲートや ref の関与の方が気になる
                    // nStopwatch. なしでもコンパイルはできて正しく動作しないというのがスッキリしない
                    // こういう条件が揃うと（通常はテストしないほど）簡単な処理でもテストを欠かせないと念頭に置く

                    // C# Static Members cannot be accessed with an instance reference - Stack Overflow
                    // https://stackoverflow.com/questions/52452205/c-sharp-static-members-cannot-be-accessed-with-an-instance-reference

                    // c# - Member '<member name>' cannot be accessed with an instance reference - Stack Overflow
                    // https://stackoverflow.com/questions/1100009/member-member-name-cannot-be-accessed-with-an-instance-reference

                    Interlocked.Increment (ref nStopwatch.mThreadCount);
                    Interlocked.Increment (ref nLibrary.iThreadCount);

                    while (true)
                    {
                        if (mContinuesAutoPausing == false)
                            break;

                        lock (Locker)
                        {
                            // 自動中断機能がオフなら、何もせずループを回す
                            // カウント中で、自動中断するべきなら、Pause の処理を
                            // これにより IsRunning のフラグが倒れる

                            if (AutoPauses && IsRunning && DateTime.UtcNow >= NextAutoPausingUtc)
                                iPauseOrStop (true);
                        }

                        Thread.Sleep (AutoPausingThreadSleepTimeout);
                    }
                }

                finally
                {
                    Interlocked.Decrement (ref nStopwatch.mThreadCount);
                    Interlocked.Decrement (ref nLibrary.iThreadCount);
                }
            });
        }

        public nStopwatch ()
        {
            mContinuesAutoPausing = true;
            AutoPausingTask = iRunAutoPausingTask ();
        }

        private void iStartOrResume (bool isStarting, string? entryName, TagType? entryTag)
        {
            if (isStarting)
            {
                if (IsRunning)
                    throw new nOperationException ();
            }

            else
            {
                // 古いデータがないなら、「再開」より「開始」が適する
                // ここでの「再開」は、何らかのミスの可能性がある

                if (IsRunning || PreviousEntries.Count == 0)
                    throw new nOperationException ();
            }

            // mCurrentEntryGuid は iPauseOrStop により既に調整されている

            CurrentEntryName = entryName;
            CurrentEntryStartUtc = DateTime.UtcNow;
            CurrentEntryTag = entryTag;

            if (AutoPauses)
                iUpdateNextAutoPausingUtc ();
        }

        /// <summary>
        /// 自動 lock。
        /// </summary>
        public void Start (string? entryName = null, TagType? entryTag = default)
        {
            lock (Locker)
            {
                iStartOrResume (true, entryName, entryTag);
            }
        }

        /// <summary>
        /// 自動 lock。
        /// </summary>
        public void Resume (string? entryName = null, TagType? entryTag = default)
        {
            lock (Locker)
            {
                iStartOrResume (false, entryName, entryTag);
            }
        }

        // 上述した、水を流し続けるためのボタンのイメージ
        // デフォルトでは3分間で自動中断なので、3分未満（「以内」でない）のインターバルでのノックが必要
        // 既に自動中断されていてのノックの場合、水がその時点から再び流れるのと同様の処理に

        /// <summary>
        /// 自動 lock。
        /// </summary>
        public void Knock (bool resumes, string? entryName = null, TagType? entryTag = default)
        {
            lock (Locker)
            {
                if (AutoPauses)
                {
                    if (IsRunning)
                        iUpdateNextAutoPausingUtc ();

                    else
                    {
                        if (resumes)
                            iStartOrResume (false, entryName, entryTag);
                    }
                }

                else
                {
                    // 自動中断がオフのときの Knock は、ステート認識のミス
                    throw new nOperationException ();
                }
            }
        }

        private void iPauseOrStop (bool isPausing)
        {
            if (AutoPauses)
            {
                // 自動中断機能がオンのときに IsRunning が倒れていれば、
                //     既に別ループでの iPauseOrStop により Current* のデータが移されている
                // 正常な動作においてよくあることなのでエラー扱いされない

                if (IsRunning == false)
                    return;
            }

            else
            {
                if (IsRunning == false)
                    throw new nOperationException ();
            }

            // CurrentEntryStartUtc が null でない前提の処理
            // カウント中であり、自動中断もされていないことの確認は呼び出し側が

            nStopwatchEntry <TagType> xEntry = new nStopwatchEntry <TagType> ()
            {
                Guid = mCurrentEntryGuid ?? Guid.NewGuid (),
                Name = CurrentEntryName,
                StartUtc = CurrentEntryStartUtc!.Value,
                ElapsedTime = DateTime.UtcNow - CurrentEntryStartUtc!.Value,
                Tag = CurrentEntryTag
            };

            // AddPreviousEntry を使わない
            // iPauseOrStop は必ず *_lock 内で呼ばれる → _lock を廃止し、<summary> を付けた
            PreviousEntries.Add (xEntry);

            if (isPausing)
                mCurrentEntryGuid = xEntry.Guid;

            else mCurrentEntryGuid = null;

            CurrentEntryName = null;
            CurrentEntryStartUtc = null;
            CurrentEntryTag = default;
        }

        // Pause/Stop において、自動中断機能がオンなら IsRunning == false は問題視されない
        // Knock 同様、いつの間にか中断されていると知らずの Pause/Stop はミスでない

        /// <summary>
        /// 自動 lock。
        /// </summary>
        public void Pause ()
        {
            lock (Locker)
            {
                iPauseOrStop (true);
            }
        }

        /// <summary>
        /// 自動 lock。
        /// </summary>
        public void Stop ()
        {
            lock (Locker)
            {
                iPauseOrStop (false);
            }
        }

        /// <summary>
        /// 自動 lock。
        /// </summary>
        public void Reset ()
        {
            lock (Locker)
            {
                PreviousEntries.Clear ();

                mCurrentEntryGuid = null;
                CurrentEntryName = null;
                CurrentEntryStartUtc = null;
                CurrentEntryTag = default;

                AutoPauses = true;
                AutoPausingInterval = DefaultAutoPausingInterval;
                NextAutoPausingUtc = null;
                AutoPausingThreadSleepTimeout = DefaultAutoPausingThreadSleepTimeout;
            }
        }

        private bool mIsDisposed = false;

        public void Dispose ()
        {
            if (mIsDisposed == false)
            {
                mContinuesAutoPausing = false;
                mIsDisposed = true;
            }

            // CA1816: Call GC.SuppressFinalize correctly (code analysis) - .NET | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1816

            GC.SuppressFinalize (this);
        }
    }

    // Tag が不要なら、こちらを使う

    public class nStopwatch: nStopwatch <object>
    {
    }
}
