using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Data;
using HR.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Web.Controllers;

[Authorize]
public class AttendanceController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AttendanceController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(DateTime? date)
    {
        var workDate = (date ?? DateTime.Today).Date;
        var records = await _db.Attendances
            .Include(a => a.Employee)
            .Where(a => a.WorkDate == workDate)
            .OrderBy(a => a.Employee.EmployeeNumber)
            .ToListAsync();

        ViewBag.WorkDate = workDate;
        return View(records);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn()
    {
        var employee = await CurrentEmployeeAsync();
        if (employee is null)
        {
            TempData["Error"] = "حسابك غير مرتبط بملف موظف.";
            return RedirectToAction(nameof(Index));
        }

        var today = DateTime.Today;
        var record = await _db.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.WorkDate == today);

        if (record?.CheckIn != null)
        {
            TempData["Error"] = "تم تسجيل الحضور مسبقاً اليوم.";
            return RedirectToAction(nameof(Index), new { date = today });
        }

        var now = DateTime.Now;
        var lateThreshold = today.AddHours(8).AddMinutes(30);

        if (record is null)
        {
            record = new Attendance
            {
                EmployeeId = employee.Id,
                WorkDate = today,
                CheckIn = now,
                Status = now > lateThreshold ? AttendanceStatus.Late : AttendanceStatus.Present
            };
            _db.Attendances.Add(record);
        }
        else
        {
            record.CheckIn = now;
            record.Status = now > lateThreshold ? AttendanceStatus.Late : AttendanceStatus.Present;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "تم تسجيل الحضور.";
        return RedirectToAction(nameof(Index), new { date = today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut()
    {
        var employee = await CurrentEmployeeAsync();
        if (employee is null)
        {
            TempData["Error"] = "حسابك غير مرتبط بملف موظف.";
            return RedirectToAction(nameof(Index));
        }

        var today = DateTime.Today;
        var record = await _db.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.WorkDate == today);

        if (record?.CheckIn is null)
        {
            TempData["Error"] = "سجّل الحضور أولاً.";
            return RedirectToAction(nameof(Index), new { date = today });
        }

        if (record.CheckOut != null)
        {
            TempData["Error"] = "تم تسجيل الانصراف مسبقاً.";
            return RedirectToAction(nameof(Index), new { date = today });
        }

        record.CheckOut = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم تسجيل الانصراف.";
        return RedirectToAction(nameof(Index), new { date = today });
    }

    private async Task<Employee?> CurrentEmployeeAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.EmployeeId is null)
        {
            return null;
        }

        return await _db.Employees.FindAsync(user.EmployeeId.Value);
    }
}
