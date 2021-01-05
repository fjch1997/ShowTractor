using ShowTractor.Mvvm;
using ShowTractor.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ShowTractor.Pages.Details
{
    [Flags]
    public enum SortBy
    {
        [Display(ResourceType = typeof(Resources), Name = nameof(Resources.SortBy_None))]
        None = 0,
        [Display(ResourceType = typeof(Resources), Name = nameof(Resources.SortBy_ReleaseDate))]
        ReleaseDate = 1,
        [Display(ResourceType = typeof(Resources), Name = nameof(Resources.SortBy_AToZ))]
        AToZ = 2,
        [Display(ResourceType = typeof(Resources), Name = nameof(Resources.SortBy_TvSeries))]
        TvSeries = 4,
    }
    public class LibraryViewModel : INotifyPropertyChanged
    {
        private readonly IAsyncEnumerable<PosterViewModel> seasonsAsync;
        private readonly string noResultMessage;
        private IEnumerable<PosterViewModel> seasons = Enumerable.Empty<PosterViewModel>();
        public LibraryViewModel(IAsyncEnumerable<PosterViewModel> seasons, SortBy availableSorts, SortBy defaultSort, string noResultMessage)
        {
            seasonsAsync = seasons;
            this.noResultMessage = noResultMessage;
            AvailableSorts = Enum.GetValues(typeof(SortBy)).Cast<SortBy>().Where(s => availableSorts.HasFlag(s)).Select(e => new EnumItem(e)).ToArray();
            sortByEnumItem = AvailableSorts.First(s => (SortBy)s.Value == defaultSort);
            showSorts = availableSorts != defaultSort;
            _ = LoadAsync();
        }
        public string ErrorMessage { get => errorMessage; set { errorMessage = value; OnPropertyChanged(); } }
        private string errorMessage = string.Empty;
        public bool ShowSorts { get => showSorts; set { showSorts = value; OnPropertyChanged(); } }
        private bool showSorts;

        private async Task LoadAsync()
        {
            Loading = true;
            try
            {
                var seasons = new List<PosterViewModel>();
                this.seasons = seasons;
                var i = 0;
                await foreach (var season in seasonsAsync)
                {
                    seasons.Add(season);
                    i++;
                    if (i % 10 == 0)
                        RefreshView();
                }
                if (i == 0 || i % 10 != 0)
                    RefreshView();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                Loading = false;
            }
        }

        private void RefreshView()
        {
            ErrorMessage = !seasons.Any() ? noResultMessage : string.Empty;
            switch (SortBy)
            {
                case SortBy.None:
                    View = seasons;
                    break;
                case SortBy.ReleaseDate:
                    View = seasons.OrderByDescending(s => s.FirstEpisodeAirDate).GroupBy(s => s.FirstEpisodeAirDate.Year);
                    break;
                case SortBy.AToZ:
                    View = seasons.OrderBy(s => s.ShowName).ThenBy(s => s.Season).GroupBy(s => s.ShowName.Substring(0, 1));
                    break;
                case SortBy.TvSeries:
                    View = seasons.OrderBy(s => s.ShowName).ThenBy(s => s.Season).GroupBy(s => s.ShowName);
                    break;
                default:
                    break;
            }
        }

        public IEnumerable<EnumItem> AvailableSorts { get; private set; }
        public EnumItem SortByEnumItem
        {
            get => sortByEnumItem;
            set
            {
                if (sortByEnumItem != value)
                {
                    sortByEnumItem = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SortBy));
                    OnPropertyChanged(nameof(ViewGrouped));
                    RefreshView();
                }
            }
        }
        private EnumItem sortByEnumItem;
        public SortBy SortBy
        {
            get => (SortBy)SortByEnumItem.Value;
            set
            {
                sortByEnumItem = AvailableSorts.First(s => (SortBy)s.Value == value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(SortByEnumItem));
                OnPropertyChanged(nameof(ViewGrouped));
                RefreshView();
            }
        }
        public bool Loading { get => loading; set { loading = value; OnPropertyChanged(); } }
        private bool loading;
        public object? View { get => view; set { view = value; OnPropertyChanged(); } }
        private object? view;
        public bool ViewGrouped => SortBy != SortBy.None;
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
