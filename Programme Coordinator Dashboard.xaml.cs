using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ContractMonthlyClaimSystem2.Models;

namespace ContractMonthlyClaimSystem2
{
    public partial class CoordinatorDashboard : Window
    {
        private ObservableCollection<ClaimForReview> _allClaims;
        private ObservableCollection<ClaimForReview> _filteredClaims;
        private ClaimForReview _selectedClaim;

        public CoordinatorDashboard(string email, string role)
        {
            InitializeComponent();
            lblUserInfo.Text = $"Welcome, {role}! ({email})";
            _allClaims = new ObservableCollection<ClaimForReview>();
            _filteredClaims = new ObservableCollection<ClaimForReview>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadClaims();
            claimsDataGrid.ItemsSource = _filteredClaims;
            cmbStatusFilter.SelectedIndex = 0;
            UpdateStats();
        }

        private void LoadClaims()
        {
            _allClaims.Clear();
            _allClaims.Add(new ClaimForReview { Id = 1, LecturerName = "Sarah Johnson", LecturerEmail = "sarah@uni.ac.za", Amount = 3500, HoursWorked = 20, Status = ClaimStatus.Submitted, Description = "Intro to C# tutorials", SubmissionDate = DateTime.Now.AddDays(-1) });
            _allClaims.Add(new ClaimForReview { Id = 2, LecturerName = "Michael Brown", LecturerEmail = "michael@uni.ac.za", Amount = 2800, HoursWorked = 16, Status = ClaimStatus.Approved, Description = "DB Management", SubmissionDate = DateTime.Now.AddDays(-3) });

            ApplyFilter("Pending Only");
        }

        private void ApplyFilter(string filter)
        {
            _filteredClaims.Clear();
            IEnumerable<ClaimForReview> filtered = _allClaims;

            if (filter == "Pending Only")
                filtered = _allClaims.Where(c => c.Status == ClaimStatus.Submitted);
            else if (filter == "Under Review")
                filtered = _allClaims.Where(c => c.Status == ClaimStatus.UnderReview);
            else if (filter == "Approved")
                filtered = _allClaims.Where(c => c.Status == ClaimStatus.Approved);
            else if (filter == "Rejected")
                filtered = _allClaims.Where(c => c.Status == ClaimStatus.Rejected);
            else
                filtered = _allClaims;

            foreach (var claim in filtered)
                _filteredClaims.Add(claim);

            UpdateStats();
        }

        private void ClaimsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (claimsDataGrid.SelectedItem is ClaimForReview)
            {
                _selectedClaim = (ClaimForReview)claimsDataGrid.SelectedItem;
                ShowClaimDetails();
            }
        }

        private void ShowClaimDetails()
        {
            pnlClaimDetails.Visibility = Visibility.Collapsed;
            pnlDetailedInfo.Visibility = Visibility.Visible;
            pnlActions.Visibility = Visibility.Visible;

            txtDetailClaimId.Text = $"#{_selectedClaim.Id}";
            txtDetailLecturerName.Text = _selectedClaim.LecturerName;
            txtDetailEmail.Text = _selectedClaim.LecturerEmail;
            txtDetailDescription.Text = _selectedClaim.Description;
            txtDetailStatus.Text = _selectedClaim.Status.ToString();

            double progressValue = 0;
            if (_selectedClaim.Status == ClaimStatus.Submitted)
                progressValue = 1;
            else if (_selectedClaim.Status == ClaimStatus.UnderReview)
                progressValue = 2;
            else if (_selectedClaim.Status == ClaimStatus.Approved)
                progressValue = 3;
            else if (_selectedClaim.Status == ClaimStatus.Rejected)
                progressValue = 3;

            claimProgressBar.Value = progressValue;
        }

        private void ApproveClaim_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClaim == null) return;
            _selectedClaim.Status = ClaimStatus.Approved;
            RefreshUI("approved");
        }

        private void RejectClaim_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClaim == null) return;
            _selectedClaim.Status = ClaimStatus.Rejected;
            RefreshUI("rejected");
        }

        private void RefreshUI(string action)
        {
            ApplyFilter(((ComboBoxItem)cmbStatusFilter.SelectedItem)?.Content?.ToString() ?? "All Claims");
            ShowClaimDetails();
            MessageBox.Show($"Claim #{_selectedClaim.Id} has been {action}.", "Claim Updated", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (cmbStatusFilter.SelectedItem is ComboBoxItem)
            {
                var item = (ComboBoxItem)cmbStatusFilter.SelectedItem;
                ApplyFilter(item.Content.ToString());
            }
        }

        private void UpdateStats()
        {
            txtPendingCount.Text = _allClaims.Count(c => c.Status == ClaimStatus.Submitted).ToString();
            txtApprovedCount.Text = _allClaims.Count(c => c.Status == ClaimStatus.Approved).ToString();
            txtRejectedCount.Text = _allClaims.Count(c => c.Status == ClaimStatus.Rejected).ToString();
            txtPendingAmount.Text = $"R {_allClaims.Where(c => c.Status == ClaimStatus.Submitted).Sum(c => c.Amount):N2}";
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }
    }
}
