using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // xArray [1, 2, 3] = 100 をいきなりやれる、自動拡大する多次元配列
    // GetSubarray により特定の次元のデータを nMultiArray として取得できる
    // LINQ との親和性が高く、ちょっとした集計に便利

    public class nMultiArray <ElementType>
    {
        public ElementType? Value;

        private List <nMultiArray <ElementType>>? mSubarrays = null;

        public List <nMultiArray <ElementType>> Subarrays
        {
            get
            {
                if (mSubarrays == null)
                    mSubarrays = new List <nMultiArray <ElementType>> ();

                return mSubarrays;
            }
        }

        public nMultiArray <ElementType> GetSubarray (int index)
        {
            if (index >= Subarrays.Count)
            {
                for (int temp = Subarrays.Count; temp <= index; temp ++)
                    Subarrays.Add (new nMultiArray <ElementType> ());
            }

            return Subarrays [index];
        }

        public nMultiArray <ElementType> GetSubarray (params int [] indices)
        {
            if (indices.Length == 0)
                throw new nArgumentException ();

            nMultiArray <ElementType>? xSubarray = null;

            foreach (int xIndex in indices)
            {
                if (xSubarray == null)
                    xSubarray = GetSubarray (xIndex);

                else xSubarray = xSubarray.GetSubarray (xIndex);
            }

            // indices.Length を見ているのに、コンパイラーが「null かもしれない」と言ってくる
            return xSubarray!;
        }

        // 値型なら、フィールド、引数、戻り値などに ? が付いていても Nullable にならない
        // 詳しくは、iGenericTester.TestNullability に

        // 引数が一つだけなら一つ目が呼ばれる
        // 二つを作り分けることで、少しでも高速化

        public ElementType? this [int index]
        {
            get
            {
                return GetSubarray (index).Value;
            }

            set
            {
                GetSubarray (index).Value = value;
            }
        }

        public ElementType? this [params int [] indices]
        {
            get
            {
                return GetSubarray (indices).Value;
            }

            set
            {
                GetSubarray (indices).Value = value;
            }
        }
    }
}
