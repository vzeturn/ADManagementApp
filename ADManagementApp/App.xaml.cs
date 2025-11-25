using System.Windows;
using ADManagementApp.Services;
using ADManagementApp.ViewModels;

namespace ADManagementApp
{
    public partial class App : Application
    {
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
            var mainViewModel = new MainViewModel(adService);
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