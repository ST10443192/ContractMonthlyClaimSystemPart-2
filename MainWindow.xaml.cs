using System;
using System.Windows;
using Microsoft.VisualBasic; // <-- Needed for Interaction.InputBox
using ContractMonthlyClaimSystem2.Database;
using ContractMonthlyClaimSystem2.Helpers;
using ContractMonthlyClaimSystem2.Models;

namespace ContractMonthlyClaimSystem2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text?.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both email and password.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // ✅ Authenticate user
            var user = DatabaseManager.Instance.AuthenticateUser(email, password);

            if (user != null)
            {
                MessageBox.Show($"Welcome {user.FullName}!", "Login Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigateToDashboard(user);
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid email or password.",
                    "Login Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void NavigateToDashboard(Users user)
        {
            if (user == null)
            {
                MessageBox.Show("User is null.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Window dashboard = null;
            string role = (user.Role ?? string.Empty).Trim();

            switch (role)
            {
                case "Lecturer":
                    dashboard = new LecturerDashboard(user.Email, user.FullName);
                    break;

                case "Coordinator":
                    dashboard = new CoordinatorDashboard(user.Email, user.FullName);
                    break;

                case "Manager":
                    dashboard = new ManagerDashboard(user.Email, user.FullName);
                    break;

                case "Admin":
                    dashboard = new AdminDashboard(user.Email, user.FullName);
                    break;

                default:
                    MessageBox.Show("Invalid user role.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            dashboard?.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtEmail.Text = "";
            txtPassword.Password = "";

            MessageBox.Show(
                "Default Login Credentials:\n\n" +
                "Admin: admin@university.ac.za / Admin@123\n" +
                "Lecturer: lecturer@university.ac.za / Lecturer@123\n" +
                "Coordinator: coordinator@university.ac.za / Coordinator@123\n" +
                "Manager: manager@university.ac.za / Manager@123",
                "Login Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // 🔽 NEW IMPLEMENTATION: Full working registration feature
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = Interaction.InputBox("Enter email:", "Register (demo)", "user@example.com").Trim();
                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Registration cancelled or email empty.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string fullName = Interaction.InputBox("Full name:", "Register (demo)", "Jane Doe").Trim();
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    MessageBox.Show("Full name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string password = Interaction.InputBox("Password (min 8 chars):", "Register (demo)", "P@ssw0rd");
                if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                {
                    MessageBox.Show("Password must be at least 8 characters long.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBoxResult confirmPassword = MessageBox.Show($"Your password is: {password}\n\nPress Yes to continue, No to re-enter password.",
                    "Confirm Password", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirmPassword == MessageBoxResult.No)
                {
                    password = Interaction.InputBox("Re-enter Password (min 8 chars):", "Register (demo)", "P@ssw0rd");
                    if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                    {
                        MessageBox.Show("Password must be at least 8 characters long.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                string role = Interaction.InputBox("Role (Lecturer / Coordinator / Manager / Admin):", "Register (demo)", "Lecturer").Trim();
                if (string.IsNullOrWhiteSpace(role)) role = "Lecturer";
                role = role.Trim();

                if (role != "Lecturer" && role != "Coordinator" && role != "Manager" && role != "Admin")
                {
                    MessageBox.Show("Invalid role. Use one of: Lecturer, Coordinator, Manager, Admin",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool created = DatabaseManager.Instance.CreateUser(email, password, fullName, role);

                if (created)
                {
                    MessageBox.Show($"User created successfully.\nEmail: {email}\nRole: {role}", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // ✅ Auto-login after successful registration
                    var newUser = new Users
                    {
                        Email = email,
                        FullName = fullName,
                        Role = role
                    };

                    NavigateToDashboard(newUser);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to create user. Email may already exist.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration error: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
