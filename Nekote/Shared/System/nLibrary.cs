using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nLibrary
    {
        internal static int iThreadCount = 0;

        /// <summary>
        /// 処理の複雑なプログラムなら、このプロパティーによりスレッド数の変遷をチェックする。
        /// </summary>
        public static int ThreadCount
        {
            get
            {
                return iThreadCount;
            }
        }
    }
}
