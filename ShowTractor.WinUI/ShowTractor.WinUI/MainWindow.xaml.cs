using Microsoft.Data.Sqlite;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace ShowTractor.WinUI
{
    public sealed partial class MainWindow : Window
    {
        private nint icon;

        public MainWindow()
        {
            InitializeComponent();
            var exeHandle = GetModuleHandle(Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName));
            icon = LoadImage(exeHandle, IDI_APPLICATION, IMAGE_ICON, 16, 16, 0);
            AppWindow.SetIcon(Win32Interop.GetIconIdFromIcon(icon));
            Closed += MainWindow_Closed;
        }

		private void MainWindow_Closed(object sender, WindowEventArgs args)
		{
			// Closing main window is equivalent to app shutdown.
			SqliteConnection.ClearAllPools();
            DestroyIcon(icon);
        }

        private const int IDI_APPLICATION = 32512;
        private const int IMAGE_ICON = 1;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern nint GetModuleHandle(string lpModuleName);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern nint LoadImage(nint hinst, nint name, int type, int cx, int cy, int fuLoad);

        // call this if/when you want to destroy the icon (window closed, etc.)
        [DllImport("user32")]
        private static extern bool DestroyIcon(nint hIcon);
    }
}
