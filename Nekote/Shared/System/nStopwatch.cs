using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nStopwatch <DataType, TagType>
        where DataType: class
        where TagType: struct
    {
        // 実装のチェック項目:
        //     ひとまとめの処理が lock で保護されているか
        //     ひとまとめの処理を分断しうるフラグへの書き込みアクセスが lock で制限されているか
        //     各メソッドの始まりと終わりにおいて IsRunning と NextAutoPausingUtc の状態が正しいか
        //     NextAutoPausingUtc のチェック時に IsRunning のチェックも必要でないか

        public object Locker = new object ();

        public readonly List <nStopwatchEntry <DataType, TagType>> PreviousEntries = new List <nStopwatchEntry <DataType, TagType>> ();

        // null でないなら前回が Pause
        private Guid? mCurrentEntryGuid;

        public string? CurrentEntryName;

        // 計測中かどうかは、これにより判断される
        public DateTime? CurrentEntryStartUtc;

        public bool IsRunning
        {
            get
            {
                return CurrentEntryStartUtc != null;
            }
        }

        public TimeSpan CurrentEntryElapsedTime
        {
            get
            {
                lock (Locker)
                {
                    if (IsRunning)
                        return DateTime.UtcNow - CurrentEntryStartUtc!.Value;

                    // 計測中でないなら無理に落とさず0を返す
                    else return TimeSpan.Zero;
                }
            }
        }

        public TimeSpan TotalElapsedTime
        {
            get
            {
                lock (Locker)
                {
                    return TimeSpan.FromTicks (PreviousEntries.Sum (x => x.ElapsedTime.Ticks) + CurrentEntryElapsedTime.Ticks);
                }
            }
        }

        public DataType? CurrentEntryData;

        public TagType? CurrentEntryTag;

        // デフォルトではオフなので明示的に
        public bool AutoPauses = false;

        // 最後の計測時の Task のインスタンスが入る
        // 不要のようなので Dispose や null 設定はされない
        public Task? AutoPausingTask;

        public TimeSpan AutoPausingInterval = TimeSpan.FromMinutes (3);

        // 自動中断機能が動いているかどうかは、AutoPausingTask の状態でなく、この変数により判断される
        public DateTime? NextAutoPausingUtc;

        public TimeSpan AutoPausingThreadSleepTimeout = TimeSpan.FromMilliseconds (100);

        private void iStartOrResume (string? entryName, DataType? entryData, TagType? entryTag)
        {
            lock (Locker)
            {
                CurrentEntryName = entryName;
                CurrentEntryStartUtc = DateTime.UtcNow;
                CurrentEntryData = entryData;
                CurrentEntryTag = entryTag;

                if (AutoPauses)
                {
                    if (NextAutoPausingUtc != null)
                        return;

                    NextAutoPausingUtc = DateTime.UtcNow.Add (AutoPausingInterval);

                    AutoPausingTask = Task.Run (() =>
                    {
                        while (true)
                        {
                            // lock 内の lock だが、別スレッドなので関係なし

                            lock (Locker)
                            {
                                if (IsRunning == false || NextAutoPausingUtc == null)
                                    break;

                                else if (DateTime.UtcNow >= NextAutoPausingUtc)
                                {
                                    iPauseOrStop (true);
                                    break;
                                }
                            }

                            Thread.Sleep (AutoPausingThreadSleepTimeout);
                        }
                    });
                }
            }
        }

        public void Start (string? entryName = null, DataType? entryData = null, TagType? entryTag = null)
        {
            if (IsRunning)
                throw new nOperationException ();

            iStartOrResume (entryName, entryData, entryTag);
        }

        public void Knock (bool resumes, string? entryName = null, DataType? entryData = null, TagType? entryTag = null)
        {
            if (AutoPauses == false)
                throw new nOperationException ();

            // Knock が瞬間的に二度呼ばれても iStartOrResume が二度実行されないように lock

            lock (Locker)
            {
                if (IsRunning && NextAutoPausingUtc != null)
                    NextAutoPausingUtc = DateTime.UtcNow.Add (AutoPausingInterval);

                else if (resumes)
                    iStartOrResume (entryName, entryData, entryTag);
            }
        }

        private void iPauseOrStop (bool isPausing)
        {
            lock (Locker)
            {
                if (AutoPauses)
                {
                    // lock 状態で、つまり、別スレッドが動きようのない状態でフラグを倒す
                    // Sleep 中またはその直前だとスレッドが短時間生き延びるが、
                    //     バックグラウンドスレッドだし、処理もシンプルなので問題なし

                    if (NextAutoPausingUtc != null)
                        NextAutoPausingUtc = null;

                    // フラグが既に倒れているなら、計測は自動中断されている
                    // その場合、現行データもないのでメソッドを抜ける
                    else return;
                }

                nStopwatchEntry <DataType, TagType> xEntry = new nStopwatchEntry <DataType, TagType> ()
                {
                    Guid = mCurrentEntryGuid ?? Guid.NewGuid (),
                    Name = CurrentEntryName,
                    StartUtc = CurrentEntryStartUtc!.Value,
                    EndUtc = DateTime.UtcNow,
                    Data = CurrentEntryData,
                    Tag = CurrentEntryTag
                };

                PreviousEntries.Add (xEntry);

                if (isPausing)
                    mCurrentEntryGuid = xEntry.Guid;

                else mCurrentEntryGuid = null;

                CurrentEntryName = null;
                CurrentEntryStartUtc = null;
                CurrentEntryData = null;
                CurrentEntryTag = null;
            }
        }

        public void Pause ()
        {
            if (AutoPauses == false && IsRunning == false)
                throw new nOperationException ();

            iPauseOrStop (true);
        }

        public void Resume (string? entryName = null, DataType? entryData = null, TagType? entryTag = null)
        {
            if (IsRunning || PreviousEntries.Count == 0)
                throw new nOperationException ();

            iStartOrResume (entryName, entryData, entryTag);
        }

        public void Stop ()
        {
            if (AutoPauses == false && IsRunning == false)
                throw new nOperationException ();

            iPauseOrStop (false);
        }
    }

    // Data と Tag が不要なら、こちらを使う

    public class nStopwatch: nStopwatch <nEmptyClass, nEmptyStruct>
    {
    }
}
