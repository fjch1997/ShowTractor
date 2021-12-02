using ShowTractor.Interfaces;
using ShowTractor.Plugins.Interfaces;
using System;
using System.Linq;

namespace ShowTractor.Plugins
{
    class MetadataProviderFactory : IFactory<IMetadataProvider?>
    {
        private readonly PluginSettings settings;
        private readonly IServiceProvider serviceProvider;

        public MetadataProviderFactory(PluginSettings settings, IServiceProvider serviceProvider)
        {
            this.settings = settings;
            this.serviceProvider = serviceProvider;
        }

        public IMetadataProvider? Get()
        {
            foreach (var definition in settings.MetadataProviders.Where(p => p.Enabled).ToArray())
            {
                try
                {
                    return definition.Load<IMetadataProvider>(serviceProvider);
                }
                catch (Exception)
                {
                    settings.MetadataProviders.Remove(definition);
                }
            }
            return null;
        }
    }
}
