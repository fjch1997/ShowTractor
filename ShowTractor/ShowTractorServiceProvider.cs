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
            services.AddSingleton(new HttpClient());
            services.AddSingleton(openFileDialogService);
            services.AddSingleton(PluginSettings.Default);
            services.AddSingleton(GeneralSettings.Default);
            services.AddSingleton<IFactory<IMetadataProvider?>>(p => new MetadataProviderFactory(p.GetRequiredService<PluginSettings>(), p));
            services.AddSingleton<IAggregateMediaSourceProvider>(p => new AggregateMediaSourceProvider(p.GetRequiredService<PluginSettings>(), p));
            services.AddSingleton<IAggregateMediaPlayer>(p => new AggregateMediaPlayer(p.GetRequiredService<PluginSettings>(), p));
            services.AddSingleton<IFactory<Database.ShowTractorDbContext>>(p => new DelegateFactory<Database.ShowTractorDbContext>(() => new Database.ShowTractorDbContext()));
            services.AddDbContext<Database.ShowTractorDbContext>();
            ConfigureViewModels(services);
            services.AddSingleton<IAsyncInitializationService>(new AsyncInitializationService(new Database.ShowTractorDbContext()));
            ConfigureBackgroundWorker(services);
            provider = services.BuildServiceProvider();
        }

        private static void ConfigureViewModels(ServiceCollection services)
        {
            services.AddTransient(p => new SearchPageViewModel(p.GetRequiredService<IFactory<IMetadataProvider?>>(), p.GetRequiredService<HttpClient>()));
            services.AddSingleton(p => new PluginSettingsPageViewModel(PluginSettings.Default, p.GetRequiredService<IOpenFileDialogService>(), p));
            services.AddSingleton(new GeneralSettingsPageViewModel(GeneralSettings.Default));
            services.AddScoped(p => new TvSeasonPageViewModel(
                p.GetRequiredService<IFactory<IMetadataProvider>>(),
                p.GetRequiredService<HttpClient>(),
                p.GetRequiredService<IFactory<Database.ShowTractorDbContext>>(),
                p.GetRequiredService<IAggregateMediaSourceProvider>(),
                p.GetRequiredService<IAggregateMediaPlayer>()));
            services.AddScoped(p => new MyShowsPageViewModel(p.GetRequiredService<IFactory<Database.ShowTractorDbContext>>()));
            services.AddScoped(
                p => new CalendarPageViewModel(
                    p.GetRequiredService<IFactory<Database.ShowTractorDbContext>>(),
                    p.GetRequiredService<IAsyncInitializationService>(),
                    p.GetRequiredService<GeneralSettings>()));
            services.AddScoped(
                p => new UnwatchedPageViewModel(
                    p.GetRequiredService<IFactory<Database.ShowTractorDbContext>>(),
                    p.GetRequiredService<GeneralSettings>()));
            services.AddScoped(
                p => new AllTimeStatisticsViewModel(
                    p.GetRequiredService<Database.ShowTractorDbContext>(),
                    p.GetRequiredService<GeneralSettings>()));
        }

        private static void ConfigureBackgroundWorker(ServiceCollection services)
        {
            services.AddSingleton(p => new MetadataUpdateBackgroundWork(
                            GeneralSettings.Default,
                            p.GetRequiredService<IFactory<Database.ShowTractorDbContext>>(),
                            p.GetRequiredService<IFactory<IMetadataProvider?>>(),
                            p.GetRequiredService<HttpClient>()));
            services.AddSingleton(p =>
                new ShowTractorBackgroundWorker(new BackgroundWorkCollection(
                    new IBackgroundWork[]
                    {
                        p.GetRequiredService<MetadataUpdateBackgroundWork>()
                    })));
        }

        public object GetService(Type serviceType) => provider.GetService(serviceType);
    }
}
