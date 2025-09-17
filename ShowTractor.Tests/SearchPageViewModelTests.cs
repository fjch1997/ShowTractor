using NUnit.Framework;
using NUnit.Framework.Legacy;
using ShowTractor.Interfaces;
using ShowTractor.Pages;
using ShowTractor.Pages.Details;
using ShowTractor.Plugins.Interfaces;
using ShowTractor.Tests.Mocks;
using ShowTractor.Tests.TestPlugins;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static ShowTractor.Tests.TestFixtures.ExampleSearchResults;

namespace ShowTractor.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class SearchPageViewModelTests : HttpMessageHandler
    {
        private MockArtworkService artworkService = new MockArtworkService();
        [TestCase]
        public async Task MissingMetadataProviderTestAsync()
        {
            var subject = new SearchPageViewModel(new DelegateFactory<IMetadataProvider?>(() => null))
            {
                Parameter = "search term"
            };
            await WaitForLoadingAsync(subject);
            ClassicAssert.True(!string.IsNullOrEmpty(subject.ErrorMessage) || !string.IsNullOrEmpty(subject.LibraryViewModel?.ErrorMessage));
            ClassicAssert.IsNull(subject.LibraryViewModel);
        }
        [TestCase]
        public async Task SearchErrorTestAsync()
        {
            var subject = new SearchPageViewModel(new DelegateFactory<IMetadataProvider?>(() => new TestMetadataProvider(true)))
            {
                Parameter = "search term"
            };
            await WaitForLoadingAsync(subject);
            ClassicAssert.True(!string.IsNullOrEmpty(subject.ErrorMessage) || !string.IsNullOrEmpty(subject.LibraryViewModel?.ErrorMessage));
        }
        [TestCase]
        public async Task SearchTestAsync()
        {
            var subject = new SearchPageViewModel(new DelegateFactory<IMetadataProvider?>(() => new TestMetadataProvider(false)))
            {
                Parameter = "search term"
            };
            await WaitForLoadingAsync(subject);
            ClassicAssert.That(subject.ErrorMessage, Is.Empty);
            if (subject.LibraryViewModel == null)
                throw new AssertionException(nameof(subject.LibraryViewModel));
            ClassicAssert.That(subject.LibraryViewModel.ErrorMessage, Is.Empty);
            ClassicAssert.IsNotNull(subject.LibraryViewModel);
            AssertFiltersAndSorts(subject);
            subject.LibraryViewModel.SortBy = SortBy.TvSeries;
            var all = ((IEnumerable<IGrouping<string, PosterViewModel>>?)subject.LibraryViewModel.View)?.SelectMany(g => g) ?? throw new AssertionException("");
            void AssertShow(TvSeason tvSeason)
            {
                var vm = (SearchResultPosterViewModel)all.Where(v => v.ShowName == tvSeason.ShowName && v.Season == tvSeason.Season).First();
                if (tvSeason.ArtworkUri == null)
                {
                    Assert.That(vm.Artwork, Is.Null);
                }
                else
                {
                    Assert.That(vm.Artwork?.Scheme, Is.EqualTo("https"));
                }
                ClassicAssert.AreEqual(tvSeason.ShowName, vm.Data.ShowName);
                ClassicAssert.AreEqual(tvSeason.SeasonDescription, vm.Data.SeasonDescription);
                ClassicAssert.AreEqual(tvSeason.ShowDescription, vm.Data.ShowDescription);
                ClassicAssert.AreEqual(tvSeason.Season, vm.Data.Season);
                ClassicAssert.AreEqual(tvSeason.Episodes.Count, vm.Data.Episodes.Count);
            }
            AssertShow(TestTvSeason1);
            AssertShow(TestTvSeason2);
            AssertShow(TestTvSeason3);
            AssertShow(TestTvSeason6);
            artworkService.AssertNothingSaved();
        }
        [TestCase]
        public async Task SearchFilterByShowTestAsync()
        {
            var subject = new SearchPageViewModel(new DelegateFactory<IMetadataProvider?>(() => new TestMetadataProvider(false)))
            {
                Parameter = "search term"
            };
            await WaitForLoadingAsync(subject);
            ClassicAssert.That(subject.ErrorMessage, Is.Empty);
            if (subject.LibraryViewModel == null)
                throw new AssertionException(nameof(subject.LibraryViewModel));
            ClassicAssert.That(subject.LibraryViewModel.ErrorMessage, Is.Empty);
            ClassicAssert.IsNotNull(subject.LibraryViewModel);
            AssertFiltersAndSorts(subject);
            subject.LibraryViewModel.SortBy = SortBy.TvSeries;
            var result = ((IEnumerable<IGrouping<string, PosterViewModel>>?)subject.LibraryViewModel.View)?.ToArray() ?? throw new AssertionException("");
            ClassicAssert.AreEqual(2, result.Length);
            ClassicAssert.AreEqual(TestTvSeason6.ShowName, result[0].Key);
            ClassicAssert.AreEqual(TestTvSeason1.ShowName, result[1].Key);
            ClassicAssert.AreEqual(1, result[0].Count());
            ClassicAssert.AreEqual(3, result[1].Count());
            artworkService.AssertNothingSaved();
        }
        [TestCase]
        public async Task SearchFilterByAToZTestAsync()
        {
            var subject = new SearchPageViewModel(new DelegateFactory<IMetadataProvider?>(() => new TestMetadataProvider(false)))
            {
                Parameter = "search term"
            };
            await WaitForLoadingAsync(subject);
            ClassicAssert.IsTrue(string.IsNullOrEmpty(subject.ErrorMessage));
            ClassicAssert.IsNotNull(subject.LibraryViewModel);
            AssertFiltersAndSorts(subject);
            if (subject.LibraryViewModel == null)
                throw new AssertionException(nameof(subject.LibraryViewModel));
            subject.LibraryViewModel.SortBy = SortBy.AToZ;
            var result = ((IEnumerable<IGrouping<string, PosterViewModel>>?)subject.LibraryViewModel.View)?.ToArray() ?? throw new AssertionException("");
            ClassicAssert.AreEqual(1, result.Length);
            ClassicAssert.AreEqual(TestTvSeason1.ShowName.Substring(0, 1), result[0].Key);
            ClassicAssert.AreEqual(4, result[0].Count());
            artworkService.AssertNothingSaved();
        }
        private static void AssertFiltersAndSorts(SearchPageViewModel subject)
        {
            if (subject.LibraryViewModel == null)
                throw new AssertionException(nameof(subject.LibraryViewModel));
            ClassicAssert.AreEqual(3, subject.LibraryViewModel.AvailableSorts.Count());
        }
        private async Task WaitForLoadingAsync(SearchPageViewModel subject)
        {
            while (subject.Loading)
            {
                var tcs = new TaskCompletionSource<bool>();
                void handler(object? s, System.ComponentModel.PropertyChangedEventArgs e) => tcs.TrySetResult(false);
                subject.PropertyChanged += handler;
                await tcs.Task;
                subject.PropertyChanged -= handler;
            }
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new AssertionException("");
        }
    }
}
