using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nExceptionLogger
    {
        public static readonly object Locker = new object ();

        /// <summary>
        /// マルチスレッドなら Locker で lock。
        /// </summary>
        public static readonly nExceptionLogger Default = new nExceptionLogger ();

        // アプリのログの特性としては、「それを吐いたアプリが古いものまで把握しても、できることがさほどない」というのがある
        // もちろん、「このアプリでこれまでに発生した全てのエラーを表示」といった機能もアプリによっては必要だが、
        //     たいていのアプリでは、「エラーが起こったのでログを見てちょんまげ」を出し、できるだけ状態を復旧するだけ
        // そのため、Nekote としては最近のエントリーがメモリー上に残るだけで十分
        // 古いログデータをファイルからロードしたければ、それに適したデータプロバイダーのクラスに頼る

        // DateTime は Comparer <DateTime>.Default で問題なく処理されるため comparer の指定が不要

        public readonly SortedList <DateTime, Exception> RecentEntries = new SortedList <DateTime, Exception> ();

        public void Log <ExceptionType> (ExceptionType exception, DateTime? utc = null)
            where ExceptionType: Exception
        {
            RecentEntries.Add (utc ?? DateTime.UtcNow, exception);
        }
    }
}
