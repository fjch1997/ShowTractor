using Microsoft.UI.Xaml;
using ShowTractor.Pages.Details;
using ShowTractor.WinUI.Controls;
using ShowTractor.WinUI.Extensions;
using System.IO;
using Windows.ApplicationModel.DataTransfer;

namespace ShowTractor.WinUI.Pages.Details
{
    public sealed partial class TvSeasonPage : NavigationPage
    {
        public TvSeasonPage()
        {
            InitializeComponent(); 
        }

        private void MenuFlyoutCopy_Click(object sender, RoutedEventArgs e)
        {
            var episode = ((FrameworkElement)sender).DataContext as TvEpisodeViewModel;
            var cleanShowName = string.Join(" ", episode.Parent.ShowName.Split(Path.GetInvalidFileNameChars()));
            var data = new DataPackage();
            data.SetText((cleanShowName + " " + episode.D2Identifier).CompactWhitespaces());
            Clipboard.SetContent(data);
        }
    }
}
