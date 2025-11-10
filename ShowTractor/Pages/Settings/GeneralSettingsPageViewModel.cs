using ShowTractor.Mvvm;
using ShowTractor.Properties;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace ShowTractor.Pages.Settings
{
    public class GeneralSettingsPageViewModel : ISupportNavigation
    {
        private readonly Assembly assembly = Assembly.GetExecutingAssembly();
        internal GeneralSettingsPageViewModel(GeneralSettings settings)
        {
            Settings = settings;
            var stringBuilder = new StringBuilder(128);
            StrFormatByteSize(new FileInfo(settings.DatabaseFilename).Length, stringBuilder, stringBuilder.Capacity);
            DatabaseSize = stringBuilder.ToString();
            LoadCommand = new DelegateCommand(() =>
            {

            });
            MoveCommand = new DelegateCommand(() =>
            {

            });
            ChangeCommand = new DelegateCommand(() =>
            {

            });
        }
        public GeneralSettings Settings { get; }
        public string Version => Resources.VersionColon + assembly.GetName().Version?.ToString();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used by generated files.")]
        public string Author => Resources.AuthorColonAuthorName;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used by generated files.")]
        public string Copyright => Resources.CopyrightColonCopyrightInfo;
        public ICommand LoadCommand { get; set; }
        public ICommand MoveCommand { get; set; }
        public ICommand ChangeCommand { get; set; }
        public object? Parameter { get => null; set { } }
        public string DatabaseSize { get; private set; }
        public void OnNavigatedFrom()
        {
            Settings.Save();
        }
        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern long StrFormatByteSize(long fileSize, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer, int bufferSize);
    }
}
