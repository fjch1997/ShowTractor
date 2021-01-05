using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using System;

namespace ShowTractor.WinUI.Extensions
{
    [MarkupExtensionReturnType(ReturnType = typeof(object))]
    public class FromServiceProviderExtension : MarkupExtension
    {
        public Type? Type { get; set; }
        public string? TypeName { get; set; }
        public IServiceProvider? ServiceProvider { get; set; }
        protected override object? ProvideValue(IXamlServiceProvider serviceProvider)
        {
            if (ServiceProvider == null)
            {
                throw new InvalidOperationException(nameof(ServiceProvider) + " must be set for " + nameof(FromServiceProviderExtension) + ". ");
            }
            else
            {
                Type type;
                if (Type != null)
                    type = Type;
                else if (TypeName != null)
                    type = Type.GetType(TypeName, true) ?? throw new InvalidOperationException($"Type {TypeName} not found.");
                else
                    throw new ArgumentException($"Either {nameof(Type)} or {nameof(TypeName)} must be set.");
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    var constructor = type.GetConstructor(Array.Empty<Type>());
                    return constructor?.Invoke(null);
                }
                else
                {
                    return ServiceProvider.GetService(type) ??
                        throw new InvalidOperationException($"{nameof(FromServiceProviderExtension)} was unable to find the requested service {type.Name} from the service provider.");
                }
            }
        }
    }
}
