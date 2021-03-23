using ShowTractor.Plugins;
using ShowTractor.Plugins.Interfaces;
using ShowTractor.Plugins.LocalFile.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

[assembly: ShowTractorPluginAssembly(MediaSourceProvider = typeof(LocalFileMediaSourceProvider))]

namespace ShowTractor.Plugins
{
    public class LocalFileMediaSourceProvider : IMediaSourceProvider
    {
        public string Name => "Local File";

        public PluginSettingsDescriptions? PluginSettingsDescriptions =>
            new PredefinedPluginSettingsDescriptions(PluginSettingsDescriptionsType.Flyout)
            {
                Groups = new PluginSettingsDescriptionsGroup[]
                {
                    new PluginSettingsDescriptionsGroup
                    {
                        Name = Resources.SearchLocationName,
                        Subtitle = Resources.SearchLocationSubtitle,
                        Descriptions = new []
                        {
                            new DirectoryPluginSettingsDescription(() => Settings.Default.SearchLocation, s => Settings.Default.SearchLocation = s)
                        },
                    },
                    new PluginSettingsDescriptionsGroup
                    {
                        Name = Resources.SearchPatternName,
                        Subtitle = Resources.SeasonPatternSubtitle,
                        Descriptions = new []
                        {
                            new StringPluginSettingsDescription(() => Settings.Default.ShowNameSearchPattern, s => Settings.Default.ShowNameSearchPattern = s)
                            {
                                Name = Resources.ShowNamePatternName,
                                Subtitle = Resources.ShowNamePatternSubtitle,
                            },
                            new StringPluginSettingsDescription(() => Settings.Default.SeasonSearchPattern, s => Settings.Default.SeasonSearchPattern = s)
                            {
                                Name = Resources.SeasonPatternName,
                                Subtitle = Resources.SeasonPatternSubtitle,
                            },
                            new StringPluginSettingsDescription(() => Settings.Default.EpisodeNumberSearchPattern, s => Settings.Default.EpisodeNumberSearchPattern = s)
                            {
                                Name = Resources.EpisodeNumberPatternName,
                                Subtitle = Resources.EpisodeNumberPatternSubtitle,
                            },
                            new StringPluginSettingsDescription(() => Settings.Default.ShowNameReplacementCharactors, s => Settings.Default.ShowNameReplacementCharactors = s)
                            {
                                Name = Resources.ShowNameReplacementCharactorsName,
                                Subtitle = Resources.ShowNameReplacementCharactorsSubtitle,
                            }
                        },
                    }
                }
            };

        public IAsyncEnumerable<MediaSource> GetAsync(TvSeason tvSeason, TvEpisode tvEpisode)
        {
            throw new NotImplementedException();
        }

        public Stream GetIconStream() => Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(ShowTractor)}.{nameof(Plugins)}.{nameof(LocalFile)}.logo.png");
    }
}
