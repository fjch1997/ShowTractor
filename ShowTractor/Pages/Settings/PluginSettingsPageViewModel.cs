using ShowTractor.Interfaces;
using ShowTractor.Mvvm;
using ShowTractor.Plugins;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
            MetadataProviders = new ObservableCollection<PluginViewModel>();
            foreach (var definition in settings.MetadataProviders)
            {
                try
                {
                    MetadataProviders.Add(new MetadataProviderPluginViewModel(definition, serviceProvider));
                }
                catch (Exception ex)
                {
                    if (ErrorMessage != string.Empty)
                        ErrorMessage += Environment.NewLine;
                    ErrorMessage += ex.Message;
                }
            }
        }

        public ObservableCollection<PluginViewModel> MetadataProviders { get; private set; }
        public object? Parameter { get => null; set { } }
        public string ErrorMessage { get => errorMessage; set { errorMessage = value; OnPropertyChanged(); } }
        private string errorMessage = string.Empty;
        public ICommand LoadMetadataProviderCommand => new AwaitableDelegateCommand(async () =>
        {
            ErrorMessage = string.Empty;
            try
            {
                var path = await openFileDialogService.OpenFileAsync(new string[] { ".dll" });
                if (path != null)
                {
                    var definition = new PluginDefinition() { Enabled = true, FileName = path };
                    var vm = new MetadataProviderPluginViewModel(definition, serviceProvider);
                    MetadataProviders.Add(vm);
                    settings.MetadataProviders.Add(definition);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            settings.Save();
        });
        public ICommand RemoveCommand => new DelegateCommand<PluginViewModel>(p =>
        {
            settings.MetadataProviders.Remove(p.Definition);
            settings.Save();
            MetadataProviders.Remove(p);
        });

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
