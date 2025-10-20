using System;

namespace Nekote.Core.Randomization
{
    /// <summary>
    /// System.Random を使用して乱数を生成する、IRandomProviderのデフォルト実装です。
    /// この実装はスレッドセーフです。
    /// </summary>
    public class SystemRandomProvider : IRandomProvider
    {
        private readonly object _lock = new();
        private readonly Random _random;

        /// <summary>
        /// 時間に依存したデフォルトのシード値を使用して、新しいインスタンスを初期化します。
        /// </summary>
        public SystemRandomProvider() => _random = new Random();

        /// <summary>
        /// 指定されたシード値を使用して、新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="seed">擬似乱数系列の開始値を計算するために使用する数値。</param>
        public SystemRandomProvider(int seed) => _random = new Random(seed);

        /// <inheritdoc />
        public int Next() { lock (_lock) return _random.Next(); }

        /// <inheritdoc />
        public int Next(int maxValue) { lock (_lock) return _random.Next(maxValue); }

        /// <inheritdoc />
        public int Next(int minValue, int maxValue) { lock (_lock) return _random.Next(minValue, maxValue); }

        /// <inheritdoc />
        public long NextInt64() { lock (_lock) return _random.NextInt64(); }

        /// <inheritdoc />
        public long NextInt64(long maxValue) { lock (_lock) return _random.NextInt64(maxValue); }

        /// <inheritdoc />
        public long NextInt64(long minValue, long maxValue) { lock (_lock) return _random.NextInt64(minValue, maxValue); }

        /// <inheritdoc />
        public double NextDouble() { lock (_lock) return _random.NextDouble(); }

        /// <inheritdoc />
        public float NextSingle() { lock (_lock) return _random.NextSingle(); }

        /// <inheritdoc />
        public void NextBytes(byte[] buffer) { lock (_lock) _random.NextBytes(buffer); }

        /// <inheritdoc />
        public void NextBytes(Span<byte> buffer) { lock (_lock) _random.NextBytes(buffer); }

        /// <inheritdoc />
        public T[] GetItems<T>(T[] choices, int length) { lock (_lock) return _random.GetItems(choices, length); }

        /// <inheritdoc />
        public void GetItems<T>(ReadOnlySpan<T> choices, Span<T> destination) { lock (_lock) _random.GetItems(choices, destination); }

        /// <inheritdoc />
        public T[] GetItems<T>(ReadOnlySpan<T> choices, int length) { lock (_lock) return _random.GetItems(choices, length); }

        /// <inheritdoc />
        public void Shuffle<T>(T[] values) { lock (_lock) _random.Shuffle(values); }

        /// <inheritdoc />
        public void Shuffle<T>(Span<T> values) { lock (_lock) _random.Shuffle(values); }
    }
}
