using ForestChurches.Components.Email;
using ForestChurches.Components.Users;
using ForestChurches.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ForestChurches.Pages
{
    public class ContactModel : PageModel
    {
        // Dependency Injection
        private readonly iEmail _emailService;
        private readonly UserManager<ChurchAccount> _userManager;

        // Data Objects
        [BindProperty]
        public UserContact Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public ContactModel(iEmail emailService, UserManager<ChurchAccount> userManager)
        {
            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task OnPostAsync()
        {
            // User must be authenticated as this contact is ONLY for authenticated users, and will use their authenticated email address as the user contact.
            if (Input != null && User.Identity.IsAuthenticated)
            {
                var userData = new Dictionary<string, string>()
                {
                    { "{template_title}", "Forest Churches Contact Form" },
                    { "{template_content}", $"Name: {Input.Name} <br /><br /> Email: {User.Identity.Name} <br /><br /> Message: {Input.Message}" },
                    { "{template_button_name}", "View Messages" },
                    { "{template_link}", string.Empty }
                };

                await _emailService.StartEmailAsync("support@forestchurches.co.uk", userData, "Forest Churches Contact Form", "./templates/admin_email.html");

                StatusMessage = "Your message has been sent!";
            }

            else
            {
                StatusMessage = "Error: An error occurred while sending your message";
            }
        }

    }
}
