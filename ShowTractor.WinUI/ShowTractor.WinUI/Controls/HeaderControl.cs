using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ShowTractor.WinUI.Controls
{
    public class HeaderControl : Grid
    {
        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set
            {
                SetValue(ScrollViewerProperty, value);
                value.RegisterPropertyChangedCallback(ScrollViewer.VerticalOffsetProperty, OnScrolled);
            }
        }
        private void OnScrolled(DependencyObject sender, DependencyProperty dp)
        {
            var scrollViewer = (ScrollViewer)sender;
            var height = MaxHeight - scrollViewer.VerticalOffset;
            if (height < HeaderMinHeight)
            {
                height = HeaderMinHeight;
            }
            Height = height;
        }
        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register(nameof(ScrollViewer), typeof(ScrollViewer), typeof(HeaderControl), new PropertyMetadata(null));

        public double HeaderMinHeight
        {
            get { return (double)GetValue(HeaderMinHeightProperty); }
            set { SetValue(HeaderMinHeightProperty, value); }
        }
        public static readonly DependencyProperty HeaderMinHeightProperty =
            DependencyProperty.Register(nameof(HeaderMinHeight), typeof(double), typeof(HeaderControl), new PropertyMetadata(120D));
    }
}
