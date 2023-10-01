using System.Diagnostics;

namespace Troublecat.Core.Diagnostics;

internal class PerfScope : IDisposable {
    private readonly Stopwatch _stopwatch = new Stopwatch();

    private readonly string _name;

    public PerfScope(string? name = null) {
        _name = name ?? "PerfScope";
        _stopwatch.Start();
    }

    public void Dispose() {
        _stopwatch.Stop();
    }

    public override string ToString() {
        return $"{_name}: {_stopwatch.ElapsedMilliseconds}ms";
    }
}
