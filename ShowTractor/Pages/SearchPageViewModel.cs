using ShowTractor.Interfaces;
using ShowTractor.Mvvm;
using ShowTractor.Pages.Details;
using ShowTractor.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ShowTractor.Pages
{
    public class SearchPageViewModel : ISupportNavigation, INotifyPropertyChanged
    {
        private readonly IFactory<IMetadataProvider?> metadataProviderFactory;
        private readonly HttpClient httpClient;
        private CancellationTokenSource? cts;
        private string? parameter;

        internal SearchPageViewModel(IFactory<IMetadataProvider?> metadataProviderFactory, HttpClient httpClient)
        {
            this.metadataProviderFactory = metadataProviderFactory;
            this.httpClient = httpClient;
        }

        public object? Parameter { get => parameter; set { SetParameter(value); OnPropertyChanged(); OnPropertyChanged(nameof(Title)); } }

        public string Title => "Results for: " + parameter;

        public bool Loading { get => loading || (libraryViewModel?.Loading == true); set { loading = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowResults)); } }
        private bool loading;

        public string ErrorMessage { get => errorMessage; set { errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowResults)); } }
        private string errorMessage = string.Empty;

        public bool ShowResults => !Loading && ErrorMessage == string.Empty;

        public LibraryViewModel? LibraryViewModel { get => libraryViewModel; set { libraryViewModel = value; OnPropertyChanged(); } }
        private LibraryViewModel? libraryViewModel;

        private void SetParameter(object? value)
        {
            parameter = (string?)value;
            OnPropertyChanged(nameof(Parameter));
            if (value != null)
            {
                LoadAsync((string)value).ConfigureAwait(false);
            }
        }

        private Task LoadAsync(string value)
        {
            Loading = true;
            cts = new CancellationTokenSource();
            try
            {
                var provider = metadataProviderFactory.Get();
                if (provider == null)
                {
                    ErrorMessage = "You must have a Metadata Provider plugin to search.";
                }
                else
                {
                    LibraryViewModel = new LibraryViewModel(
                        SearchAsync(value, provider, cts.Token),
                        SortBy.AToZ | SortBy.TvSeries,
                        SortBy.None,
                        "No result found"
                    );
                    LibraryViewModel.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == nameof(LibraryViewModel.Loading))
                            OnPropertyChanged(nameof(Loading));
                    };
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Search Failed. {ex.Message}";
            }
            finally
            {
                cts.Dispose();
                cts = null;
                Loading = false;
            }
            return Task.FromResult(false);
        }

        private async IAsyncEnumerable<SearchResultPosterViewModel> SearchAsync(string value, IMetadataProvider provider, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var item in await Task.Run(() => provider.SearchAsync(value, token)))
            {
                yield return new SearchResultPosterViewModel(item, httpClient);
            }
        }

        bool ISupportNavigation.OnNavigatingFrom()
        {
            cts?.Cancel();
            return false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
