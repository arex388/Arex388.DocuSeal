using BenchmarkDotNet.Running;
using System.Reflection;

var assembly = Assembly.GetExecutingAssembly();

if (args.Length == 0) {
	BenchmarkRunner.Run(assembly);
} else {
	BenchmarkSwitcher.FromAssembly(assembly).Run(args);
}
