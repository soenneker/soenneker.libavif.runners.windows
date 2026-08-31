using Soenneker.Tests.HostedUnit;

namespace Soenneker.Libavif.Runners.Windows.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class LibavifWindowsRunnerTests : HostedUnitTest
{
    public LibavifWindowsRunnerTests(Host host) : base(host)
    {
    }

    [Test]
    public void Default()
    {

    }
}
