using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nNotImplementedException: nException
    {
        public nNotImplementedException ()
        {
        }

        public nNotImplementedException (string message): base (message)
        {
        }

        public nNotImplementedException (string message, Exception inner): base (message, inner)
        {
        }
    }
}
