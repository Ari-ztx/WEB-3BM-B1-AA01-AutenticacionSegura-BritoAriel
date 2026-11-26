using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Deber_AutenticacionSegura.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string Fullname { get; set; } =
        string.Empty;
    
    public bool IsEnabled { get; set; } = true;
}
