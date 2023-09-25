using System.Diagnostics;
using Xunit.Sdk;

namespace MonoGame.Extensions.Hosting.IntegrationTests;

[XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.{Platform}")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FactSkipWhenDebuggerIsAttachedAttribute : FactAttribute
{
    public FactSkipWhenDebuggerIsAttachedAttribute()
    {
        if (Debugger.IsAttached)
        {
            Skip = "This integration test can only run when no debugger are attached.";
        }

#if DEBUG
        Skip = "This integration test can only run when no debugger are attached.";
#endif
    }
}
