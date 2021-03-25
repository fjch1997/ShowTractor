using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShowTractor.Interfaces
{
    public interface IOpenFileDialogService
    {
        Task<string?> OpenFileAsync(IEnumerable<string> filters);
        Task<string?> OpenFolderAsync();
    }
}
