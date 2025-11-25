using System.Windows;
using System.Windows.Controls;

namespace ADManagementApp.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Auto-load data when view is displayed
            if (DataContext is ViewModels.DashboardViewModel viewModel)
            {
                await viewModel.LoadDataAsync();
            }
        }
    }
}