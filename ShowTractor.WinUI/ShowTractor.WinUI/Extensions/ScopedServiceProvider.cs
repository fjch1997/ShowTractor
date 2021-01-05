using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;

namespace ShowTractor.WinUI.Extensions
{
    public class ScopedServiceProvider : FrameworkElement, IServiceProvider
    {
        private IServiceScope? scope;
        public IServiceProvider? ParentProvider { get; set; }
        public ScopedServiceProvider()
        {
            Unloaded += ScopedServiceProvider_Unloaded;
        }
        private void ScopedServiceProvider_Unloaded(object sender, RoutedEventArgs e)
        {
            scope?.Dispose();
            Unloaded -= ScopedServiceProvider_Unloaded;
        }
        public object? GetService(Type serviceType)
        {
            if (ParentProvider == null) throw new InvalidOperationException($"{nameof(ParentProvider)} is null.");
            if (scope == null)
                scope = ParentProvider.CreateScope();
            return scope.ServiceProvider.GetService(serviceType);
        }
        public object? GetService(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));
            return GetService(Type.GetType(typeName, true) ?? throw new InvalidOperationException());
        }
    }
}
