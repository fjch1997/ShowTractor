using Microsoft.UI.Xaml;
using ShowTractor.Interfaces;
using ShowTractor.Pages;
using ShowTractor.WinUI.Pages;
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
        public AppServiceProvider() : base(new OpenFileDialogService(), new NotificationService()) { }
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
    }
    public class NotificationService : INotificationService
    {
        public async ValueTask<INotificationService.SyncConflictResolution> ShowSyncConflict(SyncConflictViewModel syncConflictViewModel)
        {
            var dialog = new SyncConflictDialog();
            dialog.XamlRoot = ((App)Application.Current).MainWindow.Content.XamlRoot;
            dialog.DataContext = syncConflictViewModel;
            await dialog.ShowAsync();
            return dialog.Resolution;
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
