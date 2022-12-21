using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using Nekote;

namespace ConsoleTester
{
    internal class Program
    {
        static void Main (string [] args)
        {
            try
            {
            }

            catch (Exception xException)
            {
                nException.Log (xException);
                nConsole.WriteErrorHasOccurredMessage (xException);
                nConsole.WritePressAnyKeyToCloseThisWindowMessage ();
            }
        }
    }
}
