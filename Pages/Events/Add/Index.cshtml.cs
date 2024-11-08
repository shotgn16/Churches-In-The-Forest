using ForestChurches.Components.AutoEvents;
using ForestChurches.Components.Email;
using ForestChurches.Components.Http;
using ForestChurches.Components.ImageHandler;
using ForestChurches.Components.Users;
using ForestChurches.Data;
using ForestChurches.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ForestChurches.Pages.Events.Add
{
    [Authorize(Policy = "UserEvents.View", Roles = "AuthorizedChurch")]
    public class IndexModel : PageModel
    {
        // Binds form input to this property
        [BindProperty]
        public EventsModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }
        public Guid EventID { get; set; }

        private readonly ILogger<IndexModel> _logger;
        private ForestChurchesContext _context;
        private ImageInterface _iImage;
        private iEmail _mailRepository;
        private EventInterface _eventController;
        private UserManager<ChurchAccount> _userManager;

        // Does user have 'CTM' role? 
        public IActionResult Index() => Content("CTM");

        public IndexModel(
            ILogger<IndexModel> logger, 
            IHttpWrapper httpWrapper,
            ForestChurchesContext context, 
            ImageInterface iImage, 
            iEmail mailRepository,
            UserManager<ChurchAccount> userManager,
            EventInterface eventController)
        {
            _logger = logger;
            _context = context;
            _iImage = iImage;
            _userManager = userManager;
            _mailRepository = mailRepository;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input != null)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);

                if (Input.Image != null)
                {
                    byte[] storableImage = await _iImage.ConvertToByteArray(Input.Image);

                    await _context.Events.AddAsync(new EventsModel
                    {
                        Name = Input.Name,
                        Description = Input.Description,
                        Date = Input.Date,
                        User = user.UserName,
                        Address = Input.Address,
                        StartTime = Input.StartTime,
                        EndTime = Input.EndTime,
                        Church = user.ChurchName,
                        ImageArray = storableImage,
                        Repeats = Input.Repeats,
                        Link = Input.Link
                    });

                    await _context.SaveChangesAsync();

                    EventID = await _eventController.GetEventIDAsync(Input);
                        StatusMessage = EventID != Guid.Empty ? "New Event Successfullly Created!" : "An Error occurred while submitting your event. ";

                    if (StatusMessage == "New Event Successfullly Created!")
                    {
                        var userData = new Dictionary<string, string>()
                        {
                            { "{event_name}", Input.Name },
                            { "{event_description}", Input.Description },
                            { "{event_location}", Input.Address },
                            { "{event_date}", Input.Date.ToString() },
                            { "{event_startTime}", Input.StartTime.ToString() },
                            { "{event_endTime}", Input.EndTime.ToString() },
                            { "{event_repeats", IntToRepeats(Input.Repeats).Result }
                        };

                        await _mailRepository.StartEmailAsync(User.Identity.Name, userData, "Event Successfully created", "./templates/event_created.html");
                    }
                }

                else if (Input.Image == null)
                {
                    byte[] storableImage = await _iImage.ConvertToByteArrayFromUrl("https://i.imgur.com/1yxyCKl.png");

                    await _context.Events.AddAsync(new EventsModel
                    {
                        Name = Input.Name,
                        Description = Input.Description,
                        Date = Input.Date,
                        User = user.UserName,
                        Address = Input.Address,
                        StartTime = Input.StartTime,
                        EndTime = Input.EndTime,
                        Church = user.ChurchName,
                        ImageArray = storableImage,
                        Repeats = Input.Repeats,
                        Link = Input.Link
                    });

                    await _context.SaveChangesAsync();

                    EventID = await _eventController.GetEventIDAsync(Input);
                        StatusMessage = EventID != Guid.Empty ? "New Event Successfullly Created!" : "An Error occurred while submitting your event. ";
                
                    if (StatusMessage == "New Event Successfullly Created!")
                    {
                        var userData = new Dictionary<string, string>()
                        {
                            { "{event_name}", Input.Name },
                            { "{event_description}", Input.Description },
                            { "{event_location}", Input.Address },
                            { "{event_date}", Input.Date.ToString() },
                            { "{event_startTime}", Input.StartTime.ToString() },
                            { "{event_endTime}", Input.EndTime.ToString() },
                            { "{event_repeats", IntToRepeats(Input.Repeats).Result }
                        };

                        await _mailRepository.StartEmailAsync(User.Identity.Name, userData, "Event Successfully created", "./templates/event_created.html");
                    }
                }
            }

            return Page();
        }

        private async Task<string> IntToRepeats(int repeats)
        {
            switch (repeats)
            {
                case 0: return "None";
                case 1: return "Daily";
                case 2: return "Weekly";
                case 3: return "Every 2 Weeks";
                case 4: return "Every 3 Weeks";
                case 5: return "Monthly";
                case 6: return "Every 2 Months";
                default: return "None";
            }
        }
    }
}
