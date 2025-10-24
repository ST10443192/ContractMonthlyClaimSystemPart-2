using System;
using System.Windows;
using System.Windows.Controls;
using ContractMonthlyClaimSystem2.Database;
using ContractMonthlyClaimSystem2.Helpers;

namespace ContractMonthlyClaimSystem2
{
    /// <summary>
    /// Dashboard for system administrators
    /// </summary>
    public partial class AdminDashboard : Window
    {
        private readonly string _email;
        private readonly string _fullName;

        public AdminDashboard()
        {
            InitializeComponent();
        }

        public AdminDashboard(string email, string fullName) : this()
        {
            _email = email;
            _fullName = fullName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblUserInfo.Content = $"Welcome, {_fullName}! ({_email})";
            AuditLogger.LogAction("DASHBOARD_ACCESS", "Admin accessed dashboard");
        }

        private void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            string email = txtNewUserEmail.Text.Trim();
            string password = txtNewUserPassword.Password;
            string fullName = txtNewUserFullName.Text.Trim();
            string role = (cmbRole.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!email.Contains("@"))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool success = DatabaseManager.Instance.CreateUser(email, password, fullName, role);

                if (success)
                {
                    AuditLogger.LogAction("CREATE_USER", $"Created new user: {email} with role: {role}");
                    MessageBox.Show($"User created successfully!\n\nEmail: {email}\nRole: {role}",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    txtNewUserEmail.Clear();
                    txtNewUserPassword.Clear();
                    txtNewUserFullName.Clear();
                    cmbRole.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("Failed to create user. Email may already exist.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating user: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.CurrentUser == null)
            {
                MessageBox.Show("No user is currently logged in.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string oldPassword = txtOldPassword.Password;
            string newPassword = txtNewPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            if (string.IsNullOrWhiteSpace(oldPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Please fill in all password fields.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("New password and confirmation do not match.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword.Length < 8)
            {
                MessageBox.Show("New password must be at least 8 characters long.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?",
                "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AuditLogger.LogAction("LOGOUT", $"User {_email} logged out");
                UserSession.Logout();

                new MainWindow().Show();
                Close();
            }
        }
    }
}