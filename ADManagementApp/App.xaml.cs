using ADManagementApp.Services;
using ADManagementApp.ViewModels;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows;

namespace ADManagementApp
{
    public partial class App : Application
    {
        public IConfiguration Configuration { get; }
        public App()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Prevent multiple windows - ensure only one MainWindow
            // Remove any existing windows
            foreach (Window window in Windows)
            {
                if (window is MainWindow)
                    window.Close();
            }

            // Initialize services
            var adService = new ADService();

            // Create main window with ViewModel
            var mainViewModel = new MainViewModel(Configuration, adService);
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            // Set as main window
            MainWindow = mainWindow;

            // Show the window
            mainWindow.Show();
        }
    }
}