using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Entities;

public class Employee : BaseEntity
{
    public string EmployeeNumber { get; set; } = string.Empty;

    public string FirstNameAr { get; set; } = string.Empty;
    public string LastNameAr { get; set; } = string.Empty;
    public string FirstNameEn { get; set; } = string.Empty;
    public string LastNameEn { get; set; } = string.Empty;

    /// <summary>Saudi National ID (رقم الهوية) for citizens.</summary>
    public string? NationalId { get; set; }

    /// <summary>Iqama number (رقم الإقامة) for expatriates.</summary>
    public string? IqamaNumber { get; set; }

    public string Nationality { get; set; } = "Saudi";
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Single;

    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }

    public DateTime HireDate { get; set; }
    public string JobTitleAr { get; set; } = string.Empty;
    public string JobTitleEn { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public string? GosiNumber { get; set; }
    public string? BankIban { get; set; }
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    /// <summary>Linked ASP.NET Identity user id, when the employee can sign in.</summary>
    public string? UserId { get; set; }

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    public string FullNameAr => $"{FirstNameAr} {LastNameAr}".Trim();
    public string FullNameEn => $"{FirstNameEn} {LastNameEn}".Trim();
}
