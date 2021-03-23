using ShowTractor.Interfaces;
using ShowTractor.Mvvm;
using ShowTractor.Plugins;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShowTractor.Pages.Settings
{
    public class PluginSettingsPageViewModel : INotifyPropertyChanged, ISupportNavigation
    {
        private readonly PluginSettings settings;
        private readonly IOpenFileDialogService openFileDialogService;
        private readonly IServiceProvider serviceProvider;

        internal PluginSettingsPageViewModel(PluginSettings settings, IOpenFileDialogService openFileDialogService, IServiceProvider serviceProvider)
        {
            this.settings = settings;
            this.openFileDialogService = openFileDialogService;
            this.serviceProvider = serviceProvider;
            foreach (var definition in settings.MetadataProviders)
            {
                DoAdd(() => MetadataProviders.Add(new MetadataProviderPluginViewModel(definition, serviceProvider)));
            }
            foreach (var definition in settings.MediaSourceProviders)
            {
                DoAdd(() => MediaSourceProviders.Add(new MediaSourceProviderPluginViewModel(definition, serviceProvider)));
            }
            foreach (var definition in settings.DownloadManagers)
            {
                DoAdd(() => DownloadManagers.Add(new DownloadManagerPluginViewModel(definition, serviceProvider)));
            }
            foreach (var definition in settings.MediaPlayers)
            {
                DoAdd(() => MediaPlayers.Add(new MediaPlayerPluginViewModel(definition, serviceProvider)));
            }

            void DoAdd(Action action)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    if (ErrorMessage != string.Empty)
                        ErrorMessage += Environment.NewLine;
                    ErrorMessage += ex.Message;
                }
            }
        }

        public ObservableCollection<PluginViewModel> MetadataProviders { get; private set; } = new ObservableCollection<PluginViewModel>();
        public ObservableCollection<PluginViewModel> MediaSourceProviders { get; private set; } = new ObservableCollection<PluginViewModel>();
        public ObservableCollection<PluginViewModel> DownloadManagers { get; private set; } = new ObservableCollection<PluginViewModel>();
        public ObservableCollection<PluginViewModel> MediaPlayers { get; private set; } = new ObservableCollection<PluginViewModel>();
        public object? Parameter { get => null; set { } }
        public string ErrorMessage { get => errorMessage; set { errorMessage = value; OnPropertyChanged(); } }
        private string errorMessage = string.Empty;
        public ICommand LoadMetadataProviderCommand => new AwaitableDelegateCommand(async () =>
            await LoadPluginAsync(d =>
            {
                var vm = new MetadataProviderPluginViewModel(d, serviceProvider);
                MetadataProviders.Add(vm);
                settings.MetadataProviders.Add(d);
            }));
        public ICommand LoadMediaSourceProviderCommand => new AwaitableDelegateCommand(async () =>
            await LoadPluginAsync(d =>
            {
                var vm = new MediaSourceProviderPluginViewModel(d, serviceProvider);
                MediaSourceProviders.Add(vm);
                settings.MetadataProviders.Add(d);
            }));
        public ICommand LoadDownloadManagerCommand => new AwaitableDelegateCommand(async () =>
            await LoadPluginAsync(d =>
            {
                var vm = new DownloadManagerPluginViewModel(d, serviceProvider);
                DownloadManagers.Add(vm);
                settings.MetadataProviders.Add(d);
            }));
        public ICommand LoadMediaPlayerCommand => new AwaitableDelegateCommand(async () =>
            await LoadPluginAsync(d =>
            {
                var vm = new MediaPlayerPluginViewModel(d, serviceProvider);
                MediaPlayers.Add(vm);
                settings.MetadataProviders.Add(d);
            }));
        public ICommand RemoveCommand => new DelegateCommand<PluginViewModel>(p =>
        {
            settings.MetadataProviders.Remove(p.Definition);
            settings.MediaSourceProviders.Remove(p.Definition);
            settings.DownloadManagers.Remove(p.Definition);
            settings.MediaPlayers.Remove(p.Definition);
            settings.Save();
            MetadataProviders.Remove(p);
        });
        private async Task LoadPluginAsync(Action<PluginDefinition> action)
        {
            ErrorMessage = string.Empty;
            try
            {
                var path = await openFileDialogService.OpenFileAsync(new string[] { ".dll" });
                if (path != null)
                {
                    var definition = new PluginDefinition() { Enabled = true, FileName = path };
                    action(definition);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            settings.Save();
        }

        public bool OnNavigatingFrom()
        {
            // Save ordering.
            settings.MetadataProviders.Clear();
            foreach (var provider in MetadataProviders)
            {
                settings.MetadataProviders.Add(provider.Definition);
            }
            settings.Save();
            return false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
