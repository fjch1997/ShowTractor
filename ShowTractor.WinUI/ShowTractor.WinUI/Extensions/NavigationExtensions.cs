using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace ShowTractor.WinUI.Extensions
{
    public class NavigationExtensions
    {
        public static Type GetPageType(DependencyObject obj)
        {
            return (Type)obj.GetValue(PageTypeProperty);
        }
        public static void SetPageType(DependencyObject obj, Type value)
        {
            obj.SetValue(PageTypeProperty, value);
        }
        public static readonly DependencyProperty PageTypeProperty =
            DependencyProperty.RegisterAttached("PageType", typeof(Type), typeof(NavigationViewItem), new PropertyMetadata(null));
        public static Frame GetFrame(DependencyObject obj)
        {
            return (Frame)obj.GetValue(FrameProperty);
        }
        public static void SetFrame(DependencyObject obj, Frame value)
        {
            var view = (NavigationView)obj;
            obj.SetValue(FrameProperty, value);
            view.SelectionChanged -= OnSelectionChanged;
            view.SelectionChanged += OnSelectionChanged;
            view.BackRequested -= OnBackRequested;
            view.BackRequested += OnBackRequested;
            value.Navigated -= OnNavigated;
            value.Navigated += OnNavigated;
        }
        public static readonly DependencyProperty FrameProperty =
            DependencyProperty.RegisterAttached("Frame", typeof(Frame), typeof(NavigationView), new PropertyMetadata(null));
        public static Type GetSettingsPageType(DependencyObject obj)
        {
            return (Type)obj.GetValue(SettingsPageTypeProperty);
        }
        public static void SetSettingsPageType(DependencyObject obj, Type value)
        {
            obj.SetValue(SettingsPageTypeProperty, value);
        }
        public static readonly DependencyProperty SettingsPageTypeProperty =
            DependencyProperty.RegisterAttached("SettingsPageType", typeof(Type), typeof(NavigationView), new PropertyMetadata(null));
        private static void OnNavigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            var navigationView = ((Frame)sender).Parent as NavigationView;
            if (navigationView != null)
            {
                var targetPagetype = e.Content.GetType();
                if (!navigationView.MenuItems.Cast<DependencyObject>().Any(i => GetPageType(i) == targetPagetype))
                {
                    navigationView.SelectedItem = null;
                }
            }
        }
        private static void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            var frame = (Frame)sender.Content;
            frame.GoBack();
        }
        private static void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem == null)
                return;
            Type pageType;
            if (sender.SettingsItem == args.SelectedItem)
            {
                pageType = GetSettingsPageType(sender);
            }
            else
            {
                pageType = GetPageType(args.SelectedItemContainer);
            }
            if (pageType != null)
            {
                var frame = (Frame)sender.Content;
                frame.SourcePageType = pageType;
            }
        }
    }
}
