using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Libavif.Runners.Windows.Utils.Abstract;

public interface IFileOperationsUtil
{
    /// <summary>
    /// Downloads the latest stable libavif Windows artifacts and prepares them for packaging.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The path to the directory containing the prepared artifacts.</returns>
    ValueTask<string> Process(CancellationToken cancellationToken = default);
}
