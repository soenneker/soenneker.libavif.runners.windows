using Microsoft.Extensions.DependencyInjection;
using Soenneker.GitHub.Repositories.Releases.Registrars;
using Soenneker.Libavif.Runners.Windows.Utils;
using Soenneker.Libavif.Runners.Windows.Utils.Abstract;
using Soenneker.Managers.Runners.Registrars;
using Soenneker.Utils.Directory.Registrars;
using Soenneker.Utils.File.Download.Registrars;

namespace Soenneker.Libavif.Runners.Windows;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddHostedService<ConsoleHostedService>()
            .AddSingleton<IFileOperationsUtil, FileOperationsUtil>()
            .AddDirectoryUtilAsSingleton()
            .AddFileDownloadUtilAsSingleton()
            .AddGitHubRepositoriesReleasesUtilAsSingleton()
            .AddRunnersManagerAsSingleton();
    }
}
