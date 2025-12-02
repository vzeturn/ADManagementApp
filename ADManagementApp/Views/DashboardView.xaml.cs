using System.Windows.Controls;

namespace ADManagementApp.Views
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// View is dumb - no auto-loading, user controls everything
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            // NO auto-loading - user will click refresh button when needed
        }
    }
}
