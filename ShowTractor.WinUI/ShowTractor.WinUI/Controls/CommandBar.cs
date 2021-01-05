namespace ShowTractor.WinUI.Controls
{
    public class CommandBar : Microsoft.UI.Xaml.Controls.CommandBar
    {
        public CommandBar()
        {
            Loaded += CommandBar_Loaded;
        }

        private void CommandBar_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var button = (Microsoft.UI.Xaml.Controls.Button)GetTemplateChild("MoreButton");
            button.CornerRadius = new Microsoft.UI.Xaml.CornerRadius(20);
            Loaded -= CommandBar_Loaded;
        }
    }
}
