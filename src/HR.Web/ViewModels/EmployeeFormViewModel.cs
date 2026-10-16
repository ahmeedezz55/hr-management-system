using System.ComponentModel.DataAnnotations;
using HR.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HR.Web.ViewModels;

public class EmployeeFormViewModel
{
    public int Id { get; set; }

    [Required, Display(Name = "رقم الموظف"), StringLength(20)]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required, Display(Name = "الاسم الأول (عربي)"), StringLength(80)]
    public string FirstNameAr { get; set; } = string.Empty;

    [Required, Display(Name = "اسم العائلة (عربي)"), StringLength(80)]
    public string LastNameAr { get; set; } = string.Empty;

    [Required, Display(Name = "First Name (EN)"), StringLength(80)]
    public string FirstNameEn { get; set; } = string.Empty;

    [Required, Display(Name = "Last Name (EN)"), StringLength(80)]
    public string LastNameEn { get; set; } = string.Empty;

    [Display(Name = "رقم الهوية"), StringLength(10)]
    public string? NationalId { get; set; }

    [Display(Name = "رقم الإقامة"), StringLength(15)]
    public string? IqamaNumber { get; set; }

    [Required, Display(Name = "الجنسية")]
    public string Nationality { get; set; } = "Saudi";

    [Required, Display(Name = "الجنس")]
    public Gender Gender { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "تاريخ الميلاد")]
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-25);

    [Required, Display(Name = "الحالة الاجتماعية")]
    public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Single;

    [Display(Name = "الجوال"), Phone]
    public string? Phone { get; set; }

    [Display(Name = "البريد الإلكتروني"), EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [Display(Name = "المدينة")]
    public string? City { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "تاريخ التعيين")]
    public DateTime HireDate { get; set; } = DateTime.Today;

    [Required, Display(Name = "المسمى الوظيفي (عربي)")]
    public string JobTitleAr { get; set; } = string.Empty;

    [Required, Display(Name = "Job Title (EN)")]
    public string JobTitleEn { get; set; } = string.Empty;

    [Required, Display(Name = "الراتب الأساسي"), Range(0, 1_000_000)]
    public decimal BasicSalary { get; set; }

    [Display(Name = "رقم التأمينات (GOSI)")]
    public string? GosiNumber { get; set; }

    [Display(Name = "الآيبان")]
    public string? BankIban { get; set; }

    [Required, Display(Name = "حالة التوظيف")]
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;

    [Required, Display(Name = "القسم")]
    public int DepartmentId { get; set; }

    public IEnumerable<SelectListItem> Departments { get; set; } = [];
}
