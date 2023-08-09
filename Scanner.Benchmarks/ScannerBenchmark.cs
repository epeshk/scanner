using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Scanner.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.NativeAot80)]
[MemoryDiagnoser]
public class ScannerBenchmark
{
    private readonly int length = 1024*1024;

    public ScannerBenchmark()
    {
        var random = new Random(1);
        var ints = Enumerable.Range(0, length).Select(_ => random.Next(100000, 1000000)).ToArray();
        Ints = string.Join("\n", ints.Chunk(4).Select(x => string.Join(" ", x)));
        // Ints = string.Join(" ", ints);
        Ascii = Encoding.ASCII.GetBytes(Ints);
    }

    public byte[] Ascii { get; set; }

    [Benchmark]
    public void ReadInt()
    {
        using var sr = new StringReader(Ints);
        using var scanner = new TextScanner(sr);

        while (scanner.TryRead(out int _)) ;
    }

    [Benchmark]
    public void AsciiReadInt()
    {
        using var sr = new MemoryStream(Ascii);
        var scanner = new AsciiScanner(sr);
        while (scanner.TryRead(out int _)) ;
    }

    [Benchmark]
    public void ReadInt_ReadLine()
    {
        using var ms = new StringReader(Ints);

        while (true)
        {
            var line = ms.ReadLine();
            if (line == null) break;

            foreach (var s in line.Split())
            {
                int.Parse(s);
            }
        }
    }

    [Benchmark]
    public void ReadChars()
    {
        using var ms = new StringReader(Ints);
        var scanner = new TextScanner(ms);
  
        while (scanner.TryRead(out char _)) ;
    }

    private string Ints { get; }
}