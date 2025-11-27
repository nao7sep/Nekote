using System;

namespace Nekote.Core.Guids
{
    /// <summary>
    /// Guid.NewGuid()を使用してGUIDを生成する、IGuidProviderのデフォルト実装です。
    /// </summary>
    public class SystemGuidProvider : IGuidProvider
    {
        /// <inheritdoc />
        public Guid NewGuid() => Guid.NewGuid();
    }
}
