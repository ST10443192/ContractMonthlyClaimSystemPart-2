using System;
using System.IO;
using System.Windows;
using BCrypt.Net;

namespace ContractMonthlyClaimSystem2.Helpers
{
    // ============================================
    // PASSWORD HASHING UTILITY
    // ============================================
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public static bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }
    }

    // ============================================
    // USER SESSION MANAGEMENT
    // ============================================
    public static class UserSession
    {
        public static Models.User CurrentUser { get; set; }

        public static bool IsAuthenticated => CurrentUser != null;

        public static bool HasRole(params string[] roles)
        {
            if (CurrentUser == null) return false;

            foreach (var role in roles)
            {
                if (CurrentUser.Role.Equals(role, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }

    // ============================================
    // AUTHORIZATION ATTRIBUTE
    // ============================================
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RequireRoleAttribute : Attribute
    {
        public string[] Roles { get; }

        public RequireRoleAttribute(params string[] roles)
        {
            Roles = roles;
        }

        public bool IsAuthorized()
        {
            return UserSession.HasRole(Roles);
        }
    }

    // ============================================
    // SECURE WINDOW BASE CLASS
    // ============================================
    public abstract class SecureWindow : Window
    {
        protected string[] RequiredRoles { get; set; }

        protected SecureWindow(params string[] requiredRoles)
        {
            RequiredRoles = requiredRoles;
            Loaded += SecureWindow_Loaded;
        }

        private void SecureWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UserSession.IsAuthenticated)
            {
                MessageBox.Show("You must be logged in to access this page.",
                    "Unauthorized",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                new MainWindow().Show();
                this.Close();
                return;
            }

            if (RequiredRoles != null && RequiredRoles.Length > 0)
            {
                if (!UserSession.HasRole(RequiredRoles))
                {
                    MessageBox.Show($"Access denied. This page requires one of the following roles: {string.Join(", ", RequiredRoles)}",
                        "Unauthorized",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    new MainWindow().Show();
                    this.Close();
                }
            }
        }
    }

    // ============================================
    // AUDIT LOGGER
    // ============================================
    public static class AuditLogger
    {
        private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audit.log");

        public static void LogAction(string action, string details = "")
        {
            if (!UserSession.IsAuthenticated) return;

            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | User: {UserSession.CurrentUser.Email} | Role: {UserSession.CurrentUser.Role} | Action: {action} | Details: {details}";

            try
            {
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Audit log failed: {ex.Message}");
            }
        }

        public static void LogLogin(string email, bool success)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | LOGIN | Email: {email} | Success: {success}";

            try
            {
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch { }
        }
    }
}
