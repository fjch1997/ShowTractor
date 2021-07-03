using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using ShowTractor.Background;
using System;
using Windows.ApplicationModel;

namespace ShowTractor.WinUI
{
    public partial class App : Application
    {
        private ShowTractorBackgroundWorker? backgroundWorker;
        private Window? mainWindow;
        public App()
        {
            InitializeComponent();
        }
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            mainWindow = new MainWindow();
            mainWindow.Activate();
            var serviceProvider = (ShowTractorServiceProvider)Resources["ServiceProvider"] ?? throw new ArgumentNullException($"A {nameof(ServiceProvider)} must exists in the resource dictionary of the app.");
            backgroundWorker = serviceProvider.GetRequiredService<ShowTractorBackgroundWorker>();
            backgroundWorker.Start();
            base.OnLaunched(args);
        }
        public Window MainWindow
        {
            set => mainWindow = value;
            get => mainWindow ?? throw new InvalidOperationException("Uninitialized property: " + nameof(MainWindow));
        }
    }
}
