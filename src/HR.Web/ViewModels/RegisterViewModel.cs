using System.ComponentModel.DataAnnotations;

namespace HR.Web.ViewModels;

public class RegisterViewModel
{
    [Required, Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "تأكيد كلمة المرور")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
