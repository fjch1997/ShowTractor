using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ShowTractor.Pages.Details;
using ShowTractor.WinUI.Extensions;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

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
            var data = new DataPackage();
            data.SetText((episode.ShowName + " " + episode.D2Identifier).CleanName());
            Clipboard.SetContent(data);
        }
        private void MenuFlyoutShowInThePirateBay_Click(object sender, RoutedEventArgs e)
        {
            var episode = (CalendarPosterViewModel)((FrameworkElement)sender).DataContext;
            var query = episode.ShowName + " " + episode.D2Identifier;
            var _ = Launcher.LaunchUriAsync(new Uri("https://thepiratebay.org/search.php?q=" + Uri.EscapeDataString(query)));
        }
    }
}
