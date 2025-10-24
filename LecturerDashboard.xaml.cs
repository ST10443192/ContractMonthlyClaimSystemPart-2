using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ContractMonthlyClaimSystem2.Database;
using ContractMonthlyClaimSystem2.Helpers;
using ContractMonthlyClaimSystem2.Models;

namespace ContractMonthlyClaimSystem2
{
    public partial class LecturerDashboard : Window
    {
        private string _email;
        private string _role;
        private ObservableCollection<Claim> _claims;
        private List<string> _uploadedDocuments;

        public LecturerDashboard()
        {
            InitializeComponent();
            _claims = new ObservableCollection<Claim>();
            _uploadedDocuments = new List<string>();
        }

        public LecturerDashboard(string email, string role) : this()
        {
            _email = email;
            _role = role;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblUserInfo.Text = $"Welcome, {_role}! ({_email})";
            LoadExistingClaims();
            claimsDataGrid.ItemsSource = _claims;
        }

        private void LoadExistingClaims()
        {
            _claims.Clear();
            _claims.Add(new Claim
            {
                Id = 1001,
                LecturerId = "L-001",
                LecturerName = "Dr. Alice Smith",
                LecturerEmail = "alice.smith@university.ac.za",
                Amount = 4500.00m,
                HoursWorked = 30m,
                HourlyRate = 150m,
               
                Status = ClaimStatus.Submitted,
                SubmissionDate = DateTime.Now.AddDays(-5),
                Description = "Teaching: Software Engineering - 30 hours",
                Documents = new List<Document>()
            });
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                UserSession.Logout();
                new MainWindow().Show();
                Close();
            }
        }

        private void NewClaim_Click(object sender, RoutedEventArgs e)
        {
            ClearClaimForm();
            MessageBox.Show("Form cleared. Ready to submit a new claim!", "New Claim",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UploadDocument_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Supporting Documents",
                Filter = "All Files (*.*)|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                long maxBytes = 10 * 1024 * 1024;
                var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".pdf", ".docx", ".xlsx", ".jpg", ".jpeg", ".png" };

                int added = 0;
                foreach (string file in dialog.FileNames)
                {
                    string ext = System.IO.Path.GetExtension(file);
                    long size = new System.IO.FileInfo(file).Length;

                    if (!allowedExt.Contains(ext))
                    {
                        MessageBox.Show($"Disallowed file type: {ext}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }

                    if (size > maxBytes)
                    {
                        MessageBox.Show($"File too large: {System.IO.Path.GetFileName(file)}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }

                    if (!_uploadedDocuments.Contains(file))
                    {
                        _uploadedDocuments.Add(file);
                        lstDocuments.Items.Add(System.IO.Path.GetFileName(file));
                        added++;
                    }
                }

                if (added > 0)
                    MessageBox.Show($"{added} document(s) uploaded successfully.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CalculateAmount(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtHoursWorked.Text, out decimal hours) &&
                decimal.TryParse(txtHourlyRate.Text, out decimal rate))
                txtClaimAmount.Text = (hours * rate).ToString("F2");
            else
                txtClaimAmount.Text = "0.00";
        }

        private void SubmitClaim_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateClaimForm()) return;

            try
            {
                decimal.TryParse(txtClaimAmount.Text, out decimal amount);
                decimal.TryParse(txtHoursWorked.Text, out decimal hours);
                decimal.TryParse(txtHourlyRate.Text, out decimal rate);

                var newClaim = new Claim
                {
                    Id = GenerateClaimId(),
                    LecturerId = _email,
                    LecturerName = _role,
                    LecturerEmail = _email,
                    Amount = amount,
                    HoursWorked = hours,
                    HourlyRate = rate,
                    Status = ClaimStatus.Submitted,
                    SubmissionDate = DateTime.Now,
                    Description = txtDescription.Text,
                    Documents = _uploadedDocuments.Select(f => new Document
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        FileName = System.IO.Path.GetFileName(f),
                        FileType = System.IO.Path.GetExtension(f),
                        FileSizeBytes = new System.IO.FileInfo(f).Length,
                        UploadDate = DateTime.Now
                    }).ToList()
                };

                _claims.Insert(0, newClaim);
                DatabaseManager.Instance.SaveClaim(newClaim);
                AuditLogger.LogAction("SubmitClaim", $"ClaimId={newClaim.Id}, Amount={newClaim.Amount}");

                MessageBox.Show($"Claim submitted successfully!\nID: CLM-{newClaim.Id}\nAmount: R{newClaim.Amount:N2}",
                    "Claim Submitted", MessageBoxButton.OK, MessageBoxImage.Information);

                ClearClaimForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting claim: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ClaimsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (claimsDataGrid.SelectedItem is Claim selectedClaim)
            {
                string details =
                    $"Claim ID: CLM-{selectedClaim.Id}\n" +
                    $"Amount: R{selectedClaim.Amount:N2}\n" +
                    $"Status: {selectedClaim.Status}\n" +
                    $"Submitted: {selectedClaim.SubmissionDate:dd/MM/yyyy}\n" +
                    $"Description: {selectedClaim.Description}";

                MessageBox.Show(details, "Claim Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private bool ValidateClaimForm()
        {
            if (!decimal.TryParse(txtClaimAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Description is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearClaimForm()
        {
            txtClaimAmount.Clear();
            txtDescription.Clear();
            lstDocuments.Items.Clear();
            _uploadedDocuments.Clear();
            txtHoursWorked.Clear();
            txtHourlyRate.Clear();
        }

        private int GenerateClaimId()
        {
            if (_claims == null || _claims.Count == 0) return 1001;

            var numericIds = _claims.Select(c => c.Id).ToList();
            return numericIds.Max() + 1;
        }

        private void ViewClaimDetails_Click(object sender, RoutedEventArgs e)
        {
            if (claimsDataGrid.SelectedItem is Claim claim)
            {
                string docs = claim.Documents != null && claim.Documents.Any()
                    ? string.Join("\n", claim.Documents.Select(d =>
                        $"{d.FileName} ({d.FileSizeBytes} bytes) - {d.UploadDate:dd/MM/yyyy}"))
                    : "No documents attached.";

                MessageBox.Show(docs, $"Claim CLM-{claim.Id} Documents",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select a claim first.",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}