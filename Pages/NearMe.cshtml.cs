using ForestChurches.Components.Email;
using ForestChurches.Components.FileManager;
using ForestChurches.Components.Http.Google;
using ForestChurches.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ForestChurches.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using Microsoft.CodeAnalysis.Scripting;
using ForestChurches.Components.Users;

namespace ForestChurches.Pages
{
    public class NearMeModel : PageModel
    {
        private GoogleInterface _googleInterface;
        private PostcodeValidatorInterface _iPostcode;
        private FileInterface _fileInterface;
        private ILogger<NearMeModel> _logger;
        private ForestChurchesContext _context;
        private iEmail _mailRepository;
        private UserManager<ChurchAccount> _userManager;

        internal List<PlacesAPI.Place> _places;
        internal List<EventsModel> _events;
        internal List<OrganizedEvents> _organisedEvents;
        internal List<string> cid;
        internal string emailAddress;

        [TempData]
        internal bool isAuthenticated { get; set; }
        internal bool ShowPlacesModal { get; set; }
        internal bool ShowEventsModal { get; set; }

        public NearMeModel(
            GoogleInterface GoogleInterface, 
            FileInterface fileInterface, 
            PostcodeValidatorInterface iPostcode, 
            ForestChurchesContext context, 
            iEmail mailRepository, 
            UserManager<ChurchAccount> userManager,
            ILogger<NearMeModel> logger)
        {
            this._googleInterface = GoogleInterface;
            this._context = context;
            this._mailRepository = mailRepository;
            this._fileInterface = fileInterface;
            this._iPostcode = iPostcode;
            this._userManager = userManager;
            this._logger = logger;

            this.cid = new();
            this._places = new();
            this._events = new();
            this._organisedEvents = new();
        }

        // Churches Near Me!
        public async Task<IActionResult> OnPostChurchForm(string postcode)
        {
            try
            {
                postcode = postcode.ToUpper();
                ViewData["postcode"] = postcode;

                await _iPostcode.ValidatePostcodeAsync(postcode.ToUpper());

                double[] coordinates = await _googleInterface.ConvertToCoordinates(postcode);
                this._places = await _googleInterface.GetNearbyPlaces(coordinates[0], coordinates[1]);

                foreach (var place in _places)
                {
                    cid.Add(place.id);
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                RedirectToPage("/Error", ex.GetType());
            }

        PageReturn:
            ShowPlacesModal = true;
            return Page();
        }

        public async Task<IActionResult> OnPostEventForm()
        {
            try
            {
                _events = await _context.Events.Include(e => e.Church).ToListAsync();

                _organisedEvents = _events
                    .Select(item => new OrganizedEvents
                    {
                        Events = item,
                        User =  _userManager.FindByNameAsync(item.User).Result
                    })
                    .OrderBy(o => o.Events.Date)
                    .ToList();

                ShowEventsModal = true;
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostDownloadInformation(string Id)
        {
            var church = await _googleInterface.GetPlaceInformation(Id);
            return await _fileInterface.CreateFile(church, "text/plain", "Church Information");
        }

        public async Task<IActionResult> OnPostSendInformationWithEmail(string Id, string email = "")
        {
            emailAddress = email;

            // Getting church details from Google API
            var church = _googleInterface.GetPlaceInformation(Id).Result;

            // Checking if the user is Authenticated or not
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                emailAddress = user?.Email;
            }

            // Ensure email is not null or empty for both authenticated and unauthenticated users
            if (string.IsNullOrEmpty(email))
            {
                // Throw error?
                return Page();
            }

            // Defining empty dictionary object for user data (used in emails)
            Dictionary<string, string> userData = new();

            if (church != null)
            {
                if (church.result.opening_hours != null)
                {
                    userData = new Dictionary<string, string>()
                    {
                        { "{username}", email },
                        { "{church_name}", church?.result.name ?? "Not available" },
                        { "{church_address}", church?.result?.formatted_address ?? "Not available" },
                        { "{church_phone}", church?.result?.formatted_phone_number ?? "Not available" },
                        { "{church_website}", church?.result?.website ?? "Not available" },
                        { "{church_wheelchairEntrance}", church.result.wheelchair_accessible_entrance == null ? "Not available" : (church.result.wheelchair_accessible_entrance ? "Available" : "Unavailable") },
                        { "{opening_monday}", church.result.current_opening_hours.weekday_text[0] },
                        { "{opening_tuesday}", church?.result.current_opening_hours.weekday_text[1] },
                        { "{opening_wednesday}", church?.result.current_opening_hours.weekday_text[2] },
                        { "{opening_thursday}", church?.result.current_opening_hours.weekday_text[3] },
                        { "{opening_friday}", church?.result.current_opening_hours.weekday_text[4] },
                        { "{opening_saturday}", church?.result.current_opening_hours.weekday_text[5] },
                        { "{opening_sunday}", church?.result.current_opening_hours.weekday_text[6] }
                    };
                }

                else if (church.result.opening_hours == null)
                {
                    userData = new Dictionary<string, string>()
                    {
                        { "{username}", email },
                        { "{church_name}", church?.result.name ?? "Not available" },
                        { "{church_address}", church?.result?.formatted_address ?? "Not available" },
                        { "{church_phone}", church?.result?.formatted_phone_number ?? "Not available" },
                        { "{church_website}", church?.result?.website ?? "Not available" },
                        { "{church_wheelchairEntrance}", church.result.wheelchair_accessible_entrance == null ? "Not available" : (church.result.wheelchair_accessible_entrance ? "Available" : "Unavailable") },
                        { "{opening_monday}", "Not available" },
                        { "{opening_tuesday}", "Not available" },
                        { "{opening_wednesday}", "Not available" },
                        { "{opening_thursday}", "Not available" },
                        { "{opening_friday}", "Not available" },
                        { "{opening_saturday}", "Not available" },
                        { "{opening_sunday}", "Not available" }
                    };
                }
            }
            
            await _mailRepository.StartEmailAsync(emailAddress, userData, "Church Information", "./templates/church_information.html");
                TempData["Message"] = $"The information has been sent to '{emailAddress}'. Please be sure to also check your spam folder.";

            return RedirectToPage("/NearMe");
        }
    }
}


