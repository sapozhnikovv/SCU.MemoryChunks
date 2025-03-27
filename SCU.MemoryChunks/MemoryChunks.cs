using System.Runtime.CompilerServices;

namespace SCU.MemoryChunks
{
    public static class MemoryExtensions
    {
        /// <summary>
        /// Splits to Memory<char> chunks with zero-memory allocation. 
        /// Safe for using in async methods. Strings are immutable, so, you can use this enumaration in code with long-running enumaration.
        /// </summary>
        /// <param name="chunkSize">max size of chunk</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<ReadOnlyMemory<char>> MemoryChunks(this string input, int chunkSize) => MemoryChunks<char>(input.AsMemory(), chunkSize);
        /// <summary>
        /// Splits to Memory<<typeparamref name="T"/>> chunks with zero-memory allocation. 
        /// Safe for using in async methods. 
        /// Make sure <paramref name="input"/> won't be modified during enumeration, otherwise if your enumaration is long-running then use ToArray/ToList to materialize/execute this enumeration before changes in original array.
        /// </summary>
        /// <param name="chunkSize">max size of chunk</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<ReadOnlyMemory<T>> MemoryChunks<T>(this T[] input, int chunkSize) => MemoryChunks<T>(input.AsMemory(), chunkSize);

        /// <summary>
        /// Splits to Memory<<typeparamref name="T"/>> chunks with zero-memory allocation. 
        /// Safe for using in async methods. 
        /// Make sure <paramref name="input"/> won't be modified during enumeration, otherwise if your enumaration is long-running then use ToArray/ToList to materialize/execute this enumeration before changes in original array.
        /// </summary>
        /// <param name="chunkSize">max size of chunk</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<ReadOnlyMemory<T>> MemoryChunks<T>(this ReadOnlyMemory<T> source, int chunkSize)
        {
            if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));
            for (int i = 0; i < source.Length; i += chunkSize)
            {
                yield return source.Slice(i, Math.Min(chunkSize, source.Length - i));
            }
        }
    }
}
