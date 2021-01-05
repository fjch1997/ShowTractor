using Microsoft.EntityFrameworkCore;
using ShowTractor.Extensions;
using ShowTractor.Interfaces;
using ShowTractor.Pages.Details;
using ShowTractor.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ShowTractor.Pages
{
    public class UnwatchedPageViewModel : INotifyPropertyChanged
    {
        private readonly IFactory<Database.ShowTractorDbContext> factory;
        private readonly GeneralSettings settings;

        internal UnwatchedPageViewModel(IFactory<Database.ShowTractorDbContext> factory, GeneralSettings settings)
        {
            this.factory = factory;
            this.settings = settings;
            LoadTotalTimeUnwatchedAsync().ConfigureAwait(false);
        }

        private async Task LoadTotalTimeUnwatchedAsync()
        {
            TotalTimeUnwatched = await Task.Run(async () =>
            {
                using var context = factory.Get();
                var query = from e in (IQueryable<Database.TvEpisode>)context.TvEpisodes
                            where e.WatchProgress == TimeSpan.Zero && e.FirstAirDate <= DateTime.Today && e.TvSeason.Following
                            select new { e.WatchProgress, e.Runtime };
                var total = TimeSpan.Zero;
                await foreach (var item in query.AsAsyncEnumerable())
                {
                    if (item.Runtime == TimeSpan.Zero)
                    {
                        total += settings.DefaultEpisodeLength;
                    }
                    else if (item.WatchProgress > item.Runtime)
                    {
                        total += item.Runtime;
                    }
                    else
                    {
                        total += (item.Runtime - item.WatchProgress);
                    }
                }
                return total;
            });
        }

        public int TotalUnwatched { get => totalUnwatched; set { totalUnwatched = value; OnPropertyChanged(); } }
        private int totalUnwatched;
        public TimeSpan TotalTimeUnwatched { get => totalTimeUnwatched; set { totalTimeUnwatched = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalTimeUnwatchedDisplayText)); } }
        private TimeSpan totalTimeUnwatched;
        public string TotalTimeUnwatchedDisplayText => TotalTimeUnwatched.ToDisplayText();

        public LibraryViewModel LibraryViewModel => new(GetUnwatchedAsync(), SortBy.None, SortBy.None, Resources.LibraryNoTvShows);

        private async IAsyncEnumerable<PosterViewModel> GetUnwatchedAsync()
        {
            using var context = factory.Get();
            var query = from e in (IQueryable<Database.TvEpisode>)context.TvEpisodes
                        where e.WatchProgress == TimeSpan.Zero && e.FirstAirDate <= DateTime.Today && e.TvSeason.Following
                        orderby e.FirstAirDate
                        group e by e.TvSeasonId into g
                        select new { g.Key, Count = g.Count() } into c
                        join s in context.TvSeasons on c.Key equals s.Id
                        where s.Following
                        select new LibraryPosterViewModel(s.Id, s.ShowName, s.Season, default, factory)
                        {
                            Unwatched = c.Count,
                            ShowUnwatched = true,
                        };

            await foreach (var item in await Task.Run(() => query.AsAsyncEnumerable()))
            {
                if (item.Unwatched != 0)
                    TotalUnwatched += item.Unwatched;
                yield return item;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
