using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Data;
using HR.Infrastructure.Identity;
using HR.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR.Web.Controllers;

[Authorize]
public class LeavesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public LeavesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var query = _db.LeaveRequests.Include(l => l.Employee).AsQueryable();

        if (!User.IsInRole(AppRoles.Admin) && !User.IsInRole(AppRoles.Hr) && !User.IsInRole(AppRoles.Manager))
        {
            var user = await _userManager.GetUserAsync(User);
            query = query.Where(l => l.EmployeeId == user!.EmployeeId);
        }

        var leaves = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
        return View(leaves);
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildFormAsync(new LeaveFormViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveFormViewModel model)
    {
        if (model.EndDate < model.StartDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "تاريخ النهاية يجب أن يكون بعد تاريخ البداية.");
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model));
        }

        var totalDays = (model.EndDate.Date - model.StartDate.Date).Days + 1;
        _db.LeaveRequests.Add(new LeaveRequest
        {
            EmployeeId = model.EmployeeId,
            LeaveType = model.LeaveType,
            StartDate = model.StartDate.Date,
            EndDate = model.EndDate.Date,
            TotalDays = totalDays,
            Reason = model.Reason,
            Status = LeaveStatus.Pending
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "تم إرسال طلب الإجازة.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Hr},{AppRoles.Manager}")]
    public async Task<IActionResult> Approve(int id, string? notes)
    {
        return await ReviewAsync(id, LeaveStatus.Approved, notes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Hr},{AppRoles.Manager}")]
    public async Task<IActionResult> Reject(int id, string? notes)
    {
        return await ReviewAsync(id, LeaveStatus.Rejected, notes);
    }

    private async Task<IActionResult> ReviewAsync(int id, LeaveStatus status, string? notes)
    {
        var leave = await _db.LeaveRequests.FindAsync(id);
        if (leave is null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        leave.Status = status;
        leave.ReviewerUserId = user?.Id;
        leave.ReviewedAt = DateTime.UtcNow;
        leave.ReviewNotes = notes;
        await _db.SaveChangesAsync();

        TempData["Success"] = status == LeaveStatus.Approved ? "تم اعتماد الإجازة." : "تم رفض الإجازة.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<LeaveFormViewModel> BuildFormAsync(LeaveFormViewModel model)
    {
        var employees = _db.Employees.OrderBy(e => e.EmployeeNumber).AsQueryable();
        var user = await _userManager.GetUserAsync(User);

        if (!User.IsInRole(AppRoles.Admin) && !User.IsInRole(AppRoles.Hr) && user?.EmployeeId is not null)
        {
            employees = employees.Where(e => e.Id == user.EmployeeId);
            model.EmployeeId = user.EmployeeId.Value;
        }

        model.Employees = await employees
            .Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.EmployeeNumber} - {e.FirstNameAr} {e.LastNameAr}"
            })
            .ToListAsync();

        return model;
    }
}
