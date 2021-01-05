using System;
using System.Collections.Generic;
using System.Linq;

namespace ShowTractor.Plugins.Interfaces
{
    public enum PluginSettingsDescriptionsType
    {
        Flyout, Page
    }
    public abstract class PluginSettingsDescriptions
    {
        protected PluginSettingsDescriptions(PluginSettingsDescriptionsType type) { Type = type; }
        public PluginSettingsDescriptionsType Type { get; }
    }
    public class TemplatedPluginSettingsDescriptions : PluginSettingsDescriptions
    {
        public TemplatedPluginSettingsDescriptions(PluginSettingsDescriptionsType type, object dataTemplate) : base(type)
        {
            DataTemplate = dataTemplate;
        }
        public object DataTemplate { get; set; }
    }
    public class PredefinedPluginSettingsDescriptions : PluginSettingsDescriptions
    {
        public PredefinedPluginSettingsDescriptions(PluginSettingsDescriptionsType type) : base(type) { }
        public IEnumerable<PluginSettingsDescriptionsGroup> Groups { get; init; } = Enumerable.Empty<PluginSettingsDescriptionsGroup>();
    }
    public class PluginSettingsDescriptionsGroup
    {
        public string Name { get; init; } = string.Empty;
        public string Subtitle { get; init; } = string.Empty;
        public string Tooltip { get; init; } = string.Empty;
        public IEnumerable<PluginSettingsDescription> Descriptions { get; init; } = Enumerable.Empty<PluginSettingsDescription>();
    }
    public abstract class PluginSettingsDescription
    {
        public string Name { get; init; } = string.Empty;
        public string Subtitle { get; init; } = string.Empty;
        public string ToolTip { get; init; } = string.Empty;
    }
    public abstract class PluginSettingsDescription<T> : PluginSettingsDescription
    {
        public abstract T? Get();
        public abstract void Set(T? value);
    }
    public class StringPluginSettingsDescription : PluginSettingsDescription<string>
    {
        private readonly Func<string?> get;
        private readonly Action<string?> set;

        public StringPluginSettingsDescription(Func<string?> get, Action<string?> set)
        {
            this.get = get;
            this.set = set;
        }

        public override string? Get()
        {
            return get();
        }

        public override void Set(string? value)
        {
            set(value);
        }
    }
}
