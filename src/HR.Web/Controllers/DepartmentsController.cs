using HR.Domain.Entities;
using HR.Infrastructure.Data;
using HR.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Web.Controllers;

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Hr},{AppRoles.Manager}")]
public class DepartmentsController : Controller
{
    private readonly ApplicationDbContext _db;

    public DepartmentsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var departments = await _db.Departments
            .Include(d => d.Employees)
            .OrderBy(d => d.Code)
            .ToListAsync();

        return View(departments);
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Hr}")]
    public IActionResult Create() => View(new Department());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Hr}")]
    public async Task<IActionResult> Create(Department model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _db.Departments.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم إضافة القسم بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Hr}")]
    public async Task<IActionResult> Edit(int id)
    {
        var department = await _db.Departments.FindAsync(id);
        return department is null ? NotFound() : View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Hr}")]
    public async Task<IActionResult> Edit(int id, Department model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var department = await _db.Departments.FindAsync(id);
        if (department is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        department.Code = model.Code;
        department.NameAr = model.NameAr;
        department.NameEn = model.NameEn;
        department.DescriptionAr = model.DescriptionAr;
        department.DescriptionEn = model.DescriptionEn;
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم تحديث القسم.";
        return RedirectToAction(nameof(Index));
    }
}
