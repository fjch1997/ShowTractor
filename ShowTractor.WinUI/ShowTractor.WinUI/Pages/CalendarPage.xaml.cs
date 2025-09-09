using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ShowTractor.Pages.Details;
using ShowTractor.WinUI.Extensions;
using System.IO;
using Windows.ApplicationModel.DataTransfer;

namespace ShowTractor.WinUI.Pages
{
    public sealed partial class CalendarPage : Page
    {
        public CalendarPage()
        {
            InitializeComponent();
        }

        private void MenuFlyoutCopy_Click(object sender, RoutedEventArgs e)
        {
            var episode = (CalendarPosterViewModel)((FrameworkElement)sender).DataContext;
            var cleanShowName = string.Join(" ", episode.ShowName.Split(Path.GetInvalidFileNameChars()));
            var data = new DataPackage();
            data.SetText((cleanShowName + " " + episode.D2Identifier).CompactWhitespaces());
            Clipboard.SetContent(data);
        }
    }
}
