using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ContractMonthlyClaimSystem2.Models
{
    // ============================================
    // ENUMS
    // ============================================
    public enum ClaimStatus
    {
        Draft,
        Submitted,
        UnderReview,
        Approved,
        Rejected,
        Paid
    }

    // ============================================
    // DOCUMENT MODEL
    // ============================================
    public class Document
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime UploadDate { get; set; }
    }

    // ============================================
    // CLAIM MODEL (Lecturer)
    // ============================================
    public class Claim : INotifyPropertyChanged
    {
        private int _id;
        private string _lecturerId;
        private string _lecturerName;
        private string _lecturerEmail;
        private decimal _amount;
        private DateTime _submissionDate;
        private ClaimStatus _status;
        private string _description;
        private decimal _hoursWorked;
        private decimal _hourlyRate;
        private List<Document> _documents = new List<Document>();

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string LecturerId
        {
            get => _lecturerId;
            set { _lecturerId = value; OnPropertyChanged(nameof(LecturerId)); }
        }

        public string LecturerName
        {
            get => _lecturerName;
            set { _lecturerName = value; OnPropertyChanged(nameof(LecturerName)); }
        }

        public string LecturerEmail
        {
            get => _lecturerEmail;
            set { _lecturerEmail = value; OnPropertyChanged(nameof(LecturerEmail)); }
        }

        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(nameof(Amount)); }
        }

        public decimal HoursWorked
        {
            get => _hoursWorked;
            set { _hoursWorked = value; OnPropertyChanged(nameof(HoursWorked)); OnPropertyChanged(nameof(Amount)); }
        }

        public decimal HourlyRate
        {
            get => _hourlyRate;
            set { _hourlyRate = value; OnPropertyChanged(nameof(HourlyRate)); OnPropertyChanged(nameof(Amount)); }
        }

        public DateTime SubmissionDate
        {
            get => _submissionDate;
            set { _submissionDate = value; OnPropertyChanged(nameof(SubmissionDate)); }
        }

        public ClaimStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusProgress));
            }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public List<Document> Documents
        {
            get => _documents;
            set { _documents = value; OnPropertyChanged(nameof(Documents)); }
        }

        public int DocumentCount => Documents?.Count ?? 0;

        // Progress bar binding helper
        public int StatusProgress
        {
            get
            {
                switch (Status)
                {
                    case ClaimStatus.Draft: return 10;
                    case ClaimStatus.Submitted: return 30;
                    case ClaimStatus.UnderReview: return 50;
                    case ClaimStatus.Approved: return 75;
                    case ClaimStatus.Paid: return 100;
                    case ClaimStatus.Rejected: return 100;
                    default: return 0;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ============================================
    // CLAIM REVIEW MODEL (Coordinator/Manager)
    // ============================================
    public class ClaimForReview
    {
        public int Id { get; set; }
        public string LecturerName { get; set; }
        public string LecturerEmail { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal Amount { get; set; }
        public ClaimStatus Status { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Description { get; set; }
        public int DocumentCount { get; set; }
        public string ReviewedBy { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string RejectionReason { get; set; }
        public string AdditionalInfoRequest { get; set; }
    }

    // ============================================
    // USER MODEL
    // ============================================
    public class User
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }
}
