using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // いずれローカライゼーションの仕組みも用意するが、その後もローカライズするほどでない文字列は残る
    // 今後も各所で使われる可能性が高く、変更の可能性の低いものを、このクラスにまとめていく

    public static class nStringLiterals
    {
        public static readonly string NullLabel = "(Null)";

        public static readonly string EmptyLabel = "(Empty)";
    }
}
