using System;
using System.Collections.Generic;

namespace Nekote.Core.Text
{
    /// <summary>
    /// <see cref="StringComparer"/> に関するヘルパーメソッドを提供します。
    /// </summary>
    public static class StringComparerHelper
    {
        /// <summary>
        /// <see cref="StringComparer"/> を対応する <see cref="StringComparison"/> に変換します。
        /// </summary>
        /// <param name="comparer">変換する <see cref="StringComparer"/>。</param>
        /// <returns>対応する <see cref="StringComparison"/> 値。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="comparer"/> が null の場合にスローされます。</exception>
        /// <exception cref="ArgumentException"><paramref name="comparer"/> が既知の <see cref="StringComparer"/> 型に対応していない場合にスローされます。</exception>
        public static StringComparison ToStringComparison(StringComparer comparer)
        {
            if (comparer is null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            if (ReferenceEquals(comparer, StringComparer.Ordinal))
            {
                return StringComparison.Ordinal;
            }

            if (ReferenceEquals(comparer, StringComparer.OrdinalIgnoreCase))
            {
                return StringComparison.OrdinalIgnoreCase;
            }

            if (ReferenceEquals(comparer, StringComparer.CurrentCulture))
            {
                return StringComparison.CurrentCulture;
            }

            if (ReferenceEquals(comparer, StringComparer.CurrentCultureIgnoreCase))
            {
                return StringComparison.CurrentCultureIgnoreCase;
            }

            if (ReferenceEquals(comparer, StringComparer.InvariantCulture))
            {
                return StringComparison.InvariantCulture;
            }

            if (ReferenceEquals(comparer, StringComparer.InvariantCultureIgnoreCase))
            {
                return StringComparison.InvariantCultureIgnoreCase;
            }

            throw new ArgumentException("The specified StringComparer is not a recognized standard comparer.", nameof(comparer));
        }
    }
}
