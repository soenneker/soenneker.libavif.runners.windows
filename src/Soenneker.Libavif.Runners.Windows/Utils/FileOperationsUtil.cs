using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.GitHub.Repositories.Releases.Abstract;
using Soenneker.Libavif.Runners.Windows.Utils.Abstract;
using Soenneker.Utils.Directory.Abstract;
using Soenneker.Utils.File.Download.Abstract;
using Soenneker.Utils.File.Abstract;

namespace Soenneker.Libavif.Runners.Windows.Utils;

/// <inheritdoc cref="IFileOperationsUtil" />
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
    private readonly IFileUtil _fileUtil;

    public FileOperationsUtil(ILogger<FileOperationsUtil> logger, IDirectoryUtil directoryUtil,
        IGitHubRepositoriesReleasesUtil releasesUtil, IFileDownloadUtil fileDownloadUtil, IFileUtil fileUtil)
    {
        _logger = logger;
        _directoryUtil = directoryUtil;
        _releasesUtil = releasesUtil;
        _fileDownloadUtil = fileDownloadUtil;
        _fileUtil = fileUtil;
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
            if (!await _fileUtil.Exists(path, cancellationToken))
                throw new FileNotFoundException($"The libavif distribution did not contain '{requiredFile}'.", path);
        }

        string licensePath = Path.Combine(stageDirectory, "LICENSE.libavif");
        string? license = await _fileDownloadUtil.Download(
            "https://raw.githubusercontent.com/AOMediaCodec/libavif/main/LICENSE", filePath: licensePath,
            log: false, cancellationToken: cancellationToken);

        if (license is null || !await _fileUtil.Exists(licensePath, cancellationToken))
            throw new FileNotFoundException("Could not download the libavif license.", licensePath);

        await _fileUtil.Write(Path.Combine(stageDirectory, "SOURCE.txt"),
            $"Official release artifacts from https://github.com/AOMediaCodec/libavif/releases/latest\nAsset: {AssetName}\n", log: false, cancellationToken);

        _logger.LogInformation("Prepared Windows x64 libavif runtime at {StageDirectory}", stageDirectory);
        return stageDirectory;
    }
}
