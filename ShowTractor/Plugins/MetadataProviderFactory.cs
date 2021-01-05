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
            var definition = settings.MetadataProviders.Where(p => p.Enabled).FirstOrDefault();
            if (definition == null)
                return null;
            return definition.Load<IMetadataProvider>(serviceProvider);
        }
    }
}
