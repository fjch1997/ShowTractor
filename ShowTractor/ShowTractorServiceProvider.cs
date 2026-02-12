using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShowTractor.Background;
using ShowTractor.Interfaces;
using ShowTractor.Pages;
using ShowTractor.Pages.Details;
using ShowTractor.Pages.Settings;
using ShowTractor.Plugins;
using ShowTractor.Plugins.Interfaces;
using System;
using System.Net.Http;

namespace ShowTractor
{
    public class ShowTractorServiceProvider : IServiceProvider
    {
        private readonly ServiceProvider provider;

        public ShowTractorServiceProvider(IOpenFileDialogService openFileDialogService)
        {
            var services = new ServiceCollection();
            var httpClient = new HttpClient();
            services.AddSingleton(httpClient);
            services.AddSingleton(openFileDialogService);
            services.AddSingleton(PluginSettings.Default);
            services.AddSingleton(GeneralSettings.Default);
            services.AddSingleton<IFactory<IMetadataProvider?>>(p => new MetadataProviderFactory(p.GetRequiredService<PluginSettings>(), p));
            services.AddSingleton<AggregateMediaSourceProvider>();
            services.AddDbContextFactory<Database.ShowTractorDbContext>();
            ConfigureViewModels(services);
            services.AddSingleton<IArtworkService, ArtworkService>();
            services.AddSingleton<IAsyncInitializationService>(p => new AsyncInitializationService(p.GetRequiredService<Database.ShowTractorDbContext>(), p.GetRequiredService<IArtworkService>()));
            ConfigureBackgroundWorker(services);
            provider = services.BuildServiceProvider();
        }
        private static void ConfigureViewModels(ServiceCollection services)
        {
            services.AddTransient(p => new SearchPageViewModel(
                p.GetRequiredService<IFactory<IMetadataProvider?>>()));
            services.AddSingleton(p => new PluginSettingsPageViewModel(PluginSettings.Default, p.GetRequiredService<IOpenFileDialogService>(), p));
            services.AddSingleton(p => new GeneralSettingsPageViewModel(GeneralSettings.Default, p.GetRequiredService<IOpenFileDialogService>()));
            services.AddScoped(p => new TvSeasonPageViewModel(
                p.GetRequiredService<IFactory<IMetadataProvider>>(),
                p.GetRequiredService<AggregateMediaSourceProvider>(),
                p.GetRequiredService<IDbContextFactory<Database.ShowTractorDbContext>>(),
                p.GetRequiredService<IArtworkService>()));
            services.AddScoped(p => new MyShowsPageViewModel(
                p.GetRequiredService<IDbContextFactory<Database.ShowTractorDbContext>>(),
                p.GetRequiredService<IArtworkService>()));
            services.AddScoped(
                p => new CalendarPageViewModel(
                    p.GetRequiredService<IDbContextFactory<Database.ShowTractorDbContext>>(),
                    p.GetRequiredService<IAsyncInitializationService>(),
                    p.GetRequiredService<GeneralSettings>()));
            services.AddScoped(
                p => new UnwatchedPageViewModel(
                    p.GetRequiredService<IDbContextFactory<Database.ShowTractorDbContext>>(),
                    p.GetRequiredService<GeneralSettings>(),
                    p.GetRequiredService<IArtworkService>()));
            services.AddScoped(
                p => new AllTimeStatisticsViewModel(
                    p.GetRequiredService<Database.ShowTractorDbContext>(),
                    p.GetRequiredService<GeneralSettings>()));
        }
        private static void ConfigureBackgroundWorker(ServiceCollection services)
        {
            services.AddSingleton(p => new MetadataUpdateBackgroundWork(
                            GeneralSettings.Default,
                            p.GetRequiredService<IDbContextFactory<Database.ShowTractorDbContext>>(),
                            p.GetRequiredService<IFactory<IMetadataProvider?>>()));
            services.AddSingleton(p =>
                new ShowTractorBackgroundWorker(new BackgroundWorkCollection(
                    new IBackgroundWork[]
                    {
                        p.GetRequiredService<MetadataUpdateBackgroundWork>()
                    })));
        }
        public object? GetService(Type serviceType) => provider.GetService(serviceType);
    }
}
