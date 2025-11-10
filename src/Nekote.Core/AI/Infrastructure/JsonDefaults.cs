using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure
{
    /// <summary>
    /// JSON シリアライゼーションのデフォルトオプションを提供します。
    /// </summary>
    internal static class JsonDefaults
    {
        /// <summary>
        /// 標準オプション (フォーマットなし、デシリアライズ用)。
        /// </summary>
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// フォーマット付きオプション (インデント、診断データ記録用)。
        /// </summary>
        public static readonly JsonSerializerOptions FormattedOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }
}
