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
            
            // Initialize services
            var adService = new ADService();
            
            // Create main window with ViewModel
            var mainViewModel = new MainViewModel(adService);
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            
            mainWindow.Show();
        }
    }
}