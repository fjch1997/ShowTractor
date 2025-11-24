using Microsoft.Data.Sqlite;
using ShowTractor.Database;
using ShowTractor.Interfaces;
using ShowTractor.Mvvm;
using ShowTractor.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace ShowTractor.Pages.Settings
{
    public class GeneralSettingsPageViewModel : ISupportNavigation, INotifyPropertyChanged
    {
        private readonly Assembly assembly = Assembly.GetExecutingAssembly();

        public event PropertyChangedEventHandler? PropertyChanged;

        internal GeneralSettingsPageViewModel(GeneralSettings settings, IOpenFileDialogService openFileDialogService)
        {
            Settings = settings;
            var stringBuilder = new StringBuilder(128);
            StrFormatByteSize(new FileInfo(settings.DatabaseFilename).Length, stringBuilder, stringBuilder.Capacity);
            DatabaseSize = stringBuilder.ToString();
            LoadCommand = new AwaitableDelegateCommand(async () =>
            {
                var result = await openFileDialogService.OpenFileAsync(new string[] { ".sqlite" });
                if (string.IsNullOrEmpty(result))
                    return;
                var oldFilename = settings.DatabaseFilename;
                settings.DatabaseFilename = result;
                try
                {
                    using var context = new ShowTractorDbContext(settings);
                    context.TvSeasons.Count();
                    DatabaseErrorMessage = null;
                    settings.Save();
                }
                catch (Exception ex)
                {
                    DatabaseErrorMessage = Resources.GeneralSettingsCouldNotLoadDatabaseFile + Environment.NewLine + ex.Message;
                    settings.DatabaseFilename = oldFilename;
                }
            });
            MoveCommand = new AwaitableDelegateCommand(async () =>
            {
                var result = await openFileDialogService.SaveFileAsync(new Dictionary<string, IList<string>>
                {
                    { "SQLite database", new List<string> { ".sqlite" } }
                }, "ShowTractor.sqlite", ".sqlite");
                if (string.IsNullOrEmpty(result))
                    return;
                try
                {
                    using (var localDbConnection = new SqliteConnection(
                        new SqliteConnectionStringBuilder { DataSource = Settings.DatabaseFilename, Mode = SqliteOpenMode.ReadWrite }.ConnectionString))
                    using (var remoteConnection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = result }.ConnectionString))
                    {
                        localDbConnection.Open();
                        localDbConnection.BackupDatabase(remoteConnection);
                        SqliteConnection.ClearPool(localDbConnection);
                    }
                    settings.DatabaseFilename = result;
                    settings.Save();
                    DatabaseErrorMessage = null;
                }
                catch (Exception ex)
                {
                    DatabaseErrorMessage = Resources.GeneralSettingsCouldNotMoveDatabaseFile + Environment.NewLine + ex.Message;
                }
            });
            ChangeCommand = new AwaitableDelegateCommand(async () =>
            {
                var result = await openFileDialogService.PickFolderAsync();
                if (!string.IsNullOrEmpty(result))
                {
                    Settings.ArtworkDirectoryName = result;
                }
            });
        }
        public GeneralSettings Settings { get; }
        public string Version => Resources.VersionColon + assembly.GetName().Version?.ToString();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used by generated files.")]
        public string Author => Resources.AuthorColonAuthorName;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used by generated files.")]
        public string Copyright => Resources.CopyrightColonCopyrightInfo;
        public AwaitableDelegateCommand LoadCommand { get; set; }
        public AwaitableDelegateCommand MoveCommand { get; set; }
        public AwaitableDelegateCommand ChangeCommand { get; set; }
        public object? Parameter { get => null; set { } }
        public string DatabaseSize { get; private set; }
        public string? DatabaseErrorMessage { get => databaseErrorMessage; set { databaseErrorMessage = value; OnPropertyChanged(); } }
        private string? databaseErrorMessage;
        public void OnNavigatedFrom()
        {
            Settings.Save();
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern long StrFormatByteSize(long fileSize, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer, int bufferSize);
    }
}
