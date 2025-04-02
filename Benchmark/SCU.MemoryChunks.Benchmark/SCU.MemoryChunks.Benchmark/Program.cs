using SCU.MemoryChunks;
async Task CheckStrinsAsync()
{
    var text = "1234567890qwertyuiop";
    var linq = text.Chunk(3).Select(_ => new string(_)).ToArray();
    var lastManagedThreadId = Thread.CurrentThread.ManagedThreadId;
    var countOfChangedTHreads = 0;
    var enumerable = text.MemoryChunks(3);//can be iterated multiple times
    do
    {
        var i = 0;
        foreach (var chunk in enumerable)
        {
            await Task.Yield();
            var str = chunk.ToString();
            var valueFomrLinqMethod = linq[i++];
            if (valueFomrLinqMethod != str)
            {
                throw new Exception("strings are not equal because of await, need to recheck MemoryChunks sources, probably Span<> used (work on stack), instead of Memory<> (work on heap)");
            }
            if (lastManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                lastManagedThreadId = Thread.CurrentThread.ManagedThreadId;
                countOfChangedTHreads++;
            }
        }
    }
    while (countOfChangedTHreads <= 2);
}
async Task CheckArraysAsync()
{
    var arr = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 };
    var linq = arr.Chunk(3).ToArray();
    var lastManagedThreadId = Thread.CurrentThread.ManagedThreadId;
    var countOfChangedTHreads = 0;
    var enumerable = arr.MemoryChunks(3);//can be iterated multiple times
    do
    {
        var i = 0;
        foreach (var chunk in enumerable)
        {
            await Task.Yield();
            var arrChnk = chunk.ToArray();
            var valueFomrLinqMethod = linq[i++];
            if (!Enumerable.SequenceEqual(valueFomrLinqMethod, arrChnk))
            {
                throw new Exception("arrays are not equal because of await, need to recheck MemoryChunks sources, probably Span<> used (work on stack), instead of Memory<> (work on heap)");
            }
            if (lastManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                lastManagedThreadId = Thread.CurrentThread.ManagedThreadId;
                countOfChangedTHreads++;
            }
        }
    }
    while (countOfChangedTHreads <= 2);
}
await CheckStrinsAsync().ConfigureAwait(false);
await CheckArraysAsync().ConfigureAwait(false);
Console.WriteLine("MemoryChunks working fine, starting Benchmark of Arrays");
BenchmarkDotNet.Running.BenchmarkRunner.Run<TestArrays>();
Console.WriteLine("Press any key to start Benchmark of Strings");
Console.ReadKey();
BenchmarkDotNet.Running.BenchmarkRunner.Run<TestStrings>();
Console.ReadKey();