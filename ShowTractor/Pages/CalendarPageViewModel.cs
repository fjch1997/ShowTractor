using ShowTractor.Interfaces;
using ShowTractor.Pages.Details;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ShowTractor.Pages
{
    public class CalendarPageViewModel : INotifyPropertyChanged
    {
        private readonly IFactory<Database.ShowTractorDbContext> factory;
        private readonly IAsyncInitializationService asyncInitializationService;
        private readonly GeneralSettings settings;
        private readonly Dictionary<(int year, int month), Task> tasks = new();
        private readonly Dictionary<DateTimeOffset, CalendarDayViewModel> days = new();
        private readonly Task<IDictionary<DateTime, IEnumerable<CalendarPosterViewModel>>> loadDataTask;

        internal CalendarPageViewModel(IFactory<Database.ShowTractorDbContext> factory, IAsyncInitializationService asyncInitializationService, GeneralSettings settings)
        {
            this.factory = factory;
            this.asyncInitializationService = asyncInitializationService;
            this.settings = settings;
            loadDataTask = LoadDataAsync();
        }

        public Func<DateTimeOffset, object> CalendarDayItemDataContextProvider => date =>
        {
            date = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);
            if (days.TryGetValue(date, out var day))
                return day;
            day = new CalendarDayViewModel();
            days[date] = day;
            var month = (date.Year, date.Month);
            if (!tasks.TryGetValue(month, out _))
            {
                tasks[month] = LoadMonthAsync(month);
            }
            return day;
        };

        public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }

        private async Task<IDictionary<DateTime, IEnumerable<CalendarPosterViewModel>>> LoadDataAsync()
        {
            await asyncInitializationService.Task;
            using var context = factory.Get();
            var query = from e in (IQueryable<Database.TvEpisode>)context.TvEpisodes
                        join s in context.TvSeasons on e.TvSeasonId equals s.Id
                        where s.Following == true
                        select new CalendarPosterViewModel(e.TvSeasonId, s.ShowName, s.Season, e.EpisodeNumber, e.Name, e.FirstAirDate, TvEpisodeViewModel.GetWatchPercentage(e.Runtime, e.WatchProgress) > 0, factory, settings)
                        {
                            FirstEpisodeAirDate = e.FirstAirDate,
                        };
            return await Task.Run(() => query.AsEnumerable().GroupBy(e => DateTime.SpecifyKind(e.FirstEpisodeAirDate, DateTimeKind.Utc)).ToDictionary(g => g.Key, g => g.AsEnumerable()));
        }

        private async Task LoadMonthAsync((int year, int month) month)
        {
            var data = await loadDataTask;
            for (var i = new DateTimeOffset(month.year, month.month, 1, 0, 0, 0, 0, TimeSpan.Zero); i.Month == month.month; i = i.AddDays(1))
            {
                if (!days.TryGetValue(i, out var dayVm))
                {
                    dayVm = new CalendarDayViewModel();
                    days[i] = dayVm;
                }
                if (data.TryGetValue(i.DateTime, out var episodes))
                    dayVm.TvEpisodes = episodes;
                else
                    dayVm.TvEpisodes = Enumerable.Empty<CalendarPosterViewModel>();
            }
        }
    }

    public class CalendarDayViewModel : INotifyPropertyChanged
    {
        public IEnumerable<CalendarPosterViewModel>? TvEpisodes { get => tvEpisodes; set { tvEpisodes = value; OnPropertyChanged(); } }
        private IEnumerable<CalendarPosterViewModel>? tvEpisodes;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
