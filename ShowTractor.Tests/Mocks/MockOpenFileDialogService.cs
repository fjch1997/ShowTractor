using ShowTractor.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShowTractor.Tests.Mocks
{
    class MockOpenFileDialogService : IOpenFileDialogService
    {
        public string? Filename { get; set; }
        public string? FolderName { get; set; }
        public Task<string?> OpenFileAsync(IEnumerable<string> filters)
        {
            return Task.FromResult(Filename);
        }
        public Task<string?> PickFolderAsync()
        {
            return Task.FromResult(FolderName);
        }
        public Task<string?> SaveFileAsync(IDictionary<string, IList<string>> fileTypeChoices, string suggestedFileName, string defaultFileExtension)
        {
            return Task.FromResult(Filename);
        }
    }
}
