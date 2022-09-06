using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nStopwatchEntry <DataType, TagType>
        where DataType: class
        where TagType: struct
    {
        public Guid Guid;

        public string? Name;

        private DateTime mStartUtc;

        public DateTime StartUtc
        {
            get
            {
                return mStartUtc;
            }

            set
            {
                mStartUtc = value;
                mElapsedTime = null;
            }
        }

        private DateTime mEndUtc;

        public DateTime EndUtc
        {
            get
            {
                return mEndUtc;
            }

            set
            {
                mEndUtc = value;
                mElapsedTime = null;
            }
        }

        private TimeSpan? mElapsedTime;

        public TimeSpan ElapsedTime
        {
            get
            {
                // 負になるなどを看過
                // プロパティーがあるならメンバー変数に直接アクセスしない作法を適用

                if (mElapsedTime == null)
                    mElapsedTime = EndUtc - StartUtc;

                return mElapsedTime.Value;
            }
        }

        public DataType? Data;

        public TagType? Tag;
    }
}
