using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ShowTractor.WinUI.Pages;

namespace ShowTractor.WinUI.Controls
{
    public static class SearhBox
    {
        public static Frame GetSerchResultFrame(DependencyObject obj)
        {
            return (Frame)obj.GetValue(SerchResultFrameProperty);
        }
        public static void SetSerchResultFrame(DependencyObject obj, Frame value)
        {
            var box = (AutoSuggestBox)obj;
            obj.SetValue(SerchResultFrameProperty, value);
            box.QuerySubmitted += Box_QuerySubmitted;
        }
        private static void Box_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var frame = GetSerchResultFrame(sender);
            frame.Navigate(typeof(SearchPage), sender.Text);
        }
        public static readonly DependencyProperty SerchResultFrameProperty =
            DependencyProperty.RegisterAttached("SerchResultFrame", typeof(Frame), typeof(AutoSuggestBox), new PropertyMetadata(null));
    }
}
