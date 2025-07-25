# SCU.MemoryChunks
![Logo](https://github.com/sapozhnikovv/SCU.MemoryChunks/blob/main/img/mem.chunks.png)

Minimal, Effective, Safe and Simple extension with single functionality - Split strings or arrays by size into chunks, without allocation redundant intermediate arrays like in LINQ version (Chunk method).

This extension was originally designed to split strings, because when you use linq Chunk(N) you need to convert char arrays to strings. 

**Issues with LINQ Approach**:
When using `text.Chunk(N).Select(c => new string(c)).ToArray()`, you incur:
1. `N` redundant temporary `char[]` allocations (from `Chunk()`)
2. `N` new string allocations
3. Internal buffer resizing in `Chunks()` method

**This Solution**:
- Zero intermediate allocations
- Direct slicing of source string
- Controlled allocation points via `.ToString()`

### Why not 'Substrings'?
Yes, it would be possible to do with 'substring', it would be even faster, but in case you need to work with Memory<> I decided to make this universal extension.
  
This extension split strings without this redundant allocations, only N strings in result. 
This extension can be used for arrays too (not only for strings). 
It will be better than Linq too, because in this situation you will have N allocations of arrays, but Linq version has logic for array resize, and there is a possibility to have more than N allocations.

 - **No redundant** memory allocations
 - Almost **zero cognitive complexity**
 - Without 'ref struct enumerators', without self-written enumerators, can be used on almost all .net f/c versions (copy-paste in project and add System.Memory package if you have .net standard <= 2.0 or something old )
 - **Logic in 3 lines of code**
 - **Async-compatible** and **Memory-safe** (uses `Memory<T>` instead of `Span<T>`)
 - **11.2x faster** than LINQ in synchronous operations (**1.7x memory efficiency**)

Less code, less problems.


You can use Nuget package MemoryChunks.SCU, or copy-past code of this extension in you project, or not use it. It's up to you :)

Code:
```c#
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

```


# Nuget (only .net8.0 for now)
https://www.nuget.org/packages/MemoryChunks.SCU
Note: Nuget not passing name SCU.MemoryChunks, so in Nuget this extension has name MemoryChunks.SCU, but in C# code name of lib is SCU.MemoryChunks

```shell
dotnet add package MemoryChunks.SCU
```
or
```shell
NuGet\Install-Package MemoryChunks.SCU
```

```c#
using SCU.MemoryChunks;
```

# MemoryChunks vs LINQ Chunk Performance Benchmark

## Test Environment:

 - .NET 8.0.8
 - Intel Core i5-11320H (4 cores @ 3.20GHz)
 - Windows 10 20H2
 - BenchmarkDotNet v0.14.0

## Results
### Benchmark for Strings

| Method                                         | Time (μs) | Ratio | Memory Allocated | Relative Allocation |
|------------------------------------------------|----------:|------:|-----------------:|--------------------:|
| **MemoryChunks** (no string alloc)             |   **1.01** |  1.00 |          **88 B** |               1.00x |
| **MemoryChunks** (with string alloc)           |   **2.69** |  2.66 |      **27,752 B** |             315.36x |
| LINQ Chunk (no string alloc)                   |     28.20 | 27.56 |        28,368 B  |             322.36x |
| LINQ Chunk (with string alloc)                 |     30.02 | 29.66 |        56,032 B  |             636.73x |
| **MemoryChunks Async** (with alloc)            |   **79.32** | 78.36 |      **39,855 B** |             452.90x |
| LINQ Chunk Async (with alloc)                  |     93.40 | 92.27 |        68,128 B  |             774.18x |

**String Chunks Comparison**  
(because we should compare tests with allocations, not just compare tests with no alloc and alloc)
- **Synchronous**: MemoryChunks processes strings **11.2x faster** (2.69μs vs 30.02μs) while using **2x less memory** (27.7KB vs 56KB) than LINQ.  
- **Async**: Maintains **1.2x speed advantage** (79.32μs vs 93.40μs) with **1.7x memory efficiency** (39.8KB vs 68.1KB).  

> **Note about Async tests**:  
> The async benchmarks simulate await points during iteration. While they show MemoryChunks' consistent advantage (1.7x less memory), these scenarios are primarily included to demonstrate thread safety with `Memory<T>`. In real-world usage, the synchronous versions will always be more efficient.


### Benchmark for Arrays

| Method                                                 | Time (μs)  | Ratio | Memory Allocated  | Relative Allocation  |
|--------------------------------------------------------|-----------:|------:|------------------:|---------------------:|
| **MemoryChunks** (no array alloc)                      |   **805**  |  1.00 |          **88 B** |               1.00x  |
| **MemoryChunks** (with array alloc)                    | **3,833**  |  4.76 |      **52,448 B** |             596.00x  |
| LINQ Chunk (with array alloc)                          | 30,181     | 37.48 |       53,504 B    |              608.00x |
| **MemoryChunks Async** (with alloc)                    | **64,255** | 79.75 |      **64,552 B** |             733.55x  |
| LINQ Chunk Async (with alloc)                          | 91,641     | 113.74|       65,600 B    |              745.45x |


**Array Chunks Comparison**  
(because we should compare tests with allocations, not just compare tests with no alloc and alloc)
- **Synchronous**: MemoryChunks handles arrays **7.9x faster** (3,833μs vs 30,181μs) with **2% memory savings** (52.4KB vs 53.5KB).  
- **Async**: Shows **1.4x performance gain** (64,255μs vs 91,641μs) despite near-identical memory usage (64.5KB vs 65.6KB).  .


## Recommended Use Cases

1. **For maximum performance**:
   ```csharp
   foreach (var chunk in text.MemoryChunks(100)) // No allocations
   {
       // Process chunk directly
   }
   ```
   
   or when allocation is needed for string, it will be better than Linq version because Linq produces redundant intermediate arrays (and Linq has logic for Array resize in Chunks method)
   ```csharp
   foreach (var chunk in text.MemoryChunks(100)) // No allocations
   {
       // use chunk.ToString()
   }
   ```
   
   or when allocation is needed for arrays, it will be better than Linq version because Linq produces redundant intermediate arrays (and Linq has logic for Array resize in Chunks method)
   
   ```csharp
   foreach (var chunk in text.MemoryChunks(100)) // No allocations
   {
       // use chunk.ToArray()
   }
   ```
   

 - Can be used in Async methods, because use Memory<>, not Span<> types
 - Safe for strings (immutable by nature)
 - For arrays: ensure source isn't modified during enumeration
 - Safe for long-lived enumerations



## License
Free MIT license (https://github.com/sapozhnikovv/SCU.MemoryChunks/blob/main/LICENSE)
