using System.ComponentModel.DataAnnotations;
using HR.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HR.Web.ViewModels;

public class LeaveFormViewModel
{
    [Required, Display(Name = "الموظف")]
    public int EmployeeId { get; set; }

    [Required, Display(Name = "نوع الإجازة")]
    public LeaveType LeaveType { get; set; } = LeaveType.Annual;

    [Required, DataType(DataType.Date), Display(Name = "من تاريخ")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required, DataType(DataType.Date), Display(Name = "إلى تاريخ")]
    public DateTime EndDate { get; set; } = DateTime.Today;

    [Required, Display(Name = "السبب"), StringLength(500)]
    public string Reason { get; set; } = string.Empty;

    public IEnumerable<SelectListItem> Employees { get; set; } = [];
}
