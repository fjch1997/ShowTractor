using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml;

namespace ShowTractor.WinUI
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
			Closed += MainWindow_Closed;
        }

		private void MainWindow_Closed(object sender, WindowEventArgs args)
		{
			// Closing main window is equivalent to app shutdown.
			SqliteConnection.ClearAllPools();
		}
	}
}
