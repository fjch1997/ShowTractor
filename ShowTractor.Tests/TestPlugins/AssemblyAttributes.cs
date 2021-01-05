using ShowTractor.Plugins.Interfaces;
using ShowTractor.Tests.TestPlugins;

[assembly: ShowTractorPluginAssembly(MetadataProvider = typeof(TestMetadataProvider))]