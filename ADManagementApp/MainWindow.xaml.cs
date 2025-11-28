using System.Windows;
using ADManagementApp.ViewModels;

namespace ADManagementApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// View should be dumb - all logic in ViewModel
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}