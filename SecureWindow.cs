using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace ContractMonthlyClaimSystem2
{
    /// <summary>
    /// A secure base window class that validates user roles and provides optional secure storage.
    /// </summary>
    public class SecureWindow : Window
    {
        private readonly string[] _allowedRoles;

        public SecureWindow(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? Array.Empty<string>();
            this.Loaded += SecureWindow_Loaded;
        }

        private void SecureWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // If role checking is used, ensure user has valid access
            var currentRole = SecureStorage.Get("UserRole");
            if (_allowedRoles.Length > 0 && !string.IsNullOrEmpty(currentRole))
            {
                if (!_allowedRoles.Contains(currentRole))
                {
                    MessageBox.Show("Access denied. You do not have permission to view this page.",
                                    "Security Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
            }
        }

        // ✅ Secure Storage using AES Encryption (for session data, tokens, etc.)
        public static class SecureStorage
        {
            private static readonly byte[] key = Encoding.UTF8.GetBytes("A1b2C3d4E5f6G7h8"); // 16 bytes key
            private static readonly byte[] iv = Encoding.UTF8.GetBytes("1H2i3J4k5L6m7N8o"); // 16 bytes IV

            public static void Set(string keyName, string value)
            {
                if (string.IsNullOrEmpty(keyName)) return;
                string encrypted = Encrypt(value);
                Properties.Settings.Default[keyName] = encrypted;
                Properties.Settings.Default.Save();
            }

            public static string Get(string keyName)
            {
                try
                {
                    string encrypted = Properties.Settings.Default[keyName]?.ToString();
                    return string.IsNullOrEmpty(encrypted) ? string.Empty : Decrypt(encrypted);
                }
                catch
                {
                    return string.Empty;
                }
            }

            public static void Remove(string keyName)
            {
                Properties.Settings.Default[keyName] = string.Empty;
                Properties.Settings.Default.Save();
            }

            private static string Encrypt(string plainText)
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    return Convert.ToBase64String(encrypted);
                }
            }

            private static string Decrypt(string cipherText)
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    byte[] cipherBytes = Convert.FromBase64String(cipherText);
                    byte[] decrypted = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(decrypted);
                }
            }
        }

        public void SecureClose()
        {
            this.Close();
        }
    }
}
