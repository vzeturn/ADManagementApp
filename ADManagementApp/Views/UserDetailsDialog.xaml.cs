using System.Windows;
using ADManagementApp.Models;

namespace ADManagementApp.Views
{
    public partial class UserDetailsDialog : Window
    {
        public UserDetailsDialog(ADUser user)
        {
            InitializeComponent();
            DataContext = user;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}