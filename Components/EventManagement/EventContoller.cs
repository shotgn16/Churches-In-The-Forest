using ForestChurches.Components.Email;
using ForestChurches.Components.LogReader;
using ForestChurches.Components.Users;
using ForestChurches.Data;
using ForestChurches.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ForestChurches.Components.AutoEvents
{
    public class EventContoller : Controller, EventInterface
    {
        // Dependency Injection
        private ILogger<EventContoller> _logger;
        private ForestChurchesContext _context;

        // Property Delcaration
        public List<EventsModel> Events { get; set; }
        public List<EventsModel> EventsToDelete { get; set; }
        public List<EventsModel> RepeatedEvents { get; set; }
        public List<EventsModel> RepeatedEventsToDelete { get; set; }
        private readonly UserManager<ChurchAccount> _userManager;
        private readonly iEmail _mailRepository;

        public EventContoller(ForestChurchesContext context, iEmail mailRepository, UserManager<ChurchAccount> userManager, ILogger<EventContoller> logger)
        {
            _logger = logger;
            _context = context;
            _mailRepository = mailRepository;

            // Call the GetEvents method to populate the Events property
            GetEvents();
        }

        public async Task GetEvents()
        {
            Events = _context.Events
                .Where(a => a.Repeats == 0)
                .ToList();
        }

        public async Task GetRepeatedEvents()
        {
            RepeatedEvents = _context.Events
                .Where(a => a.Repeats > 0)
                .ToList();
        }

        public async Task SortAndDeleteEvents()
        {
            foreach (var item in Events)
            {
                var tempDateTime = item.Date.ToDateTime(item.EndTime);

                if (DateTime.Now.AddHours(24) >= tempDateTime)
                {
                    EventsToDelete.Add(item);
                }
            }
        }

        public async Task InformUsersAndDeleteEvents(List<EventsModel> eventsToDelete)
        {
            foreach (var item in eventsToDelete)
            {
                var user = _userManager.FindByNameAsync(item.User).Result;

                _context.Events.Remove(item);

                var userData = new Dictionary<string, string>()
                { 
                    { "{eventName}", item.Name },
                    { "{eventDate}", item.Date.ToString() },
                    { "{eventStart}", item.StartTime.ToString() },
                    { "{eventEnd}", item.EndTime.ToString() }
                };

                await _mailRepository.StartEmailAsync(user.Email, userData, "Event Deleted", "./templates/event_deleted.html");
            }

            _context.SaveChanges();
        }

        // Task : Check auto-event management works
        public async Task CheckRepeatedEvents()
        {
            foreach (var item in RepeatedEvents)
            {
                var user = _userManager.FindByNameAsync(item.User).Result;
                var tempDateTime = item.Date.ToDateTime(item.EndTime);

                if (DateTime.Now.AddDays(14) >= tempDateTime)
                {
                    // Task : Check emails work
                    var userData = new Dictionary<string, string>()
                    {
                    { "{eventName}", item.Name },
                    { "{eventDate}", item.Date.ToString() },
                    { "{eventStart}", item.StartTime.ToString() },
                    { "{eventEnd}", item.EndTime.ToString() },
                    { "{event_link}", "https://forestchurches.co.uk/Events" }
                    };

                    //var callbackUrl = Url.Page(
                    //    "/Events",
                    //    pageHandler: null,
                    //    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    //    protocol: Request.Scheme);

                    await _mailRepository.StartEmailAsync(user.Email, userData, "Do you still need this?", "./templates/confirm_repeated_event.html");
                }
            }
        }

        // SWITCHING TO DIFFERENT TYPE OF FUNCTIONS NOW

        public async Task UpdateEventAsync(EventsModel input, Byte[] Image)
        {
            try
            {
                var existingEvent = await _context.Events
                    .FirstOrDefaultAsync(x => x.ID == input.ID);

                if (existingEvent != null)
                {
                    _context.Entry(existingEvent).CurrentValues.SetValues(input);
                    _context.Entry(existingEvent.User).CurrentValues.SetValues(input.User);
                    _context.Entry(existingEvent.ImageArray).CurrentValues.SetValues(Image);

                    await _context.SaveChangesAsync();
                }

                else
                {
                    // Handle the case when the event with the provided ID dosen't exist...
                    _logger.LogError("Event with ID {0} does not exist...", input.ID);
                }
            }

            catch (Exception ex)
            {
                // Other exception
                _logger.LogError(ex.Message, ex);
            }
        }

        public async Task<Guid> GetEventIDAsync(EventsModel input)
        {
            return await _context.Events.Where(e => e.Name == input.Name)
                .Where(a => a.Date == input.Date).Where(c => c.StartTime == input.StartTime)
                    .Select(x => x.ID).FirstOrDefaultAsync();
        }

    }
}
