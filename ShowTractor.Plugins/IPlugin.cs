using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ShowTractor.Plugins.Interfaces
{
    public class AdditionalAttributes : Dictionary<AssemblyName, IReadOnlyDictionary<string, string>>
    {
        public static readonly Dictionary<AssemblyName, IReadOnlyDictionary<string, string>> Empty = new AdditionalAttributes();
    }
    public interface IPlugin
    {
        string Name { get; }
        PluginSettingsDescriptions? PluginSettingsDescriptions { get; }
        Stream GetIconStream();
        protected static Stream GetDefaultIconStream() => Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(ShowTractor)}.{nameof(Plugins)}.{nameof(Interfaces)}.Assets.placeholder.jpg");
    }
}
