using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nNotSupportedException: nException
    {
        public nNotSupportedException ()
        {
        }

        public nNotSupportedException (string message): base (message)
        {
        }

        public nNotSupportedException (string message, Exception inner): base (message, inner)
        {
        }
    }
}
