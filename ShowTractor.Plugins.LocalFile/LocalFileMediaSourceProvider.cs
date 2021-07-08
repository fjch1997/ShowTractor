using ShowTractor.Plugins.Interfaces;
using ShowTractor.Plugins.LocalFile;
using ShowTractor.Plugins.LocalFile.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

[assembly: ShowTractorPluginAssembly(MediaSourceProvider = typeof(LocalFileMediaSourceProvider))]
namespace ShowTractor.Plugins.LocalFile
{

    public class LocalFileMediaSourceProvider : IMediaSourceProvider
    {
        private readonly Regex seasonEpisodeNumberRegex = new Regex("S(\\d\\d)E(\\d\\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static ThrottledAction saveAction = new ThrottledAction(() => Settings.Default.Save(), TimeSpan.FromSeconds(10));

        public LocalFileMediaSourceProvider()
        {
            Settings.Default.PropertyChanged -= Default_PropertyChanged;
            Settings.Default.PropertyChanged += Default_PropertyChanged;
        }

        private static void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            saveAction.InvokeAction();
        }

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
                        Name = Resources.BuiltInMatchingLogics,
                        Subtitle = Resources.BuiltInMatchingLogicsSubtitle,
                        Descriptions = new[]
                        {
                            new BooleanPluginSettingsDescription(() => Settings.Default.EnableBuiltInMatchingLogic, s => Settings.Default.EnableBuiltInMatchingLogic = s)
                            {
                                Name = Resources.UseBuiltInMatchingLogics,
                            },
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

        public async IAsyncEnumerable<MediaSource> GetAsync(TvSeason tvSeason, TvEpisode tvEpisode)
        {
            await foreach (var item in BuiltInMatchAsync(tvSeason, tvEpisode, Settings.Default.SearchLocation))
            {
                yield return item;
            }
            await foreach (var item in CustomMatchAsync(tvSeason, tvEpisode))
            {
                yield return item;
            }
        }

        private async IAsyncEnumerable<MediaSource> BuiltInMatchAsync(TvSeason tvSeason, TvEpisode tvEpisode, string path)
        {
            if (path == null || !Directory.Exists(path))
                yield break;
            foreach (var directory in Directory.EnumerateDirectories(path))
            {
                await foreach (var item in BuiltInMatchAsync(tvSeason, tvEpisode, directory))
                {
                    yield return item;
                }
            }
            foreach (var file in Directory.EnumerateFiles(path))
            {
                var fileInfo = new FileInfo(file);
                var filename = fileInfo.Name;
                if (!tvSeason.ShowName.Split(' ', ':', '-', '.', '(', ')').All(t => filename.Contains(t, StringComparison.InvariantCultureIgnoreCase)))
                    continue;
                var matches = seasonEpisodeNumberRegex.Match(filename);
                if (!matches.Success || matches.Groups.Count != 3)
                    continue;
                if (!int.TryParse(matches.Groups[1].Value, out var seasonNumber) || !int.TryParse(matches.Groups[2].Value, out var episodeNumber))
                    continue;
                if (tvSeason.Season == seasonNumber && tvEpisode.EpisodeNumber == episodeNumber)
                {
                    yield return new GenericMediaSource<string>(PredefinedMediaSources.LocalFile, file, GetResolution(filename), GetCodec(filename), filename, fileInfo.Length);
                }
            }

            static MediaResolution GetResolution(string filename)
            {
                if (filename.Contains("720p", StringComparison.InvariantCultureIgnoreCase))
                    return MediaResolution.SevenTwentyP;
                if (filename.Contains("1080p", StringComparison.InvariantCultureIgnoreCase))
                    return MediaResolution.TenEightyP;
                if (filename.Contains("720", StringComparison.InvariantCultureIgnoreCase))
                    return MediaResolution.SevenTwentyP;
                if (filename.Contains("1080", StringComparison.InvariantCultureIgnoreCase))
                    return MediaResolution.TenEightyP;
                if (filename.Contains("4k", StringComparison.InvariantCultureIgnoreCase))
                    return MediaResolution.FourK;
                if (filename.Contains("UHD", StringComparison.InvariantCultureIgnoreCase))
                    return MediaResolution.FourK;
                return MediaResolution.SD;
            }

            static MediaCodec GetCodec(string filename)
            {
                if (filename.Contains("xvid", StringComparison.InvariantCultureIgnoreCase))
                    return MediaCodec.Xvid;

                if (filename.Contains("hevc", StringComparison.InvariantCultureIgnoreCase))
                    return MediaCodec.HEVC;

                if (filename.Contains("h265", StringComparison.InvariantCultureIgnoreCase))
                    return MediaCodec.HEVC;
                if (filename.Contains("h264", StringComparison.InvariantCultureIgnoreCase))
                    return MediaCodec.H254;

                if (filename.Contains("x265", StringComparison.InvariantCultureIgnoreCase))
                    return MediaCodec.HEVC;
                if (filename.Contains("x264", StringComparison.InvariantCultureIgnoreCase))
                    return MediaCodec.H254;

                if (filename.Contains("avc", StringComparison.InvariantCultureIgnoreCase))
                    return MediaCodec.H254;

                if (filename.Contains("265", StringComparison.InvariantCultureIgnoreCase))
                    return MediaCodec.HEVC;
                if (filename.Contains("264", StringComparison.InvariantCultureIgnoreCase))
                    return MediaCodec.H254;

                return MediaCodec.Unknown;
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async IAsyncEnumerable<MediaSource> CustomMatchAsync(TvSeason tvSeason, TvEpisode tvEpisode)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            yield break;
        }

        public Stream GetIconStream() => Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(ShowTractor)}.{nameof(Plugins)}.{nameof(LocalFile)}.logo.png");
    }
}
