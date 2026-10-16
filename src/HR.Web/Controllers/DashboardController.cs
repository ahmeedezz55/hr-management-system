using HR.Domain.Enums;
using HR.Infrastructure.Data;
using HR.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var model = new DashboardViewModel
        {
            TotalEmployees = await _db.Employees.CountAsync(),
            ActiveEmployees = await _db.Employees.CountAsync(e => e.Status == EmploymentStatus.Active),
            TotalDepartments = await _db.Departments.CountAsync(),
            PresentToday = await _db.Attendances.CountAsync(a =>
                a.WorkDate == today && a.CheckIn != null &&
                (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late)),
            PendingLeaves = await _db.LeaveRequests.CountAsync(l => l.Status == LeaveStatus.Pending),
            ApprovedLeavesThisMonth = await _db.LeaveRequests.CountAsync(l =>
                l.Status == LeaveStatus.Approved && l.StartDate >= monthStart)
        };

        return View(model);
    }

    [AllowAnonymous]
    public IActionResult Error() => View();
}
