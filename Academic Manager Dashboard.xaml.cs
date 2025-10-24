using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ContractMonthlyClaimSystem2.Models;
using ContractMonthlyClaimSystem2.Helpers;

namespace ContractMonthlyClaimSystem2
{
    public partial class ManagerDashboard : Window
    {
        private string userEmail;
        private string userRole;
        public ObservableCollection<Claim> AllClaims { get; set; }
        public ObservableCollection<Claim> FilteredClaims { get; set; }
        private Claim selectedClaim;

        public ManagerDashboard(string email, string role)
        {
            InitializeComponent();

            userEmail = email;
            userRole = role;

            AllClaims = new ObservableCollection<Claim>();
            FilteredClaims = new ObservableCollection<Claim>();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblUserInfo.Content = $"Logged in as: {userEmail} ({userRole})";
            cmbFilter.SelectedIndex = 0;
            claimsDataGrid.ItemsSource = FilteredClaims;
            ApplyFilter();
            UpdateDashboardStats();
        }

        private void ClaimsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (claimsDataGrid.SelectedItem is Claim claim)
            {
                selectedClaim = claim;
                DisplayClaimDetails(claim);
            }
        }

        private void DisplayClaimDetails(Claim claim)
        {
            if (claim == null) return;

            txtLecturerName.Text = claim.LecturerName;
            txtAmount.Text = $"R{claim.Amount:N2}";
            txtStatus.Text = claim.Status.ToString();
            txtDescription.Text = claim.Description;
            txtSubmittedDate.Text = claim.SubmissionDate.ToString("dd/MM/yyyy");
            lstDocuments.ItemsSource = claim.Documents;

            UpdateActionButtonStates();
        }

        private void ApplyFilter()
        {
            if (cmbFilter.SelectedIndex < 0) return;

            FilteredClaims.Clear();
            string filterText = ((ComboBoxItem)cmbFilter.SelectedItem).Content.ToString();

            var filtered = AllClaims.AsEnumerable();

            if (filterText == "Under Review")
                filtered = AllClaims.Where(c => c.Status == ClaimStatus.UnderReview);
            else if (filterText == "Approved")
                filtered = AllClaims.Where(c => c.Status == ClaimStatus.Approved);
            else if (filterText == "Paid")
                filtered = AllClaims.Where(c => c.Status == ClaimStatus.Paid);
            else if (filterText == "Rejected")
                filtered = AllClaims.Where(c => c.Status == ClaimStatus.Rejected);

            foreach (var claim in filtered)
                FilteredClaims.Add(claim);

            UpdateDashboardStats();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApproveClaim_Click(object sender, RoutedEventArgs e)
        {
            if (selectedClaim == null)
            {
                MessageBox.Show("Please select a claim to approve.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            selectedClaim.Status = ClaimStatus.Approved;
            MessageBox.Show("Claim approved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateDashboardStats();
            UpdateActionButtonStates();
        }

        private void RejectClaim_Click(object sender, RoutedEventArgs e)
        {
            if (selectedClaim == null)
            {
                MessageBox.Show("Please select a claim to reject.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            selectedClaim.Status = ClaimStatus.Rejected;
            MessageBox.Show("Claim rejected successfully.", "Rejected", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateDashboardStats();
            UpdateActionButtonStates();
        }

        private void MarkAsPaid_Click(object sender, RoutedEventArgs e)
        {
            if (selectedClaim == null)
            {
                MessageBox.Show("Please select a claim to mark as paid.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedClaim.Status != ClaimStatus.Approved)
            {
                MessageBox.Show("Only approved claims can be marked as paid.", "Invalid Action", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            selectedClaim.Status = ClaimStatus.Paid;
            MessageBox.Show("Claim marked as paid successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateDashboardStats();
            UpdateActionButtonStates();
        }

        private void ViewDocuments_Click(object sender, RoutedEventArgs e)
        {
            if (selectedClaim == null || selectedClaim.Documents == null || !selectedClaim.Documents.Any())
            {
                MessageBox.Show("No documents attached.", "No Documents", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string docList = "Attached Documents:\n\n";
            foreach (var doc in selectedClaim.Documents)
            {
                docList += $"{doc.FileName}\n   Size: {FormatFileSize(doc.FileSizeBytes)}\n   Uploaded: {doc.UploadDate:dd/MM/yyyy HH:mm}\n\n";
            }

            MessageBox.Show(docList, "Documents", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void UpdateDashboardStats()
        {
            int underReview = AllClaims.Count(c => c.Status == ClaimStatus.UnderReview);
            int approved = AllClaims.Count(c => c.Status == ClaimStatus.Approved);
            int paid = AllClaims.Count(c => c.Status == ClaimStatus.Paid);
            int rejected = AllClaims.Count(c => c.Status == ClaimStatus.Rejected);
            decimal totalApproved = AllClaims.Where(c => c.Status == ClaimStatus.Approved).Sum(c => c.Amount);

            lblStats.Content = $"Total: {AllClaims.Count} | Pending: {underReview} | Approved: {approved} | Paid: {paid} | Rejected: {rejected} | Total Approved: R{totalApproved:N2}";
        }

        private void UpdateActionButtonStates()
        {
            btnViewDocuments.IsEnabled = selectedClaim != null && selectedClaim.Documents != null && selectedClaim.Documents.Any();
            btnApprove.IsEnabled = selectedClaim != null && selectedClaim.Status == ClaimStatus.UnderReview;
            btnReject.IsEnabled = selectedClaim != null && selectedClaim.Status == ClaimStatus.UnderReview;
            btnMarkPaid.IsEnabled = selectedClaim != null && selectedClaim.Status == ClaimStatus.Approved;
        }
    }
}
