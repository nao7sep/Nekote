using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nLibrary
    {
        private static Assembly? mAssembly = null;

        public static Assembly Assembly
        {
            get
            {
                if (mAssembly == null)
                {
                    // Assembly.GetExecutingAssembly でなく Type.Assembly を使えと
                    // いずれにおいても null が得られることはなさそう
                    // ドキュメントを見る限り、例外も飛んでこない

                    // Assembly.GetExecutingAssembly Method (System.Reflection) | Microsoft Learn
                    // https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly.getexecutingassembly

                    // Type.Assembly Property (System) | Microsoft Learn
                    // https://learn.microsoft.com/en-us/dotnet/api/system.type.assembly

                    mAssembly = typeof (nLibrary).Assembly;
                }

                return mAssembly;
            }
        }

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
