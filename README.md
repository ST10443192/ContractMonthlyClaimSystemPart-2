# 🧾 Contract Monthly Claim System

A **desktop-based application** built in **C# (WPF)** for managing and automating monthly payment claims for contract lecturers.  
This system provides a secure, role-based workflow that enables lecturers to submit their claims and allows coordinators, managers, and administrators to review, approve, and manage them efficiently.

---

## 🚀 Features

- **User Authentication** (Lecturer, Coordinator, Manager, Administrator)
- **Claim Submission & Tracking**
- **Approval Workflow** (Multi-level verification)
- **SQLite Database Integration**
- **Dashboard Interface** for each role
- **Data Validation** and error handling
- **Audit Trail** for submission history
- **Modern WPF UI** with gradient themes and hover effects
- **Password Security** with SHA-256 hashing
- **Offline Storage** (Portable local SQLite database)

---

## 🧱 System Architecture

The application follows a **modular architecture**:


---

## 🧩 Technologies Used

| Component | Technology |
|------------|-------------|
| Language | C# (.NET 8 / .NET Framework 4.8) |
| Framework | WPF (Windows Presentation Foundation) |
| Database | SQLite (via `Microsoft.Data.Sqlite`) |
| UI | XAML with custom styles |
| IDE | Visual Studio 2022 or newer |

---

## ⚙️ Installation & Setup

Follow these steps to get the system running locally.

### 1️⃣ Clone the Repository
```bash
git clone https://github.com/yourusername/ContractMonthlyClaimSystem.git
cd ContractMonthlyClaimSystem
Login using a valid user account.

Lecturer submits monthly work claims with details such as:

Hours worked

Hourly rate

Description of work

Coordinator reviews the lecturer’s submission.

Manager approves or rejects claims based on policies.

Administrator manages all users and database records.
