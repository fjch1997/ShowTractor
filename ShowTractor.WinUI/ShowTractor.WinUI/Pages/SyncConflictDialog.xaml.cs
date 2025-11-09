using Microsoft.UI.Xaml.Controls;

namespace ShowTractor.WinUI.Pages
{
    public sealed partial class SyncConflictDialog : ContentDialog
    {
        public SyncConflictDialog()
        {
            InitializeComponent();
        }

        public INotificationService.SyncConflictResolution Resolution
        {
            get
            {
                if (keepLocalButton.IsChecked == true)
                {
                    return INotificationService.SyncConflictResolution.KeepLocal;
                }
                else if (keepRemoteButton.IsChecked == true)
                {
                    return INotificationService.SyncConflictResolution.KeepRemote;
                }
                else
                {
                    return INotificationService.SyncConflictResolution.Cancel;
                }
            }
        }
    }
}
