using System;
using System.Threading;

namespace Nekote.Core.Randomization
{
    /// <summary>
    /// <see cref="Random"/> を使用して乱数を生成する、<see cref="IRandomProvider"/>のデフォルト実装です。
    /// この実装はスレッドセーフです。
    /// シード値が指定されていない場合は、高性能な <see cref="Random.Shared"/> を使用します。
    /// シード値を指定した場合は、決定論的な乱数列を生成しますが、パフォーマンスが低下します。
    /// </summary>
    public class SystemRandomProvider : IRandomProvider
    {
        private readonly int? _seed;
        private readonly Lock _lock = new();
        private readonly Random? _random;

        /// <summary>
        /// <see cref="Random.Shared"/> を使用して、新しいインスタンスを初期化します。
        /// この実装は高性能で、ロックを必要としません。本番環境ではこのコンストラクタを使用してください。
        /// </summary>
        public SystemRandomProvider()
        {
            _seed = null;
            _random = null;
        }

        /// <summary>
        /// 指定されたシード値を使用して、新しいインスタンスを初期化します。
        /// シード値を使用すると予測可能な決定的な乱数列が生成されます。
        /// 警告: このコンストラクタはテスト専用です。本番環境では使用しないでください。
        /// シード付きインスタンスをシングルトンとして登録すると、アプリケーション全体が
        /// 毎回同じ「ランダム」な動作を示し、正しい動作とパフォーマンスが損なわれます。
        /// また、ロックによるパフォーマンスの低下も発生します。
        /// </summary>
        /// <param name="seed">擬似乱数系列の開始値を計算するために使用する数値。</param>
        public SystemRandomProvider(int seed)
        {
            _seed = seed;
            _random = new Random(seed);
        }

        /// <inheritdoc />
        public int Next()
        {
            if (_seed is null)
                return Random.Shared.Next();

            lock (_lock)
                return _random!.Next();
        }

        /// <inheritdoc />
        public int Next(int maxValue)
        {
            if (_seed is null)
                return Random.Shared.Next(maxValue);

            lock (_lock)
                return _random!.Next(maxValue);
        }

        /// <inheritdoc />
        public int Next(int minValue, int maxValue)
        {
            if (_seed is null)
                return Random.Shared.Next(minValue, maxValue);

            lock (_lock)
                return _random!.Next(minValue, maxValue);
        }

        /// <inheritdoc />
        public long NextInt64()
        {
            if (_seed is null)
                return Random.Shared.NextInt64();

            lock (_lock)
                return _random!.NextInt64();
        }

        /// <inheritdoc />
        public long NextInt64(long maxValue)
        {
            if (_seed is null)
                return Random.Shared.NextInt64(maxValue);

            lock (_lock)
                return _random!.NextInt64(maxValue);
        }

        /// <inheritdoc />
        public long NextInt64(long minValue, long maxValue)
        {
            if (_seed is null)
                return Random.Shared.NextInt64(minValue, maxValue);

            lock (_lock)
                return _random!.NextInt64(minValue, maxValue);
        }

        /// <inheritdoc />
        public double NextDouble()
        {
            if (_seed is null)
                return Random.Shared.NextDouble();

            lock (_lock)
                return _random!.NextDouble();
        }

        /// <inheritdoc />
        public float NextSingle()
        {
            if (_seed is null)
                return Random.Shared.NextSingle();

            lock (_lock)
                return _random!.NextSingle();
        }

        /// <inheritdoc />
        public void NextBytes(byte[] buffer)
        {
            if (_seed is null)
            {
                Random.Shared.NextBytes(buffer);
                return;
            }

            lock (_lock)
                _random!.NextBytes(buffer);
        }

        /// <inheritdoc />
        public void NextBytes(Span<byte> buffer)
        {
            if (_seed is null)
            {
                Random.Shared.NextBytes(buffer);
                return;
            }

            lock (_lock)
                _random!.NextBytes(buffer);
        }

        /// <inheritdoc />
        public T[] GetItems<T>(T[] choices, int length)
        {
            if (_seed is null)
                return Random.Shared.GetItems(choices, length);

            lock (_lock)
                return _random!.GetItems(choices, length);
        }

        /// <inheritdoc />
        public void GetItems<T>(ReadOnlySpan<T> choices, Span<T> destination)
        {
            if (_seed is null)
            {
                Random.Shared.GetItems(choices, destination);
                return;
            }

            lock (_lock)
                _random!.GetItems(choices, destination);
        }

        /// <inheritdoc />
        public T[] GetItems<T>(ReadOnlySpan<T> choices, int length)
        {
            if (_seed is null)
                return Random.Shared.GetItems(choices, length);

            lock (_lock)
                return _random!.GetItems(choices, length);
        }

        /// <inheritdoc />
        public void Shuffle<T>(T[] values)
        {
            if (_seed is null)
            {
                Random.Shared.Shuffle(values);
                return;
            }

            lock (_lock)
                _random!.Shuffle(values);
        }

        /// <inheritdoc />
        public void Shuffle<T>(Span<T> values)
        {
            if (_seed is null)
            {
                Random.Shared.Shuffle(values);
                return;
            }

            lock (_lock)
                _random!.Shuffle(values);
        }
    }
}
