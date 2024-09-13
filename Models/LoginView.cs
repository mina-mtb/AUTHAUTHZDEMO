using System.ComponentModel.DataAnnotations;

namespace AuthAuthzDemo.Models;
public class LoginView
{
    [Required]
    public string? UserName { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

}