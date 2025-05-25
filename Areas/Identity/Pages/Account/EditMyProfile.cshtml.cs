using KLENZ.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace KLENZ.Areas.Identity.Pages.Account
{
    public class EditMyProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public EditMyProfileModel(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        [BindProperty]
        public IFormFile? ProfilePicture { get; set; }

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string? PhoneNumber { get; set; }

        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [BindProperty]
        public string? Address { get; set; }

        public byte[]? CurrentProfilePicture { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            Name = user.UserName ?? "";
            Email = user.Email ?? "";
            PhoneNumber = user.PhoneNumber;
            DateOfBirth = user.DateOfBirth;
            Address = user.Address;
            CurrentProfilePicture = user.ProfilePicture;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ProfilePicture != null)
            {
                using var memoryStream = new MemoryStream();
                await ProfilePicture.CopyToAsync(memoryStream);
                user.ProfilePicture = memoryStream.ToArray();
            }

            user.UserName = Name;
            user.Email = Email;
            user.PhoneNumber = PhoneNumber;
            user.DateOfBirth = DateOfBirth;
            user.Address = Address;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }

            return RedirectToPage("MyProfile");
        }

    }
}
