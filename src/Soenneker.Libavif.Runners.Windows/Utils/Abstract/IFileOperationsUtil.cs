using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Libavif.Runners.Windows.Utils.Abstract;

public interface IFileOperationsUtil
{
    ValueTask<string> Process(CancellationToken cancellationToken = default);
}
