namespace HR.Infrastructure.Identity;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Hr = "HR";
    public const string Manager = "Manager";
    public const string Employee = "Employee";

    public static readonly string[] All = [Admin, Hr, Manager, Employee];
}
