using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShowTractor.Interfaces
{
    public interface IOpenFileDialogService
    {
        Task<string?> OpenFileAsync(IEnumerable<string> filters);
        Task<string?> PickFolderAsync();
        Task<string?> SaveFileAsync(IDictionary<string, IList<string>> fileTypeChoices, string suggestedFileName, string defaultFileExtension);
    }
}
