using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nConsole
    {
        public static readonly char [] ProcessingSymbols = { '|', '/', '-', '\\' };

        public static int ProcessingSymbolsCurrentIndex { get; private set; }

        public static char GetNextProcessingSymbol ()
        {
            char xSymbol = ProcessingSymbols [ProcessingSymbolsCurrentIndex];

            if (++ ProcessingSymbolsCurrentIndex >= ProcessingSymbols.Length)
                ProcessingSymbolsCurrentIndex = 0;

            return xSymbol;
        }

        public static void WriteNextProcessingSymbol ()
        {
            Console.Write (GetNextProcessingSymbol ());
        }

        /// <summary>
        /// \r(message)...(symbol) の決め打ち。文字列が短くなるケースに注意。
        /// </summary>
        public static void WriteProcessingMessage (string message)
        {
            Console.Write ($"\r{message}...{GetNextProcessingSymbol ()}");
        }
    }
}
