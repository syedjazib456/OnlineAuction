// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Online_Auction.Models;

namespace Online_Auction.Areas.Identity.Pages.Account
{
    public class RegisterModelFront : PageModel
    {
        private readonly SignInManager<Register> _signInManager;
        private readonly UserManager<Register> _userManager;
        private readonly IUserStore<Register> _userStore;
        private readonly IUserEmailStore<Register> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RegisterModelFront(
            UserManager<Register> userManager,
            IUserStore<Register> userStore,
            SignInManager<Register> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment webHostEnvironment
            )
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {

            [Required]
            [Column(TypeName = "varchar(100)")]
            public string FullName { get; set; }


            [Required]
            [Column(TypeName = "varchar(max)")]
            public string Address { get; set; }


            [Required]
            [Column(TypeName = "varchar(10)")]
            public string Status { get; set; } = "active";

            [NotMapped]
            public IFormFile ProfileImage { get; set; }

            [Column(TypeName = "varchar(max)")]
            public string ProfilePicture { get; set; }

            [Required]
            [Column(TypeName = "varchar(15)")]
            public string number { get; set; }


            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }


            public string Role {  get; set; }

            public IEnumerable<SelectListItem> RoleList { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!_roleManager.RoleExistsAsync(Roles.Role_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Roles.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Roles.Role_User)).GetAwaiter().GetResult();
            }
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            Input = new InputModel()
            {
                RoleList = _roleManager.Roles.Select(x=>x.Name).Select(y=>new SelectListItem()
                {
                    Text = y,
                    Value = y
                })
            };
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.Address = Input.Address;
                user.FullName = Input.FullName;
                user.PhoneNumber = Input.number;
                string filename = "profilepicturplaceholder.png";
                if (Input.ProfileImage != null)
                {
                    string uploadfolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    filename = Guid.NewGuid().ToString() + " " + Input.ProfileImage.FileName;
                    string filepath = Path.Combine(uploadfolder, filename);
                    string extension = Path.GetExtension(Input.ProfileImage.FileName);
                    if (extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png" || extension.ToLower() == ".webp")
                    {
                        if (Input.ProfileImage.Length <= 104857600)
                        {
                            Input.ProfileImage.CopyTo(new FileStream(filepath, FileMode.Create));
                        }
                        else
                        {
                            TempData["error"] = "File size exceeds the limit (100MB)";
                        }
                    }
                    else
                    {
                        TempData["error"] = "Invalid file format";
                    }
                }
                user.ProfilePicture = filename;
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    if(Input.Role == null)
                    {
                        _userManager.AddToRoleAsync(user, Roles.Role_User);
                    }
                    else
                    {
                        _userManager.AddToRoleAsync(user, Input.Role);
                    }
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail_Front",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation_Front", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }

                    //RedirectToPage("Login");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            //else
            //{
            //    var errors = ModelState
            //                    .Where(x => x.Value.Errors.Count > 0)
            //                    .Select(x => new { x.Key, x.Value.Errors })
            //                    .ToArray();

            //    var errorMessages = errors.SelectMany(e => e.Errors.Select(er => er.ErrorMessage)).ToList();

            //    var errorMessageString = string.Join(", ", errorMessages);

            //    Console.WriteLine(errorMessageString);

            //    ViewData["ErrorMessage"] = errorMessageString;
            //    return Page();
            //}

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private Register CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Register>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<Register> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<Register>)_userStore;
        }
    }
}
