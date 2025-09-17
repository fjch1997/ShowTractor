using ShowTractor.Interfaces;
using ShowTractor.Pages.Details;
using ShowTractor.Plugins;
using ShowTractor.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace ShowTractor.Pages.Settings
{
    public abstract class PluginViewModel : INotifyPropertyChanged
    {
        protected PluginViewModel(PluginDefinition definition, IPlugin metadataProvider)
        {
            Definition = definition;
            Icon = new DelegateFactory<Stream>(() => metadataProvider.GetIconStream());
            Name = metadataProvider.Name;
            Enabled = definition.Enabled;
            Plugin = metadataProvider;
        }
        public bool Enabled { get => Definition.Enabled; set { Definition.Enabled = value; OnPropertyChanged(); } }
        public IFactory<Stream> Icon { get; set; }
        public string Name { get; set; }
        public abstract IEnumerable<string> Descriptions { get; }
        public IPlugin Plugin { get; private set; }
        internal PluginDefinition Definition { get; }
        protected string GetVersionDescription() => "Version: " + Plugin.GetType()?.Assembly?.GetName()?.Version?.ToString() ?? "0.0.0.0";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class MetadataProviderPluginViewModel : PluginViewModel
    {
        internal MetadataProviderPluginViewModel(PluginDefinition definition, IServiceProvider serviceProvider)
            : base(definition, definition.Load<IMetadataProvider>(serviceProvider)) { }
        public override IEnumerable<string> Descriptions => new string[] { GetVersionDescription() };
    }
}
