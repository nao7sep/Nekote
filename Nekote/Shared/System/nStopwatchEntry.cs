using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // 入れ物クラスなので、Guid もコンストラクターで設定されるなどがない
    // 値がないと処理に問題のある StartUtc と ElapsedTime 以外はオプションなので Nullable に

    public class nStopwatchEntry <DataType, TagType>
        where DataType: class
        where TagType: struct
    {
        public Guid? Guid;

        public string? Name;

        public DateTime StartUtc;

        public TimeSpan ElapsedTime;

        public DataType? Data;

        public TagType? Tag;
    }
}
