using ForestChurches.Components.Users;
using ForestChurches.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ForestChurches.Pages.Admin.Users
{
    [Authorize(Policy = "UserManagement.Read")]
    public class IndexModel : PageModel
    {
        private ForestChurchesContext _context;
        private ILogger<IndexModel> _logger;

        private UserManager<ChurchAccount> _userManager { get; set; }
        public List<ChurchAccount> Users;

        [TempData]
        public string StatusMessage { get; set; }

        public IndexModel(
            ForestChurchesContext context, 
            UserManager<ChurchAccount> userManager, 
            ILogger<IndexModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;

            Users = _context.Users.ToList();
        }

        public void OnGet() => Users = _context.Users.ToList();

        [Authorize(Policy = "RoleManagement.Write")]
        public async Task<IActionResult> OnPostDeleteUser(string value)
        {
            IdentityResult result = new();
            try
            {
                var user = await _userManager.FindByIdAsync(value);

                if (user.Role == Components.Roles.Roles.SuperAdmin.ToString())
                {
                    _logger.LogInformation("Unable to delete SuperAdministrator user...");
                }

                else if (user.Role != Components.Roles.Roles.SuperAdmin.ToString())
                {
                    result = await _userManager.DeleteAsync(user);
                }
                

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User: {user.UserName} successfully deleted!");
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

            return RedirectToPage();
        }
    }
}
