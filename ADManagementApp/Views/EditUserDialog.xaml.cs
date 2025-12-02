using System.Windows;
using ADManagementApp.Models;

namespace ADManagementApp.Views
{
    /// <summary>
    /// Dialog for editing an existing Active Directory user.
    /// Allows modification of user properties and applies changes back to AD.
    /// </summary>
    public partial class EditUserDialog : Window
    {
        /// <summary>
        /// Gets the edited user object with updated properties.
        /// </summary>
        public ADUser? EditedUser { get; private set; }

        public EditUserDialog(ADUser user)
        {
            InitializeComponent();

            // Load existing user data
            DisplayNameTextBox.Text = user.DisplayName;
            FirstNameTextBox.Text = user.GivenName;
            LastNameTextBox.Text = user.Surname;
            EmailTextBox.Text = user.EmailAddress;
            DepartmentTextBox.Text = user.Department;
            TitleTextBox.Text = user.Title;
            PhoneTextBox.Text = user.PhoneNumber;
            DescriptionTextBox.Text = user.Description;

            EditedUser = user;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditedUser == null)
                return;

            // Update user object
            EditedUser.DisplayName = DisplayNameTextBox.Text.Trim();
            EditedUser.GivenName = FirstNameTextBox.Text.Trim();
            EditedUser.Surname = LastNameTextBox.Text.Trim();
            EditedUser.EmailAddress = EmailTextBox.Text.Trim();
            EditedUser.Department = DepartmentTextBox.Text.Trim();
            EditedUser.Title = TitleTextBox.Text.Trim();
            EditedUser.PhoneNumber = PhoneTextBox.Text.Trim();
            EditedUser.Description = DescriptionTextBox.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
