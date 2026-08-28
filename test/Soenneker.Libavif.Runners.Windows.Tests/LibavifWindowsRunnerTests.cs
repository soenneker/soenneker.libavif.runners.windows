using Soenneker.Libavif.Runners.Windows.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Libavif.Runners.Windows.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class LibavifWindowsRunnerTests : HostedUnitTest
{
    private readonly ILibavifWindowsRunner _runner;

    public LibavifWindowsRunnerTests(Host host) : base(host)
    {
        _runner = Resolve<ILibavifWindowsRunner>(true);
    }

    [Test]
    public void Default()
    {

    }
}
