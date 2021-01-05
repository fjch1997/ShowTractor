using Microsoft.EntityFrameworkCore;
using ShowTractor.Database.Extensions;
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
    public class MyShowsPageViewModel : INotifyPropertyChanged
    {
        private const SortBy AllSorts = SortBy.AToZ | SortBy.ReleaseDate | SortBy.TvSeries;
        private readonly IFactory<Database.ShowTractorDbContext> factory;

        internal MyShowsPageViewModel(IFactory<Database.ShowTractorDbContext> factory)
        {
            this.factory = factory;
        }

        public LibraryViewModel CurrentShows => new(GetCurrentShowsAsync(), AllSorts, SortBy.None, Resources.LibraryNoCurrentShows);
        public LibraryViewModel AllFollowed => new(GetShowsByIsFollowingAsync(true), AllSorts, SortBy.None, Resources.LibraryNoTvShows);
        public LibraryViewModel EndedShows => new(GetShowsByIsShowEndedAsync(true), AllSorts, SortBy.None, Resources.LibraryNoEndedShows);
        public LibraryViewModel Unfollowed => new(GetShowsByIsFollowingAsync(false), AllSorts, SortBy.None, Resources.LibraryNoTvShows);
        private async IAsyncEnumerable<LibraryPosterViewModel> GetShowsByIsFollowingAsync(bool isFollowing)
        {
            using var context = factory.Get();
            var data = await Task.Run(async () => await ((IQueryable<Database.TvSeason>)context.TvSeasons)
                .Where(s => s.Following == isFollowing)
                .SelectNoArtwork()
                .ToArrayAsync());
            foreach (var s in data)
            {
                yield return await GetPosterViewModelAsync(context, s);
            }
        }
        private async IAsyncEnumerable<LibraryPosterViewModel> GetCurrentShowsAsync()
        {
            using var context = factory.Get();
            var today = DateTime.Today;
            var data = await Task.Run(async () => await ((IQueryable<Database.TvSeason>)context.TvSeasons)
            .Where(s => s.Episodes.Any(e => e.FirstAirDate < today.AddDays(7) && e.FirstAirDate > today.AddDays(-7) && s.Following))
            .Select(s =>
                new LibraryPosterViewModel(
                    s.Id,
                    s.ShowName,
                    s.Season,
                    s.Episodes.Select(s => s.FirstAirDate).OrderBy(s => s).FirstOrDefault(),
                    factory))
            .AsAsyncEnumerable()
            .ToArrayAsync());
            foreach (var s in data)
            {
                yield return s;
            }
        }
        private async IAsyncEnumerable<LibraryPosterViewModel> GetShowsByIsShowEndedAsync(bool isShowEnded)
        {
            using var context = factory.Get();
            var data = await Task.Run(async () => await ((IQueryable<Database.TvSeason>)context.TvSeasons)
            .Where(s => s.ShowEnded == isShowEnded && s.Following)
            .SelectNoArtwork()
            .ToArrayAsync());
            foreach (var s in data)
            {
                yield return await GetPosterViewModelAsync(context, s);
            }
        }
        private async Task<LibraryPosterViewModel> GetPosterViewModelAsync(Database.ShowTractorDbContext context, Database.TvSeason season)
        {
            var firstAirDate = await Task.Run(async () => await ((IQueryable<Database.TvEpisode>)context.TvEpisodes).Where(e => e.TvSeasonId == season.Id).Select(s => s.FirstAirDate).OrderBy(s => s).FirstOrDefaultAsync());
            return new LibraryPosterViewModel(season.Id, season.ShowName, season.Season, firstAirDate, factory);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
