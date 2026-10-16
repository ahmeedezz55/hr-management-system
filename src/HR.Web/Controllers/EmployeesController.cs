using HR.Domain.Entities;
using HR.Infrastructure.Data;
using HR.Infrastructure.Identity;
using HR.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR.Web.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly ApplicationDbContext _db;

    public EmployeesController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Employees.Include(e => e.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e =>
                e.EmployeeNumber.Contains(search) ||
                e.FirstNameAr.Contains(search) ||
                e.LastNameAr.Contains(search) ||
                e.FirstNameEn.Contains(search) ||
                e.LastNameEn.Contains(search) ||
                (e.NationalId != null && e.NationalId.Contains(search)) ||
                (e.IqamaNumber != null && e.IqamaNumber.Contains(search)));
        }

        var employees = await query.OrderBy(e => e.EmployeeNumber).ToListAsync();
        ViewBag.Search = search;
        return View(employees);
    }

    public async Task<IActionResult> Details(int id)
    {
        var employee = await _db.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);

        return employee is null ? NotFound() : View(employee);
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Hr}")]
    public async Task<IActionResult> Create()
    {
        return View(await BuildFormAsync(new EmployeeFormViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Hr}")]
    public async Task<IActionResult> Create(EmployeeFormViewModel model)
    {
        if (!await ValidateIdentityAsync(model))
        {
            return View(await BuildFormAsync(model));
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model));
        }

        var employee = MapToEntity(model, new Employee());
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم إضافة الموظف بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Hr}")]
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        var model = MapToForm(employee);
        return View(await BuildFormAsync(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Hr}")]
    public async Task<IActionResult> Edit(int id, EmployeeFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var employee = await _db.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        if (!await ValidateIdentityAsync(model, id))
        {
            return View(await BuildFormAsync(model));
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model));
        }

        MapToEntity(model, employee);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم تحديث بيانات الموظف.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Hr}")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _db.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);

        return employee is null ? NotFound() : View(employee);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Hr}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        employee.IsDeleted = true;
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم حذف الموظف.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<EmployeeFormViewModel> BuildFormAsync(EmployeeFormViewModel model)
    {
        model.Departments = await _db.Departments
            .OrderBy(d => d.NameAr)
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = $"{d.NameAr} / {d.NameEn}",
                Selected = d.Id == model.DepartmentId
            })
            .ToListAsync();

        return model;
    }

    private async Task<bool> ValidateIdentityAsync(EmployeeFormViewModel model, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(model.NationalId) && string.IsNullOrWhiteSpace(model.IqamaNumber))
        {
            ModelState.AddModelError(nameof(model.NationalId), "أدخل رقم الهوية للمواطن أو رقم الإقامة للمقيم.");
            return false;
        }

        var duplicateNumber = await _db.Employees.AnyAsync(e =>
            e.EmployeeNumber == model.EmployeeNumber && e.Id != excludeId);
        if (duplicateNumber)
        {
            ModelState.AddModelError(nameof(model.EmployeeNumber), "رقم الموظف مستخدم مسبقاً.");
            return false;
        }

        return true;
    }

    private static EmployeeFormViewModel MapToForm(Employee e) => new()
    {
        Id = e.Id,
        EmployeeNumber = e.EmployeeNumber,
        FirstNameAr = e.FirstNameAr,
        LastNameAr = e.LastNameAr,
        FirstNameEn = e.FirstNameEn,
        LastNameEn = e.LastNameEn,
        NationalId = e.NationalId,
        IqamaNumber = e.IqamaNumber,
        Nationality = e.Nationality,
        Gender = e.Gender,
        DateOfBirth = e.DateOfBirth,
        MaritalStatus = e.MaritalStatus,
        Phone = e.Phone,
        Email = e.Email,
        Address = e.Address,
        City = e.City,
        HireDate = e.HireDate,
        JobTitleAr = e.JobTitleAr,
        JobTitleEn = e.JobTitleEn,
        BasicSalary = e.BasicSalary,
        GosiNumber = e.GosiNumber,
        BankIban = e.BankIban,
        Status = e.Status,
        DepartmentId = e.DepartmentId
    };

    private static Employee MapToEntity(EmployeeFormViewModel model, Employee e)
    {
        e.EmployeeNumber = model.EmployeeNumber;
        e.FirstNameAr = model.FirstNameAr;
        e.LastNameAr = model.LastNameAr;
        e.FirstNameEn = model.FirstNameEn;
        e.LastNameEn = model.LastNameEn;
        e.NationalId = model.NationalId;
        e.IqamaNumber = model.IqamaNumber;
        e.Nationality = model.Nationality;
        e.Gender = model.Gender;
        e.DateOfBirth = model.DateOfBirth;
        e.MaritalStatus = model.MaritalStatus;
        e.Phone = model.Phone;
        e.Email = model.Email;
        e.Address = model.Address;
        e.City = model.City;
        e.HireDate = model.HireDate;
        e.JobTitleAr = model.JobTitleAr;
        e.JobTitleEn = model.JobTitleEn;
        e.BasicSalary = model.BasicSalary;
        e.GosiNumber = model.GosiNumber;
        e.BankIban = model.BankIban;
        e.Status = model.Status;
        e.DepartmentId = model.DepartmentId;
        return e;
    }
}
