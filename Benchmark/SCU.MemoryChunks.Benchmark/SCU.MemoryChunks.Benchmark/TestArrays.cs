using SCU.MemoryChunks;
using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class TestArrays
{
    private int[] Array = null;
    private const int ChunkSize = 100;
    [GlobalSetup]
    public void GlobalSetup()
    {
        Array = Enumerable.Range(0, 12345).ToArray();
    }

    [Benchmark(Baseline = true)]
    public int SyncSplitIntoChunks_MemoryChunks_WithoutAllocationArrays()
    {
        var len = 0;
        foreach (var chunk in Array.MemoryChunks(ChunkSize))
        {
            len += chunk.Length;
        }
        return len;
    }

    [Benchmark]
    public int SyncSplitIntoChunks_MemoryChunks_WithAllocationArrays()
    {
        var len = 0;
        foreach (var chunk in Array.MemoryChunks(ChunkSize))
        {
            len += chunk.ToArray().Length;
        }
        return len;
    }

    [Benchmark]
    public int SyncSplitIntoChunks_LINQ_WithAllocationArrays()
    {
        var len = 0;
        foreach (var chunk in Array.Chunk(ChunkSize))
        {
            len += chunk.Length;
        }
        return len;
    }

    [Benchmark]
    public async Task<int> SyncSplitIntoChunks_MemoryChunks_WithAllocationArrays_Async()
    {
        var len = 0;
        foreach (var chunk in Array.MemoryChunks(ChunkSize))
        {
            await SomeActionAsync().ConfigureAwait(false);
            var arr = chunk.ToArray();
            len += arr.Length;
        }
        return len;
    }

    [Benchmark]
    public async Task<int> SyncSplitIntoChunks_LINQ_WithAllocationArrays_Async()
    {
        var len = 0;
        foreach (var chunk in Array.Chunk(ChunkSize))
        {
            await SomeActionAsync().ConfigureAwait(false);
            var arr = chunk;
            len += arr.Length;
        }
        return len;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task SomeActionAsync()
    {
        await Task.Yield();
    }
}
