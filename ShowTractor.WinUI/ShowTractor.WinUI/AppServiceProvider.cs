using Microsoft.UI.Xaml;
using ShowTractor.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT;

namespace ShowTractor.WinUI
{
    public class AppServiceProvider : ShowTractorServiceProvider
    {
        public AppServiceProvider() : base(new OpenFileDialogService()) { }
    }

    public class OpenFileDialogService : IOpenFileDialogService
    {
        public async Task<string> OpenFileAsync(IEnumerable<string> filters)
        {
            var picker = new FileOpenPicker();
            foreach (var filter in filters)
            {
                picker.FileTypeFilter.Add(filter);
            }
            picker.As<IInitializeWithWindow>().Initialize(((App)Application.Current).MainWindow.As<IWindowNative>().WindowHandle);
            var file = await picker.PickSingleFileAsync();
            return file?.Path;
        }
        public async Task<string> SaveFileAsync(IDictionary<string, IList<string>> fileTypeChoices, string suggestedFileName, string defaultFileExtension)
        {
            var picker = new FileSavePicker();
            picker.SuggestedFileName = suggestedFileName;
            picker.DefaultFileExtension = defaultFileExtension;
            foreach (var choice in fileTypeChoices)
            {
                picker.FileTypeChoices.Add(choice.Key, choice.Value);
            }
            picker.As<IInitializeWithWindow>().Initialize(((App)Application.Current).MainWindow.As<IWindowNative>().WindowHandle);
            var file = await picker.PickSaveFileAsync();
            return file?.Path;
        }
        public async Task<string> PickFolderAsync()
        {
            var picker = new FolderPicker();
            picker.As<IInitializeWithWindow>().Initialize(((App)Application.Current).MainWindow.As<IWindowNative>().WindowHandle);
            var folder = await picker.PickSingleFolderAsync();
            return folder?.Path;
        }
    }

    [ComImport]
    [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInitializeWithWindow
    {
        void Initialize(IntPtr hwnd);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
    internal interface IWindowNative
    {
        IntPtr WindowHandle { get; }
    }
}
