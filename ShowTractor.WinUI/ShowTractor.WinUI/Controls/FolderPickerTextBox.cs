using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ShowTractor.Plugins.Interfaces;

namespace ShowTractor.WinUI.Controls
{
    public class FolderPickerTextBox : Control
    {
        public const string TextBoxPartName = "PART_TextBox";
        public const string ButtonPartName = "PART_Button";

        private TextBox TextBox => (TextBox)GetTemplateChild(TextBoxPartName);
        private Button Button => (Button)GetTemplateChild(ButtonPartName);

        public DirectoryPluginSettingsDescription DirectoryPluginSettingsDescription
        {
            get { return (DirectoryPluginSettingsDescription)GetValue(DirectoryPluginSettingsDescriptionProperty); }
            set { OnLoaded(this, new RoutedEventArgs()); SetValue(DirectoryPluginSettingsDescriptionProperty, value); }
        }

        public static readonly DependencyProperty DirectoryPluginSettingsDescriptionProperty =
            DependencyProperty.Register(nameof(DirectoryPluginSettingsDescription), typeof(DirectoryPluginSettingsDescription), typeof(FolderPickerTextBox), new PropertyMetadata(null));

        public FolderPickerTextBox()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs _)
        {
            if (DirectoryPluginSettingsDescription != null)
            {
                if (TextBox != null)
                {
                    TextBox.Text = DirectoryPluginSettingsDescription.Value;
                    TextBox.Header = DirectoryPluginSettingsDescription.Name;
                }
                if (Button != null)
                {
                    Button.Content = Properties.Resources.DirectoryBrowseButtonText;
                }
            }
            if (Button != null)
            {
                Button.Click -= OnClick;
                Button.Click += OnClick;
            }
        }

        private async void OnClick(object sender, RoutedEventArgs e)
        {
            if (Button == null || !Button.IsEnabled || DirectoryPluginSettingsDescription == null)
                return;
            try
            {
                Button.IsEnabled = false;
                var folder = await new OpenFileDialogService().OpenFolderAsync();
                if (folder != null)
                {
                    DirectoryPluginSettingsDescription.Value = TextBox.Text = folder;
                }
            }
            finally
            {
                Button.IsEnabled = true;
            }
        }
    }
}
