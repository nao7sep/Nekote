using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nStopwatch <DataType, TagType>: IDisposable
        where DataType: class
        where TagType: struct
    {
        // lock についての考え方
        // これは、メンバー変数やプロパティーに横からアクセスしてくるループがずっと回るクラス
        // コストも気になるが、i で始まらない public なメソッドやプロパティーでは基本的に必ず lock

        public readonly object Locker = new object ();

        public readonly List <nStopwatchEntry <DataType, TagType>> PreviousEntries = new List <nStopwatchEntry <DataType, TagType>> ();

        public void AddPreviousEntry_lock (nStopwatchEntry <DataType, TagType> entry)
        {
            lock (Locker)
            {
                PreviousEntries.Add (entry);
            }
        }

        public void AddPreviousEntry_lock (DateTime startUtc, TimeSpan elapsedTime)
        {
            AddPreviousEntry_lock (new nStopwatchEntry <DataType, TagType>
            {
                StartUtc = startUtc,
                ElapsedTime = elapsedTime
            });
        }

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

        public TimeSpan CurrentEntryElapsedTime_lock
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

        public TimeSpan TotalElapsedTime_lock
        {
            get
            {
                lock (Locker)
                {
                    long xPreviousEntriesElapsedTimeTicks = PreviousEntries.Sum (x => x.ElapsedTime.Ticks);

                    if (IsRunning)
                        return TimeSpan.FromTicks (xPreviousEntriesElapsedTimeTicks + CurrentEntryElapsedTime_lock.Ticks);

                    else return TimeSpan.FromTicks (xPreviousEntriesElapsedTimeTicks);
                }
            }
        }

        public DataType? CurrentEntryData;

        public TagType? CurrentEntryTag;

        // 自動中断機能について
        // デフォルトでオン
        // オン・オフの切り換えのたびに Task を作ったり止めたりだと状態管理で死ねるので、Task は回しっぱなし
        // 自動中断は、機能がオンで、計測中で、現在時刻が条件を満たしたときに行われる
        // Task 内のループには専用の KILL フラグとして mContinuesAutoPausing が用意される

        private bool mAutoPauses = true;

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

        public static int ThreadCount;

        private Task iRunAutoPausingTask ()
        {
            return Task.Run (() =>
            {
                try
                {
                    Interlocked.Increment (ref ThreadCount);

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
                    Interlocked.Decrement (ref ThreadCount);
                }
            });
        }

        public nStopwatch ()
        {
            mContinuesAutoPausing = true;
            AutoPausingTask = iRunAutoPausingTask ();
        }

        private void iStartOrResume (bool isStarting, string? entryName, DataType? entryData, TagType? entryTag)
        {
            if (isStarting)
            {
                if (IsRunning)
                    throw new nOperationException ();
            }

            else
            {
                if (IsRunning || PreviousEntries.Count == 0)
                    throw new nOperationException ();
            }

            CurrentEntryName = entryName;
            CurrentEntryStartUtc = DateTime.UtcNow;
            CurrentEntryData = entryData;
            CurrentEntryTag = entryTag;

            if (AutoPauses)
                iUpdateNextAutoPausingUtc ();

            Console.WriteLine ("started");
        }

        public void Start_lock (string? entryName = null, DataType? entryData = null, TagType? entryTag = null)
        {
            lock (Locker)
            {
                iStartOrResume (true, entryName, entryData, entryTag);
            }
        }

        public void Resume_lock (string? entryName = null, DataType? entryData = null, TagType? entryTag = null)
        {
            lock (Locker)
            {
                iStartOrResume (false, entryName, entryData, entryTag);
            }
        }

        public void Knock_lock (bool resumes, string? entryName = null, DataType? entryData = null, TagType? entryTag = null)
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
                            iStartOrResume (false, entryName, entryData, entryTag);
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

            nStopwatchEntry <DataType, TagType> xEntry = new nStopwatchEntry <DataType, TagType> ()
            {
                Guid = mCurrentEntryGuid ?? Guid.NewGuid (),
                Name = CurrentEntryName,
                StartUtc = CurrentEntryStartUtc!.Value,
                ElapsedTime = DateTime.UtcNow - CurrentEntryStartUtc!.Value,
                Data = CurrentEntryData,
                Tag = CurrentEntryTag
            };

            // AddPreviousEntry_lock を使わない
            // iPauseOrStop は必ず *_lock 内で呼ばれる
            PreviousEntries.Add (xEntry);

            if (isPausing)
                mCurrentEntryGuid = xEntry.Guid;

            else mCurrentEntryGuid = null;

            CurrentEntryName = null;
            CurrentEntryStartUtc = null;
            CurrentEntryData = null;
            CurrentEntryTag = null;

            Console.WriteLine ("stopped");
        }

        // Pause/Stop において、自動中断機能がオンなら IsRunning == false は問題視されない
        // Knock 同様、いつの間にか中断されていると知らずの Pause/Stop はミスでない

        public void Pause_lock ()
        {
            lock (Locker)
            {
                iPauseOrStop (true);
            }
        }

        public void Stop_lock ()
        {
            lock (Locker)
            {
                iPauseOrStop (false);
            }
        }

        public void Reset ()
        {
            lock (Locker)
            {
                PreviousEntries.Clear ();

                mCurrentEntryGuid = null;
                CurrentEntryName = null;
                CurrentEntryStartUtc = null;
                CurrentEntryData = null;
                CurrentEntryTag = null;

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
        }
    }

    // Data と Tag が不要なら、こちらを使う

    public class nStopwatch: nStopwatch <nEmptyClass, nEmptyStruct>
    {
    }
}
