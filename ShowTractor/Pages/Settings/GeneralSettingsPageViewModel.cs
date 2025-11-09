using ShowTractor.Database;
using ShowTractor.Mvvm;
using ShowTractor.Properties;
using System.Reflection;

namespace ShowTractor.Pages.Settings
{
    public class GeneralSettingsPageViewModel : ISupportNavigation
    {
        private readonly Assembly assembly = Assembly.GetExecutingAssembly();
        internal GeneralSettingsPageViewModel(GeneralSettings settings, DbSyncService dbSyncService)
        {
            Settings = settings;
            DbSyncService = dbSyncService;
        }

        public GeneralSettings Settings { get; }
        public DbSyncService DbSyncService { get; }

        public string Version => Resources.VersionColon + assembly.GetName().Version.ToString();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used by generated files.")]
        public string Author => Resources.AuthorColonAuthorName;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used by generated files.")]
        public string Copyright => Resources.CopyrightColonCopyrightInfo;
        public object? Parameter { get => null; set { } }
        public void OnNavigatedFrom()
        {
            Settings.Save();
        }
    }
}
