using ForestChurches.Components.Email;
using ForestChurches.Components.Users;
using ForestChurches.Data;
using ForestChurches.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Encodings.Web;

namespace ForestChurches.Pages.Events
{
    [Authorize(Policy = "EventManagement.Read")]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private UserManager<ChurchAccount> _userManager;
        private ForestChurchesContext _context;
        private readonly iEmail _mailRepository;

        internal List<EventsModel> AllEvents;

        public IndexModel(
            UserManager<ChurchAccount> userManager, 
            ForestChurchesContext context, 
            ILogger<IndexModel> logger, 
            iEmail mailRepository)
        {
            _mailRepository = mailRepository;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async void OnGet()
        {
            ChurchAccount user = await _userManager.FindByNameAsync(User.Identity.Name);
            AllEvents = _context.Events
                .Where(x => x.Church == user.ChurchName)
                    .ToList();
        }

        public async Task OnPostDeleteEvent(Guid value)
        {
            try
            {
                var eventToDelete = _context.Events
                    .Where(e => e.ID == value)
                        .FirstOrDefault();

                var result = _context.Events
                    .Remove(eventToDelete);

                _context.SaveChanges();

                if (result.State == Microsoft.EntityFrameworkCore.EntityState.Deleted)
                {
                    _logger.LogInformation($"Event: '{eventToDelete.Name}' successfully deleted!");

                    // TODO : Test custom 'callbackUrl'
                    var callbackUrl = Url.Page("/Events");

                    var userData = new Dictionary<string, string>()
                    {
                        { "{event_link}", HtmlEncoder.Default.Encode(callbackUrl) },
                        { "{eventName}", eventToDelete.Name },
                        { "{eventDate}", eventToDelete.Date.ToString() },
                        { "{eventStart}", eventToDelete.StartTime.ToString() },
                        { "{eventEnd}", eventToDelete.EndTime.ToString() }
                    };

                    await _mailRepository.StartEmailAsync(User.Identity.Name, userData, "Event Deleted", "./templates/event_deleted.html");
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
    }
}
