using System;
using System.Data;
using Microsoft.Data.Sqlite;
using System.IO;
using ContractMonthlyClaimSystem2.Models;
using ContractMonthlyClaimSystem2.Helpers;

namespace ContractMonthlyClaimSystem2.Database
{
    public class DatabaseManager
    {
        private static readonly Lazy<DatabaseManager> _instance = new Lazy<DatabaseManager>(() => new DatabaseManager());
        public static DatabaseManager Instance => _instance.Value;

        private readonly string connectionString;
        private readonly string dbFolder;
        private readonly string dbPath;

        private DatabaseManager()
        {
            // ✅ Use stable shared folder path (accessible to all users)
            dbFolder = @"C:\Users\Public\Documents\ContractMonthlyClaimSystem2\Database";
            dbPath = Path.Combine(dbFolder, "claims.db");

            // ✅ Ensure folder exists
            if (!Directory.Exists(dbFolder))
                Directory.CreateDirectory(dbFolder);

            // ✅ SQLite connection string
            connectionString = $"Data Source={dbPath};";

            // ✅ Ensure tables and default users
            EnsureDatabase();
            InitializeDefaultUsers();
        }

        // ✅ Ensure database and tables exist
        public void EnsureDatabase()
        {
            if (!File.Exists(dbPath))
            {
                File.Create(dbPath).Close();
            }

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                string createUsers = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Email TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        FullName TEXT,
                        Role TEXT,
                        IsActive INTEGER DEFAULT 1,
                        CreatedDate TEXT,
                        LastLogin TEXT
                    );";

                string createClaims = @"
                    CREATE TABLE IF NOT EXISTS Claims (
                        ClaimId INTEGER PRIMARY KEY AUTOINCREMENT,
                        LecturerEmail TEXT,
                        LecturerName TEXT,
                        Amount REAL,
                        Status TEXT,
                        SubmissionDate TEXT,
                        Description TEXT,
                        HoursWorked REAL,
                        HourlyRate REAL,
                        DocumentCount INTEGER
                    );";

                using (var cmd = new SqliteCommand(createUsers, conn))
                    cmd.ExecuteNonQuery();

                using (var cmd = new SqliteCommand(createClaims, conn))
                    cmd.ExecuteNonQuery();
            }
        }

        // ✅ Authenticate a user
        public Users AuthenticateUser(string email, string password)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT UserId, Email, FullName, Role, IsActive, PasswordHash 
                                 FROM Users 
                                 WHERE Email = @Email AND IsActive = 1 LIMIT 1";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader.IsDBNull(5) ? null : reader.GetString(5);

                            if (!string.IsNullOrEmpty(storedHash) &&
                                PasswordHelper.VerifyPassword(password, storedHash))
                            {
                                var user = new Users
                                {
                                    UserId = reader.GetInt32(0),
                                    Email = reader.GetString(1),
                                    FullName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                    Role = reader.IsDBNull(3) ? "Lecturer" : reader.GetString(3),
                                    IsActive = reader.IsDBNull(4) ? true : reader.GetInt32(4) == 1
                                };

                                UpdateLastLogin(connection, user.UserId);
                                return user;
                            }
                        }
                    }
                }
            }
            return null;
        }

        // ✅ Update last login timestamp
        private void UpdateLastLogin(SqliteConnection connection, int userId)
        {
            string update = "UPDATE Users SET LastLogin = @LastLogin WHERE UserId = @UserId";
            using (var cmd = new SqliteCommand(update, connection))
            {
                cmd.Parameters.AddWithValue("@LastLogin", DateTime.UtcNow.ToString("o"));
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.ExecuteNonQuery();
            }
        }

        // ✅ Create a new user
        public bool CreateUser(string email, string password, string fullName, string role)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string hashedPassword = PasswordHelper.HashPassword(password);

                string query = @"
                    INSERT INTO Users (Email, PasswordHash, FullName, Role, IsActive, CreatedDate)
                    VALUES (@Email, @PasswordHash, @FullName, @Role, 1, @CreatedDate)";

                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    cmd.Parameters.AddWithValue("@FullName", fullName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Role", string.IsNullOrWhiteSpace(role) ? "Lecturer" : role);
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow.ToString("o"));

                    try
                    {
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                    catch (SqliteException)
                    {
                        return false;
                    }
                }
            }
        }

        // ✅ Change password
        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT PasswordHash FROM Users WHERE UserId = @UserId";
                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    var obj = cmd.ExecuteScalar();
                    string storedHash = obj?.ToString();

                    if (string.IsNullOrEmpty(storedHash) || !PasswordHelper.VerifyPassword(oldPassword, storedHash))
                        return false;
                }

                string newHash = PasswordHelper.HashPassword(newPassword);
                string updateQuery = "UPDATE Users SET PasswordHash = @NewHash WHERE UserId = @UserId";
                using (var cmd = new SqliteCommand(updateQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@NewHash", newHash);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        // ✅ Save claim
        public void SaveClaim(Claim claim)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    INSERT INTO Claims (LecturerEmail, LecturerName, Amount, Status, SubmissionDate, Description, HoursWorked, HourlyRate, DocumentCount)
                    VALUES (@LecturerEmail, @LecturerName, @Amount, @Status, @SubmissionDate, @Description, @HoursWorked, @HourlyRate, @DocumentCount)";

                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@LecturerEmail", claim.LecturerEmail ?? string.Empty);
                    cmd.Parameters.AddWithValue("@LecturerName", claim.LecturerName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Amount", Convert.ToDouble(claim.Amount));
                    cmd.Parameters.AddWithValue("@Status", claim.Status.ToString());
                    cmd.Parameters.AddWithValue("@SubmissionDate", claim.SubmissionDate.ToString("o"));
                    cmd.Parameters.AddWithValue("@Description", claim.Description ?? string.Empty);
                    cmd.Parameters.AddWithValue("@HoursWorked", Convert.ToDouble(claim.HoursWorked));
                    cmd.Parameters.AddWithValue("@HourlyRate", Convert.ToDouble(claim.HourlyRate));
                    cmd.Parameters.AddWithValue("@DocumentCount", claim.DocumentCount);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ✅ Initialize default users
        public void InitializeDefaultUsers()
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string checkQuery = "SELECT COUNT(*) FROM Users";
                    using (var cmd = new SqliteCommand(checkQuery, connection))
                    {
                        long count = (long)cmd.ExecuteScalar();
                        if (count > 0)
                            return;
                    }
                }

                CreateUser("admin@university.ac.za", "Admin@123", "System Administrator", "Admin");
                CreateUser("lecturer@university.ac.za", "Lecturer@123", "Dr. John Lecturer", "Lecturer");
                CreateUser("coordinator@university.ac.za", "Coordinator@123", "Academic Coordinator", "Coordinator");
                CreateUser("manager@university.ac.za", "Manager@123", "Programme Manager", "Manager");
            }
            catch
            {
                // Ignore initialization errors
            }
        }
    }
}
