using System;

namespace Nekote.Core.Guids
{
    /// <summary>
    /// GUIDの生成を抽象化し、テスト容易性を向上させるためのインターフェース。
    /// </summary>
    public interface IGuidProvider
    {
        /// <summary>
        /// 新しいGUIDを生成します。
        /// </summary>
        /// <returns>新しいGUID。</returns>
        Guid NewGuid();
    }
}
