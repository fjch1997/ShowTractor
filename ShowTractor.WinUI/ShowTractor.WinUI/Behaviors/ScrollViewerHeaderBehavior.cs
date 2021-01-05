using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System;

namespace ShowTractor.WinUI.Behaviors
{
    /// <summary>
    /// Resize a <see cref="TextBlock"/> based on <see cref="Microsoft.UI.Xaml.Controls.ScrollViewer"/>'s scoll position.
    /// </summary>
    public class ScrollViewerHeaderBehavior : Behavior<TextBlock>
    {
        private long token;
        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(TextBlockProperty); }
            set
            {
                UnregisterCallback();
                SetValue(TextBlockProperty, value);
                RegisterCallback();
            }
        }
        public static readonly DependencyProperty TextBlockProperty =
            DependencyProperty.Register(nameof(ScrollViewer), typeof(ScrollViewer), typeof(ScrollViewerHeaderBehavior), new PropertyMetadata(null));
        public double MinimumFontSize
        {
            get { return (double)GetValue(MinimumSizeProperty); }
            set { SetValue(MinimumSizeProperty, value); }
        }
        public static readonly DependencyProperty MinimumSizeProperty =
            DependencyProperty.Register(nameof(MinimumFontSize), typeof(double), typeof(ScrollViewerHeaderBehavior), new PropertyMetadata(24));
        public double MaximumFontSize
        {
            get { return (double)GetValue(MaximumSizeProperty); }
            set { SetValue(MaximumSizeProperty, value); }
        }
        public static readonly DependencyProperty MaximumSizeProperty =
            DependencyProperty.Register(nameof(MaximumFontSize), typeof(double), typeof(ScrollViewerHeaderBehavior), new PropertyMetadata(36));
        public double ScrollHeight
        {
            get { return (double)GetValue(ScrollHeightProperty); }
            set { SetValue(ScrollHeightProperty, value); }
        }
        public static readonly DependencyProperty ScrollHeightProperty =
            DependencyProperty.Register("ScrollHeight", typeof(double), typeof(ScrollViewerHeaderBehavior), new PropertyMetadata(40));

        protected override void OnAttached()
        {
            RegisterCallback();
            base.OnAttached();
        }
        protected override void OnDetaching()
        {
            UnregisterCallback();
            base.OnDetaching();
        }
        private void RegisterCallback()
        {
            if (ScrollViewer != null)
                token = ScrollViewer.RegisterPropertyChangedCallback(ScrollViewer.VerticalOffsetProperty, OnScrolled);
        }
        private void UnregisterCallback()
        {
            ScrollViewer?.UnregisterPropertyChangedCallback(ScrollViewer.VerticalOffsetProperty, token);
        }
        private void OnScrolled(DependencyObject sender, DependencyProperty dp)
        {
            var scrollViewer = (ScrollViewer)sender;
            var height = Math.Min(scrollViewer.VerticalOffset, ScrollHeight);
            var percentage = height / ScrollHeight;
            AssociatedObject.FontSize = MaximumFontSize - (MaximumFontSize - MinimumFontSize) * percentage;
        }
    }
}
