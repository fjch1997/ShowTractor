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
        private readonly Func<T?> get;
        private readonly Action<T?> set;
        protected PluginSettingsDescription(Func<T?> get, Action<T?> set)
        {
            this.get = get;
            this.set = set;
        }
        public T? Value { get => get(); set => set(value); }
        public virtual T? Get()
        {
            if (get == null) throw new ArgumentNullException(nameof(get));
            return get();
        }
        public virtual void Set(T? value)
        {
            if (set == null) throw new ArgumentNullException(nameof(set));
            set(value);
        }
    }
    public class StringPluginSettingsDescription : PluginSettingsDescription<string>
    {
        public StringPluginSettingsDescription(Func<string?> get, Action<string?> set) : base(get, set) { }
    }
    public class MultiLineStringPluginSettingsDescription : PluginSettingsDescription<string>
    {
        public MultiLineStringPluginSettingsDescription(Func<string?> get, Action<string?> set) : base(get, set) { }
    }
    public class DirectoryPluginSettingsDescription : PluginSettingsDescription<string>
    {
        public DirectoryPluginSettingsDescription(Func<string?> get, Action<string?> set) : base(get, set) { }
    }
}
