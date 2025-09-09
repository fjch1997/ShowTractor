using Microsoft.UI.Xaml;
using ShowTractor.Pages.Details;
using ShowTractor.WinUI.Controls;
using ShowTractor.WinUI.Extensions;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

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
            var episode = (TvEpisodeViewModel)((FrameworkElement)sender).DataContext;
            var data = new DataPackage();
            data.SetText((episode.Parent.ShowName + " " + episode.D2Identifier).CleanName());
            Clipboard.SetContent(data);
        }

        private void MenuFlyoutShowInThePirateBay_Click(object sender, RoutedEventArgs e)
        {
            var episode = (TvEpisodeViewModel)((FrameworkElement)sender).DataContext;
            var query = episode.Parent.ShowName + " " + episode.D2Identifier;
            var _ = Launcher.LaunchUriAsync(new Uri("https://thepiratebay.org/search.php?q=" + Uri.EscapeDataString(query)));
        }
    }
}
