using ForestChurches.Components.Http.Google;
using ForestChurches.Components.Users;
using ForestChurches.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;

namespace ForestChurches.Areas.Identity.Pages.Account.Manage
{
    public class ChurchProfile : PageModel
    {
        // Dependency Injection
        private readonly ILogger<ChurchProfile> _logger;
        private readonly UserManager<ChurchAccount> _usermanager;
        private readonly ForestChurchesContext _context;
        private readonly GoogleInterface _googleAPI;

        // Data objects
        public List<string> _servicetimes;
        public ChurchAccount _currentuser;

        [BindProperty]
        public Models.ChurchInformation _currentchurch { get; set; }
        
        // Tempory Data
        [TempData]
        public string StatusMessage { get; set; }

        // Page constructor
        public ChurchProfile(ILogger<ChurchProfile> _logger, UserManager<ChurchAccount> _usermanager, ForestChurchesContext _context, GoogleInterface googleAPI)
        {
            this._logger = _logger;
            this._usermanager = _usermanager;
            this._context = _context;
            this._googleAPI = _googleAPI;
        }

        public void OnGet()
        {
            _currentuser = _usermanager.FindByEmailAsync(User.Identity.Name).Result;
            _currentchurch = _context.ChurchInformation.Where(c => c.Name == _currentuser.ChurchName).FirstOrDefault() ?? throw new ArgumentNullException("An error occurred while loading your church information. Please try again later.\nIf this issue persists, please contact support.");
        }

        // OnPost : Will be used for submitting the page to update all church information.
        // Will NOT include service times (Will be seperate information)
        public async Task<IActionResult> OnPostUpdateChurch()
        {
            try
            {
                _currentuser = await _usermanager.FindByEmailAsync(User.Identity.Name);

                await _context.ChurchInformation.Where(c => c.ChurchAccountId == _currentuser.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(a => a.Name, _currentchurch.Name)
                        .SetProperty(b => b.Description, _currentchurch.Description)
                        .SetProperty(c => c.Denominaion, _currentchurch.Denominaion)
                        .SetProperty(d => d.Congregation, _currentchurch.Congregation)
                        .SetProperty(e => e.Address, _currentchurch.Address)
                        .SetProperty(f => f.Phone, _currentchurch.Phone)
                        .SetProperty(g => g.Churchsuite, _currentchurch.Churchsuite)
                        .SetProperty(a => a.Website, _currentchurch.Website)
                        .SetProperty(b => b.Parking, _currentchurch.Parking)
                        .SetProperty(c => c.Restrooms, _currentchurch.Restrooms)
                        .SetProperty(d => d.WheelchairAccess, _currentchurch.WheelchairAccess)
                        .SetProperty(e => e.Wifi, _currentchurch.Wifi)
                        .SetProperty(f => f.Refreshments, _currentchurch.Refreshments)
                        .SetProperty(g => g.ServiceTimes, new List<string>(_currentchurch.ServiceTimes)));

                await _context.SaveChangesAsync();
                StatusMessage = $"Church '{_currentchurch.Name}' was successfully updated!";
            }

            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddService(string _serviceTime)
        {
            _currentuser = await _usermanager.FindByEmailAsync(User.Identity.Name);

            if (!_servicetimes.Contains(_serviceTime))
            {
                _servicetimes.Append(_serviceTime);
            }

            else if (_servicetimes.Contains(_serviceTime))
            {
                _logger.LogWarning($"User '{User.Identity.Name}' tried to log church service time '{_serviceTime}' more than once in system.");
                    throw new Exception($"Service: '{_serviceTime}' is already listed.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveService(string _serviceTime)
        {
            if (!_serviceTime.Contains(_serviceTime))
            {
                _servicetimes.Remove(_serviceTime);
            }

            else if (_servicetimes.Contains(_serviceTime))
            {
                _logger.LogWarning($"User '{User.Identity.Name}' tried to remove an unknown church service time '{_serviceTime}' from the system.");
                throw new Exception($"Service: '{_serviceTime}' could not be found.");
            }

            return Page();
        }

        [HttpGet]
        public async Task<IActionResult> GetAddress(double latitude, double longitude)
        {
            var address = await _googleAPI.ConvertToAddress(latitude, longitude);

            return new OkObjectResult(address);
        }
    }
}