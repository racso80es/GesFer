using BenchmarkDotNet.Running;

namespace GesFer.Performance.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<StockBenchmark>();
    }
}
