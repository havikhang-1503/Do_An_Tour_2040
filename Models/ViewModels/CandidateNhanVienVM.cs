using System.ComponentModel.DataAnnotations;

namespace Tour_2040.Models;

public class CandidateNhanVienVM
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
