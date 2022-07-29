using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nExceptionLogger: SortedList <DateTime, Exception>
    {
        public static readonly object Lock = new ();

        /// <summary>
        /// マルチスレッドなら Lock で lock。
        /// </summary>
        public static readonly nExceptionLogger Default = new ();
    }
}
