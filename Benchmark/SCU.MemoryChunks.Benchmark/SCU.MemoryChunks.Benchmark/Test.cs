using SCU.MemoryChunks;
using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class Test
{
    private string Text = null;
    private const int ChunkSize = 100;
    [GlobalSetup]
    public void GlobalSetup()
    {
        Text = new string(Enumerable.Range(0, 12345).Select(_ => (char)_).ToArray());
    }

    [Benchmark(Baseline = true)]
    public int SyncSplitIntoChunks_MemoryChunks_WithoutAllocationStrings()
    {
        var len = 0;
        foreach (var chunk in Text.MemoryChunks(ChunkSize))
        {
            len += chunk.Span.Length;
        }
        return len;
    }

    [Benchmark]
    public int SyncSplitIntoChunks_LINQ_WithoutAllocationStrings()
    {
        var len = 0;
        foreach (var chunk in Text.Chunk(ChunkSize))
        {
            len += chunk.Length;
        }
        return len;
    }

    [Benchmark]
    public int SyncSplitIntoChunks_MemoryChunks_WithAllocationStrings()
    {
        var len = 0;
        foreach (var chunk in Text.MemoryChunks(ChunkSize))
        {
            len += chunk.ToString().Length;
        }
        return len;
    }

    [Benchmark]
    public int SyncSplitIntoChunks_LINQ_WithAllocationStrings()
    {
        var len = 0;
        foreach (var chunk in Text.Chunk(ChunkSize))
        {
            len += new string(chunk).Length;
        }
        return len;
    }

    [Benchmark]
    public async Task<int> SyncSplitIntoChunks_MemoryChunks_WithAllocationStrings_Async()
    {
        var len = 0;
        foreach (var chunk in Text.MemoryChunks(ChunkSize))
        {
            await SomeActionAsync().ConfigureAwait(false);
            var str = chunk.ToString();
            len += str.Length;
        }
        return len;
    }

    [Benchmark]
    public async Task<int> SyncSplitIntoChunks_LINQ_WithAllocationStrings_Async()
    {
        var len = 0;
        foreach (var chunk in Text.Chunk(ChunkSize))
        {
            await SomeActionAsync().ConfigureAwait(false);
            var str = new string(chunk);
            len += str.Length;
        }
        return len;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task SomeActionAsync()
    {
        await Task.Yield();
    }
}
