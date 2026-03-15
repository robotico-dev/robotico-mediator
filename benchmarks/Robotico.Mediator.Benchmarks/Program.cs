using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Robotico.Mediator.Benchmarks;

public static class Program
{
    public static int Main(string[] args)
    {
        ManualConfig config = ManualConfig.Create(DefaultConfig.Instance)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);
        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        return 0;
    }
}
