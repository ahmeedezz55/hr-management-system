using System.ComponentModel.DataAnnotations;

namespace HR.Web.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress, Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "تذكرني")]
    public bool RememberMe { get; set; }
}
