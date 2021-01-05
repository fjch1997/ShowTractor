using ShowTractor.Plugins.Interfaces;
using System;
using System.Linq;
using System.Reflection;

namespace ShowTractor.Plugins
{
    public class PluginDefinition
    {
        public string? FileName { get; set; }
        public bool Enabled { get; set; }
        public T Load<T>(IServiceProvider provider) where T : IPlugin
        {
            if (string.IsNullOrEmpty(FileName))
                throw new InvalidOperationException($"{nameof(FileName)} is null.");

            var assembly = Assembly.LoadFrom(FileName);
            var attribute = (ShowTractorPluginAssemblyAttribute)assembly.GetCustomAttributes(typeof(ShowTractorPluginAssemblyAttribute), false).FirstOrDefault();
            if (attribute == null)
                throw new TypeLoadException($"Could not load plugin from assembly {assembly.FullName}. A {nameof(ShowTractorPluginAssemblyAttribute)} was not found.");
            if (attribute.MetadataProvider == null)
                throw new TypeLoadException($"Could not load plugin from assembly {assembly.FullName}. This plugin does not provide an {nameof(IMetadataProvider)}.");
            T obj;
            var requestedType = typeof(T);
            if (requestedType == typeof(IMetadataProvider))
                obj = (T)CreateWithServiceProvider(attribute.MetadataProvider, provider ?? throw new ArgumentNullException(nameof(provider)));
            else
                throw new NotSupportedException(requestedType.FullName + " is not supported as a plugin.");
            try
            {
                return obj;
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidOperationException($"Failed to get {typeof(T).FullName} from application configurations. The loaded type is incorrect. See inner exception for details.", ex);
            }
        }
        private static object CreateWithServiceProvider(Type type, IServiceProvider serviceProvider)
        {
            foreach (var constructor in type.GetConstructors())
            {
                if (!TryGetParameters(constructor.GetParameters(), serviceProvider, out var parameters))
                    continue;
                return constructor.Invoke(parameters);
            }
            throw new InvalidOperationException($"Cannot create an instance of {type}. A suitable constructor is not found.");
        }
        private static bool TryGetParameters(ParameterInfo[] parameterDescriptions, IServiceProvider serviceProvider, out object[] parameters)
        {
            parameters = new object[parameterDescriptions.Length];
            for (int i = 0; i < parameterDescriptions.Length; i++)
            {
                parameters[i] = serviceProvider.GetService(parameterDescriptions[i].ParameterType);
                if (parameters[i] == null)
                    return false;
            }
            return true;
        }
    }
}
