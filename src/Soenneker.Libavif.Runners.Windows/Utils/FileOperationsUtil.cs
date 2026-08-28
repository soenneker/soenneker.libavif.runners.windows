using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.GitHub.Repositories.Releases.Abstract;
using Soenneker.Libavif.Runners.Windows.Utils.Abstract;
using Soenneker.Utils.Directory.Abstract;
using Soenneker.Utils.File.Download.Abstract;

namespace Soenneker.Libavif.Runners.Windows.Utils;

/// <inheritdoc cref="IFileOperationsUtil"/>
public sealed class FileOperationsUtil : IFileOperationsUtil
{
    private const string Owner = "AOMediaCodec";
    private const string Repository = "libavif";
    private const string AssetName = "windows-artifacts.zip";
    private static readonly string[] _requiredFiles = ["avifenc.exe", "avifdec.exe", "avifgainmaputil.exe"];

    private readonly ILogger<FileOperationsUtil> _logger;
    private readonly IDirectoryUtil _directoryUtil;
    private readonly IGitHubRepositoriesReleasesUtil _releasesUtil;
    private readonly IFileDownloadUtil _fileDownloadUtil;

    public FileOperationsUtil(ILogger<FileOperationsUtil> logger, IDirectoryUtil directoryUtil,
        IGitHubRepositoriesReleasesUtil releasesUtil, IFileDownloadUtil fileDownloadUtil)
    {
        _logger = logger;
        _directoryUtil = directoryUtil;
        _releasesUtil = releasesUtil;
        _fileDownloadUtil = fileDownloadUtil;
    }

    public async ValueTask<string> Process(CancellationToken cancellationToken = default)
    {
        string downloadDirectory = await _directoryUtil.CreateTempDirectory(cancellationToken);
        string? asset = await _releasesUtil.DownloadReleaseAssetByNamePattern(Owner, Repository, downloadDirectory,
            [AssetName], cancellationToken);

        if (asset is null)
            throw new FileNotFoundException($"Could not find '{AssetName}' in the latest stable {Repository} release.");

        string stageDirectory = await _directoryUtil.CreateTempDirectory(cancellationToken);
        ZipFile.ExtractToDirectory(asset, stageDirectory);

        foreach (string requiredFile in _requiredFiles)
        {
            string path = Path.Combine(stageDirectory, requiredFile);
            if (!File.Exists(path))
                throw new FileNotFoundException($"The libavif distribution did not contain '{requiredFile}'.", path);
        }

        string licensePath = Path.Combine(stageDirectory, "LICENSE.libavif");
        string? license = await _fileDownloadUtil.Download(
            "https://raw.githubusercontent.com/AOMediaCodec/libavif/main/LICENSE", filePath: licensePath,
            log: false, cancellationToken: cancellationToken);

        if (license is null || !File.Exists(licensePath))
            throw new FileNotFoundException("Could not download the libavif license.", licensePath);

        await File.WriteAllTextAsync(Path.Combine(stageDirectory, "SOURCE.txt"),
            "Official release artifacts from https://github.com/AOMediaCodec/libavif/releases/latest\n", cancellationToken);

        _logger.LogInformation("Prepared Windows x64 libavif runtime at {StageDirectory}", stageDirectory);
        return stageDirectory;
    }
}
