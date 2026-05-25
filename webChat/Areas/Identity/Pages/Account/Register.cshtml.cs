// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using webChat.Models;

namespace webChat.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string UserName { get; set; } = "";

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var normalizedEmail = Input.Email.ToUpper();

                var emailAlreadyExists = await _userManager.Users
                    .AnyAsync(u => u.NormalizedEmail == normalizedEmail);

                if (emailAlreadyExists)
                {
                    ModelState.AddModelError(string.Empty, "An account with this email already exists.");
                    return Page();
                }

                var normalizedUserName = Input.UserName.ToUpper();

                var usernameAlreadyExists = await _userManager.Users
                    .AnyAsync(u => u.NormalizedUserName == normalizedUserName);

                if (usernameAlreadyExists)
                {
                    ModelState.AddModelError(string.Empty, "An account with this username already exists.");
                    return Page();
                }

                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);

                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // Avatar default
                user.ProfileImageUrl = "/images/defaults/default-avatar.png";

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new
                        {
                            area = "Identity",
                            userId,
                            code,
                            returnUrl
                        },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Confirm your Talkr account",

                        $@"
<div style='font-family:Arial,sans-serif;background-color:#f5f5f5;padding:30px;'>

    <div style='max-width:600px;
                margin:auto;
                background:white;
                border-radius:12px;
                padding:35px;
                box-shadow:0 2px 10px rgba(0,0,0,0.08);'>

        <h1 style='color:#5aa04e;
                   margin-top:0;
                   margin-bottom:10px;'>
            Welcome to Talkr 👋
        </h1>

        <p style='font-size:16px;
                  color:#333;
                  line-height:1.6;'>
            Thanks for creating your account.
            Please confirm your email to start using Talkr.
        </p>

        <div style='margin:35px 0;'>

            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
               style='background-color:#5aa04e;
                      color:white;
                      padding:14px 24px;
                      text-decoration:none;
                      border-radius:8px;
                      font-weight:bold;
                      display:inline-block;
                      font-size:15px;'>

                Confirm Email

            </a>

        </div>

        <p style='font-size:13px;
                  color:#777;
                  margin-bottom:8px;'>

            If the button does not work, copy and paste this link into your browser:

        </p>

        <p style='font-size:13px;
                  word-break:break-all;'>

            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>

                {HtmlEncoder.Default.Encode(callbackUrl)}

            </a>

        </p>

        <hr style='border:none;
                   border-top:1px solid #eee;
                   margin:30px 0;' />

        <p style='font-size:12px;
                  color:#999;
                  line-height:1.5;'>

            If you did not create this account,
            you can safely ignore this email.

        </p>

    </div>

</div>"
                    );

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage(
                            "RegisterConfirmation",
                            new
                            {
                                email = Input.Email,
                                returnUrl
                            });
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException(
                    $"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException(
                    "The default UI requires a user store with email support.");
            }

            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}