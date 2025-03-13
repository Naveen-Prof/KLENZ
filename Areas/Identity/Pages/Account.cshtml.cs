using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KLENZ.Pages.Account
{
    [AllowAnonymous]
    public class AccountModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<AccountModel> _logger;

        public AccountModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<AccountModel> logger)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty(SupportsGet = false)]
        public LoginInputModel LoginModel { get; set; }

        [BindProperty(SupportsGet = false)]
        public RegisterInputModel RegisterModel { get; set; }


        [BindProperty(SupportsGet = false)]
        public ResetPasswordInputModel ResetPasswordModel { get; set; }  // Renamed to avoid conflict

        public string ReturnUrl { get; set; }
        public IList<Microsoft.AspNetCore.Authentication.AuthenticationScheme> ExternalLogins { get; set; }

        public class LoginInputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }

        public class RegisterInputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string UserName { get; set; }

            [EmailAddress]
            [Display(Name = "Email (Optional)")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public class ResetPasswordInputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string UserName { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (User.Identity.IsAuthenticated)
            {
                Response.Redirect(ReturnUrl);  // Prevent logged-in users from accessing login page
            }
        }

        public async Task<IActionResult> OnPostLoginAsync(string returnUrl = null)
        {
            if (string.IsNullOrEmpty(returnUrl) || returnUrl.Contains("Error"))
            {
                returnUrl = Url.Content("~/Home/Index"); // Force correct redirection
            }

            ModelState.Clear(); // Clear previous validation errors

            _logger.LogInformation($"Redirecting to: {returnUrl}");

            if (!TryValidateModel(LoginModel, nameof(LoginModel)))
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return Page();
            }

            var user = await _userManager.FindByNameAsync(LoginModel.UserName);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid username or password.";
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(user, LoginModel.Password, LoginModel.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in successfully.");
                TempData["SuccessMessage"] = "Login successful.";
                return Redirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                TempData["ErrorMessage"] = "Your account is locked. Please try again later.";
                return Page();
            }
            else if (result.IsNotAllowed)
            {
                TempData["ErrorMessage"] = "Login not allowed. Please verify your account.";
                return Page();
            }

            TempData["ErrorMessage"] = "Invalid username or password.";
            return Page();
        }



        public async Task<IActionResult> OnPostRegisterAsync(string returnUrl = null)
        {
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(RegisterModel.UserName) || string.IsNullOrWhiteSpace(RegisterModel.Password))
            {
                TempData["ErrorMessage"] = "Username and Password are required.";
                return Page();
            }

            var user = CreateUser();
            await _userStore.SetUserNameAsync(user, RegisterModel.UserName, CancellationToken.None);

            if (!string.IsNullOrEmpty(RegisterModel.Email))
            {
                await _emailStore.SetEmailAsync(user, RegisterModel.Email, CancellationToken.None);
            }

            var result = await _userManager.CreateAsync(user, RegisterModel.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User registered successfully.");
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["SuccessMessage"] = "User registered successfully.";
                return LocalRedirect("~/Home/Index");
            }

            TempData["ErrorMessage"] = string.Join("<br>", result.Errors.Select(e => e.Description));

            return Page();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync()
        {
            ModelState.Clear(); // Clear previous validation errors

            if (!TryValidateModel(ResetPasswordModel, nameof(ResetPasswordModel))) // Validate only ResetPasswordModel
            {
                return Page();
            }

            var user = await _userManager.Users
                .Where(u => u.NormalizedUserName == ResetPasswordModel.UserName.Trim().ToUpper())
                .FirstOrDefaultAsync();

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }

            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, "NewPassword@123");

            if (resetResult.Succeeded)
            {
                TempData["SuccessMessage"] = "Password reset to 'NewPassword@123'. Please log in and change it.";
                return RedirectToPage("./Account");
            }

            foreach (var error in resetResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }


        private IdentityUser CreateUser()
        {
            return new IdentityUser();
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("The default UI requires a user store with email support.");

            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
