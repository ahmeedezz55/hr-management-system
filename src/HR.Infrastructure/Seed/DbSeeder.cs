using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Data;
using HR.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        if (db.Database.IsSqlite())
        {
            // SQLite fallback: create the database straight from the model
            // (SQL Server migrations are not applicable to the SQLite provider).
            await db.Database.EnsureCreatedAsync();
        }
        else if (db.Database.GetMigrations().Any())
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (!await db.Departments.AnyAsync())
        {
            db.Departments.AddRange(
                new Department { Code = "HR", NameAr = "الموارد البشرية", NameEn = "Human Resources" },
                new Department { Code = "IT", NameAr = "تقنية المعلومات", NameEn = "Information Technology" },
                new Department { Code = "FIN", NameAr = "المالية", NameEn = "Finance" },
                new Department { Code = "OPS", NameAr = "العمليات", NameEn = "Operations" });

            await db.SaveChangesAsync();
        }

        var hrDept = await db.Departments.FirstAsync(d => d.Code == "HR");

        if (!await db.Employees.AnyAsync())
        {
            db.Employees.Add(new Employee
            {
                EmployeeNumber = "EMP-1001",
                FirstNameAr = "أحمد",
                LastNameAr = "العلي",
                FirstNameEn = "Ahmed",
                LastNameEn = "Al-Ali",
                NationalId = "1000000001",
                Nationality = "Saudi",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1990, 5, 12),
                MaritalStatus = MaritalStatus.Married,
                Phone = "0500000001",
                Email = "admin@hr.sa",
                City = "Riyadh",
                HireDate = new DateTime(2020, 1, 1),
                JobTitleAr = "مدير النظام",
                JobTitleEn = "System Administrator",
                BasicSalary = 18000m,
                DepartmentId = hrDept.Id,
                Status = EmploymentStatus.Active
            });

            await db.SaveChangesAsync();
        }

        const string adminEmail = "admin@hr.sa";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            var employee = await db.Employees.FirstAsync(e => e.Email == adminEmail);
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = employee.FullNameEn,
                EmployeeId = employee.Id,
                PhoneNumber = employee.Phone,
                IsActive = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
                employee.UserId = admin.Id;
                await db.SaveChangesAsync();
            }
        }
    }
}
