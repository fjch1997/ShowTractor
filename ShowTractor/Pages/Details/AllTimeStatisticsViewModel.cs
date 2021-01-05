using Microsoft.EntityFrameworkCore;
using ShowTractor.Extensions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ShowTractor.Pages.Details
{
    public class AllTimeStatisticsViewModel : INotifyPropertyChanged
    {
        internal AllTimeStatisticsViewModel(Database.ShowTractorDbContext context, GeneralSettings settings)
        {
            LoadAsync(context, settings).ConfigureAwait(false);
        }

        private async Task LoadAsync(Database.ShowTractorDbContext context, GeneralSettings settings)
        {
            TotalFollowing = await Task.Run(async () => await ((IQueryable<Database.TvEpisode>)context.TvEpisodes).Where(e => e.TvSeason.Following).CountAsync());
            TotalWatched = await Task.Run(async () => await ((IQueryable<Database.TvEpisode>)context.TvEpisodes).Where(e => e.TvSeason.Following && e.WatchProgress != TimeSpan.Zero).CountAsync());
            TotalTimeWatched = await Task.Run(async () =>
            {
                var query = from e in (IQueryable<Database.TvEpisode>)context.TvEpisodes
                            where e.TvSeason.Following
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

        public int TotalWatched { get => totalWatched; set { totalWatched = value; OnPropertyChanged(); } }
        private int totalWatched;
        public int TotalFollowing { get => totalFollowing; set { totalFollowing = value; OnPropertyChanged(); } }
        private int totalFollowing;
        public TimeSpan TotalTimeWatched { get => totalTimeWatched; set { totalTimeWatched = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalTimeWatched)); } }
        private TimeSpan totalTimeWatched;
        public string TotalTimeWatchedDisplayText => TotalTimeWatched.ToDisplayText();
        public int PercentageWatched => TotalFollowing == 0 ? 0 : (100 * TotalWatched / TotalFollowing);
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
