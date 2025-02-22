using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace KLENZ.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl ?? "~/");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }

        // 🔹 Reset Password Without Email Verification
        public async Task<IActionResult> OnPostResetPasswordAsync()
        {
            if (string.IsNullOrEmpty(Input.Email))
            {
                ModelState.AddModelError(string.Empty, "Email is required to reset the password.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "No user found with this email.");
                return Page();
            }

            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, "NewPassword@123");

            if (resetResult.Succeeded)
            {
                TempData["SuccessMessage"] = "Password has been reset to 'NewPassword@123'. Please log in and change it.";
                return RedirectToPage("./Login");
            }

            foreach (var error in resetResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}





//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//#nullable disable

//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.Extensions.Logging;

//namespace KLENZ.Areas.Identity.Pages.Account
//{
//    public class LoginModel : PageModel
//    {
//        private readonly SignInManager<IdentityUser> _signInManager;
//        private readonly ILogger<LoginModel> _logger;

//        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger)
//        {
//            _signInManager = signInManager;
//            _logger = logger;
//        }

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        [BindProperty]
//        public InputModel Input { get; set; }

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        public IList<AuthenticationScheme> ExternalLogins { get; set; }

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        public string ReturnUrl { get; set; }

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        [TempData]
//        public string ErrorMessage { get; set; }

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        public class InputModel
//        {
//            /// <summary>
//            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//            ///     directly from your code. This API may change or be removed in future releases.
//            /// </summary>
//            [Required]
//            [EmailAddress]
//            public string Email { get; set; }

//            /// <summary>
//            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//            ///     directly from your code. This API may change or be removed in future releases.
//            /// </summary>
//            [Required]
//            [DataType(DataType.Password)]
//            public string Password { get; set; }

//            /// <summary>
//            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//            ///     directly from your code. This API may change or be removed in future releases.
//            /// </summary>
//            [Display(Name = "Remember me?")]
//            public bool RememberMe { get; set; }
//        }

//        public async Task OnGetAsync(string returnUrl = null)
//        {
//            if (!string.IsNullOrEmpty(ErrorMessage))
//            {
//                ModelState.AddModelError(string.Empty, ErrorMessage);
//            }

//            returnUrl ??= Url.Content("~/");

//            // Clear the existing external cookie to ensure a clean login process
//            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

//            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

//            ReturnUrl = returnUrl;
//        }

//        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
//        {
//            returnUrl ??= Url.Content("~/Home/Index"); // Redirect to Home after login

//            if (!ModelState.IsValid)
//            {
//                return Page();
//            }

//            var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);
//            if (user == null)
//            {
//                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
//                return Page();
//            }

//            var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

//            if (result.Succeeded)
//            {
//                return LocalRedirect(returnUrl); // ✅ Redirect to Home page
//            }
//            else
//            {
//                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
//                return Page();
//            }
//        }


//        //public async Task<IActionResult> OnPostAsync(string returnUrl = null)
//        //{
//        //    returnUrl ??= Url.Content("~/Home/Index"); // ✅ Ensures redirection to Home page

//        //    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

//        //    if (ModelState.IsValid)
//        //    {
//        //        var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);
//        //        if (user == null)
//        //        {
//        //            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
//        //            return Page();
//        //        }

//        //        var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
//        //        if (result.Succeeded)
//        //        {
//        //            _logger.LogInformation("User logged in.");
//        //            return LocalRedirect(returnUrl); // ✅ Redirects to Home page
//        //        }
//        //        else
//        //        {
//        //            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
//        //            return Page();
//        //        }
//        //    }

//        //    return Page();
//        //}


//    }
//}
