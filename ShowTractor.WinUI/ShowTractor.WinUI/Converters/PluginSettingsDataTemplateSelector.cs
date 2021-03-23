using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ShowTractor.Plugins.Interfaces;
using System;

namespace ShowTractor.WinUI.Converters
{
    public class PluginSettingsDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? StringTemplate { get; set; }
        public DataTemplate? DirectoryTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item.GetType() == typeof(StringPluginSettingsDescription))
                return StringTemplate ?? throw new InvalidOperationException($"{nameof(PluginSettingsDataTemplateSelector)}.{nameof(StringTemplate)} is not set.");
            if (item.GetType() == typeof(DirectoryPluginSettingsDescription))
                return DirectoryTemplate ?? throw new InvalidOperationException($"{nameof(PluginSettingsDataTemplateSelector)}.{nameof(DirectoryTemplate)} is not set.");
            return base.SelectTemplateCore(item);
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }
    }
}
