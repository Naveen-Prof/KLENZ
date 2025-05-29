using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using KLENZ.Data;

namespace KLENZ.ViewComponents
{
    public class UserProfileViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserProfileViewComponent(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            string profileImageBase64;

            if (user?.ProfilePicture != null && user.ProfilePicture.Length > 0)
            {
                profileImageBase64 = $"data:image/png;base64,{Convert.ToBase64String(user.ProfilePicture)}";
            }
            else
            {
                profileImageBase64 = Url.Content("~/images/profile.png"); // fallback to default
            }

            return View("Default", profileImageBase64);
        }
    }
}
