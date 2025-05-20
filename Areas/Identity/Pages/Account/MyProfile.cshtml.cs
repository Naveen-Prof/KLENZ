using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KLENZ.Areas.Identity.Pages.Account
{
    public class MyProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public MyProfileModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string? ProfilePicturePath { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    Name = user.UserName ?? "";
                    Email = user.Email ?? "";
                    PhoneNumber = user.PhoneNumber;
                    DateOfBirth = user.DateOfBirth;
                    Address = user.Address;
                    ProfilePicturePath = user.ProfilePicturePath ?? "/images/default-profile.png";
                }
            }
        }
    }
}
