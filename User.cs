using System;

namespace ContractMonthlyClaimSystem2.Models
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class Users
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}
