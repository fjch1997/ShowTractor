using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;

namespace ShowTractor.WinUI.Extensions
{
    public static class ServiceProviderExtension
    {
        private const string ServiceScopeResourceName = "{04519BE9-AAFE-4113-85E0-2246F47E5562}";

        public static IServiceProvider? GetServiceProvider(FrameworkElement obj)
        {
            var existing = (IServiceProvider?)obj.GetValue(ServiceProviderProperty);
            if (existing != null)
                return existing;
            if (obj.Resources.TryGetValue(ServiceScopeResourceName, out var scope))
            {
                return ((IServiceScope)scope).ServiceProvider;
            }
            else
            {
                IServiceProvider? p;
                IServiceScope? s;
                (p, s) = GetProviderFromResourceDictionary(Application.Current.Resources);
                if (p != null)
                {
                    s = p.CreateScope();
                    obj.Resources[ServiceScopeResourceName] = s;
                    return s.ServiceProvider;
                }
                else if (s != null)
                {
                    s = s.ServiceProvider.CreateScope();
                    obj.Resources[ServiceScopeResourceName] = s;
                    return s.ServiceProvider;
                }
                else
                {
                    var current = obj.Parent as FrameworkElement;
                    while (current != null)
                    {
                        (p, s) = GetProviderFromResourceDictionary(current.Resources);
                        if (p != null)
                            s = p.CreateScope();
                        else if (s != null)
                            s = s.ServiceProvider.CreateScope();

                        if (s != null)
                        {
                            obj.Resources[ServiceScopeResourceName] = s;
                            SetServiceProvider(obj, s.ServiceProvider);
                            return s.ServiceProvider;
                        }
                        else
                        {
                            current = current.Parent as FrameworkElement;
                        }
                    }
                    return null;
                }
            }
        }
        public static void SetServiceProvider(FrameworkElement obj, IServiceProvider value)
        {
            obj.Unloaded += Obj_Unloaded;
            obj.SetValue(ServiceProviderProperty, value);
        }
        private static (IServiceProvider? provider, IServiceScope? scope) GetProviderFromResourceDictionary(ResourceDictionary resources)
        {
            foreach (var item in resources)
            {
                if (item.Value is IServiceProvider provider)
                {
                    return (provider, null);
                }
                else if (item.Value is IServiceScope scope)
                {
                    return (null, scope);
                }
            }
            return (null, null);
        }
        private static void Obj_Unloaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)sender;
            frameworkElement.Unloaded -= Obj_Unloaded;
            ((IServiceScope)frameworkElement.Resources[ServiceScopeResourceName]).Dispose();
        }
        public static readonly DependencyProperty ServiceProviderProperty =
            DependencyProperty.RegisterAttached("ServiceProvider", typeof(IServiceProvider), typeof(FrameworkElement), new PropertyMetadata(null));
    }
}
