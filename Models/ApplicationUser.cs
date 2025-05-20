using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? ProfilePicturePath { get; set; }
    public byte? IsAdmin { get; set; }
}
