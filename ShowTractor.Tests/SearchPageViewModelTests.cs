using NUnit.Framework;
using ShowTractor.Interfaces;
using ShowTractor.Pages;
using ShowTractor.Pages.Details;
using ShowTractor.Plugins.Interfaces;
using ShowTractor.Tests.TestPlugins;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static ShowTractor.Tests.TestFixtures.ExampleSearchResults;

namespace ShowTractor.UnitTests
{
    [TestFixture]
    public class SearchPageViewModelTests : HttpMessageHandler
    {
        [TestCase]
        public async Task MissingMetadataProviderTestAsync()
        {
            var subject = new SearchPageViewModel(new DelegateFactory<IMetadataProvider?>(() => null), new HttpClient(this))
            {
                Parameter = "search term"
            };
            await WaitForLoadingAsync(subject);
            Assert.True(!string.IsNullOrEmpty(subject.ErrorMessage) || !string.IsNullOrEmpty(subject.LibraryViewModel?.ErrorMessage));
            Assert.IsNull(subject.LibraryViewModel);
        }
        [TestCase]
        public async Task SearchErrorTestAsync()
        {
            var subject = new SearchPageViewModel(new DelegateFactory<IMetadataProvider?>(() => new TestMetadataProvider(true)), new HttpClient(this))
            {
                Parameter = "search term"
            };
            await WaitForLoadingAsync(subject);
            Assert.True(!string.IsNullOrEmpty(subject.ErrorMessage) || !string.IsNullOrEmpty(subject.LibraryViewModel?.ErrorMessage));
        }
        [TestCase]
        public async Task SearchTestAsync()
        {
            var subject = new SearchPageViewModel(new DelegateFactory<IMetadataProvider?>(() => new TestMetadataProvider(false)), new HttpClient(this))
            {
                Parameter = "search term"
            };
            await WaitForLoadingAsync(subject);
            Assert.That(subject.ErrorMessage, Is.Empty);
            if (subject.LibraryViewModel == null)
                throw new AssertionException(nameof(subject.LibraryViewModel));
            Assert.That(subject.LibraryViewModel.ErrorMessage, Is.Empty);
            Assert.IsNotNull(subject.LibraryViewModel);
            AssertFiltersAndSorts(subject);
            subject.LibraryViewModel.SortBy = SortBy.TvSeries;
            var all = ((IEnumerable<IGrouping<string, PosterViewModel>>?)subject.LibraryViewModel.View)?.SelectMany(g => g) ?? throw new AssertionException("");
            void AssertShow(TvSeason tvSeason)
            {
                var vm = all.Where(v => v.ShowName == tvSeason.ShowName && v.Season == tvSeason.Season).First();
                Assert.AreEqual(tvSeason.ShowName, ((SearchResultPosterViewModel)vm).Data.ShowName);
                Assert.AreEqual(tvSeason.SeasonDescription, ((SearchResultPosterViewModel)vm).Data.SeasonDescription);
                Assert.AreEqual(tvSeason.ShowDescription, ((SearchResultPosterViewModel)vm).Data.ShowDescription);
                Assert.AreEqual(tvSeason.Season, ((SearchResultPosterViewModel)vm).Data.Season);
                Assert.AreEqual(tvSeason.Episodes.Count, ((SearchResultPosterViewModel)vm).Data.Episodes.Count);
            }
            AssertShow(TestTvSeason1);
            AssertShow(TestTvSeason2);
            AssertShow(TestTvSeason3);
            AssertShow(TestTvSeason6);
        }
        [TestCase]
        public async Task SearchFilterByShowTestAsync()
        {
            var subject = new SearchPageViewModel(new DelegateFactory<IMetadataProvider?>(() => new TestMetadataProvider(false)), new HttpClient(this))
            {
                Parameter = "search term"
            };
            await WaitForLoadingAsync(subject);
            Assert.That(subject.ErrorMessage, Is.Empty);
            if (subject.LibraryViewModel == null)
                throw new AssertionException(nameof(subject.LibraryViewModel));
            Assert.That(subject.LibraryViewModel.ErrorMessage, Is.Empty);
            Assert.IsNotNull(subject.LibraryViewModel);
            AssertFiltersAndSorts(subject);
            subject.LibraryViewModel.SortBy = SortBy.TvSeries;
            var result = ((IEnumerable<IGrouping<string, PosterViewModel>>?)subject.LibraryViewModel.View)?.ToArray() ?? throw new AssertionException("");
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(TestTvSeason6.ShowName, result[0].Key);
            Assert.AreEqual(TestTvSeason1.ShowName, result[1].Key);
            Assert.AreEqual(1, result[0].Count());
            Assert.AreEqual(3, result[1].Count());
        }
        [TestCase]
        public async Task SearchFilterByAToZTestAsync()
        {
            var subject = new SearchPageViewModel(new DelegateFactory<IMetadataProvider?>(() => new TestMetadataProvider(false)), new HttpClient(this))
            {
                Parameter = "search term"
            };
            await WaitForLoadingAsync(subject);
            Assert.IsTrue(string.IsNullOrEmpty(subject.ErrorMessage));
            Assert.IsNotNull(subject.LibraryViewModel);
            AssertFiltersAndSorts(subject);
            if (subject.LibraryViewModel == null)
                throw new AssertionException(nameof(subject.LibraryViewModel));
            subject.LibraryViewModel.SortBy = SortBy.AToZ;
            var result = ((IEnumerable<IGrouping<string, PosterViewModel>>?)subject.LibraryViewModel.View)?.ToArray() ?? throw new AssertionException("");
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(TestTvSeason1.ShowName.Substring(0, 1), result[0].Key);
            Assert.AreEqual(4, result[0].Count());
        }
        private static void AssertFiltersAndSorts(SearchPageViewModel subject)
        {
            if (subject.LibraryViewModel == null)
                throw new AssertionException(nameof(subject.LibraryViewModel));
            Assert.AreEqual(3, subject.LibraryViewModel.AvailableSorts.Count());
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
