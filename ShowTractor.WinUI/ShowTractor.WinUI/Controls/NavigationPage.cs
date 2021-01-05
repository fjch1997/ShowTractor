using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ShowTractor.Mvvm;
using System;

namespace ShowTractor.WinUI.Controls
{
    public class NavigationPage : Page
    {
        public object Parameter
        {
            get { return (object)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register("Parameter", typeof(object), typeof(NavigationPage), new PropertyMetadata(null));

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (DataContext is ISupportNavigation dataContext)
            {
                dataContext.OnNavigatedFrom();
            }
            base.OnNavigatedFrom(e);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameter = e.Parameter is ItemClickEventArgs itemClickEventArgs ? itemClickEventArgs.ClickedItem : e.Parameter;
            Parameter = parameter;
            if (DataContext is ISupportNavigation supportNavigation)
            {
                supportNavigation.OnNavigatedTo(parameter);
            }
            else if (DataContext is ISupportNavigationParameter supportNavigationParameter)
            {
                supportNavigationParameter.Parameter = parameter;
            }
            base.OnNavigatedTo(e);
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (DataContext is ISupportNavigation dataContext)
            {
                e.Cancel = dataContext.OnNavigatingFrom();
            }
            base.OnNavigatingFrom(e);
        }
    }

    public class NavigationPage<T> : NavigationPage
    {
        public NavigationPage()
        {
            ServiceProvider = FindResource(this);
            ViewModel = (T?)ServiceProvider?.GetService(typeof(T));
        }

        private IServiceProvider? FindResource(FrameworkElement current)
        {
            if (Resources.TryGetValue(ServiceProviderResourceKey, out var provider))
                return (IServiceProvider)provider;
            else if (current.Parent is FrameworkElement parentFrameworkElement)
                return FindResource(parentFrameworkElement);
            else
                return null;
        }

        public IServiceProvider? ServiceProvider { get; set; }
        public string ServiceProviderResourceKey { get; set; } = "ServiceProvider";

        public T? ViewModel
        {
            get { return (T?)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(T), typeof(NavigationPage<T>), new PropertyMetadata(null));
    }
}
