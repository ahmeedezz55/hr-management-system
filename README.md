# نظام إدارة الموارد البشرية (Saudi HR Management System)

ASP.NET Core 8 MVC solution for HR operations in the Saudi market: bilingual Arabic/English UI, RTL layout, Identity roles, and Code First SQL Server.

## Architecture

```
HR.System.sln
├── src/HR.Domain              # Entities and enums (no infrastructure)
├── src/HR.Infrastructure      # EF Core, Identity, seeding, fluent config
└── src/HR.Web                 # MVC, views, localization, dashboard UI
```

## Features

- Authentication & roles: Admin, HR, Manager, Employee
- Employee CRUD (Arabic/English names, National ID / Iqama, GOSI, IBAN)
- Department management
- Attendance check-in / check-out
- Leave request / approve / reject (including Hajj leave)
- Dashboard statistics
- Default culture `ar-SA` with RTL Bootstrap 5 admin shell

## Run locally

1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and SQL Server (LocalDB or full instance).
2. Update `src/HR.Web/appsettings.json` connection string if needed.
3. From the repo root:

```bash
dotnet restore HR.System.sln
dotnet ef migrations add InitialCreate --project src/HR.Infrastructure --startup-project src/HR.Web
dotnet run --project src/HR.Web
```

If no migrations exist yet, startup uses `EnsureCreated()` so the database is still created on first run.

## Default login

- Email: `admin@hr.sa`
- Password: `Admin@123`

Change this password before any non-local deployment.

## Saudi-specific notes

- Identity vs Iqama is captured separately.
- Leave types include Hajj, maternity, and annual leave.
- UI defaults to Arabic RTL; use the EN toggle for LTR.
- Attendance late threshold is 08:30 (adjust in `AttendanceController` for your policy).
