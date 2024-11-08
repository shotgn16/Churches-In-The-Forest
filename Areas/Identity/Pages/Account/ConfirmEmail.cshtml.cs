// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using ForestChurches.Components.Email;
using ForestChurches.Components.Users;

namespace ForestChurches.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ChurchAccount> _userManager;
        private readonly iEmail _mailRepository;

        public ConfirmEmailModel(UserManager<ChurchAccount> userManager, iEmail mailRepository)
        {
            _userManager = userManager;
            _mailRepository = mailRepository;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var usr = await _userManager.FindByIdAsync(userId);

            var userData = new Dictionary<string, string>()
                {
                    { "{username}", usr.UserName },
                    { "{user_account_link}", "https://forestchurches.co.uk/Identity/Account/Manage" }
                };

            await _mailRepository.StartEmailAsync(usr.Email, userData, "Thanks for registering", "./templates/registration_complete.html");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            return Page();
        }
    }
}
