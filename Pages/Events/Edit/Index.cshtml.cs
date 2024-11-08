using ForestChurches.Components.AutoEvents;
using ForestChurches.Components.ImageHandler;
using ForestChurches.Data;
using ForestChurches.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForestChurches.Pages.Events.Edit
{
    [Authorize(Policy = "UserEvents.Add", Roles = "AuthorizedChurch")]
    public class IndexModel : PageModel
    {
        private ILogger<IndexModel> _logger;
        private ForestChurchesContext _context;
        private ImageInterface _ImageRepository;
        private EventInterface _eventController;

        public EventsModel currentEvent { get; set; }
        public Guid EventID { get; set; }

        [TempData]
        public string StatusMessage { get; set; } 

        [BindProperty]
        public int Frequency { get; set; }

        [BindProperty]
        public EventsModel Input { get; set; }

        public IndexModel(ILogger<IndexModel> logger,
            ForestChurchesContext context, 
            ImageInterface ImageRepository,
            EventInterface eventController)
        {
            _ImageRepository = ImageRepository;
            _eventController = eventController;
            _context = context;
            _logger = logger;
        }

        public async void OnGet(string id)
        {
            try
            {
                currentEvent = await _context.Events
                    .Where(x => x.ID == Guid.Parse(id))
                        .FirstOrDefaultAsync();

                EventID = Guid.Parse(id);

                Input = new EventsModel
                {
                    Name = currentEvent.Name,
                    Description = currentEvent.Description,
                    Date = currentEvent.Date,
                    User = currentEvent.User, // Assuming this is automatically set to the current user elsewhere
                    Address = currentEvent.Address,
                    StartTime = currentEvent.StartTime,
                    EndTime = currentEvent.EndTime,
                    Church = currentEvent.Church,
                    Link = currentEvent.Link
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }

        // BUG : Currently not working... Cannot save changes to events
        public async Task<IActionResult> OnPostUpdateEvent()
        {
            try
            {
                if (Input != null)
                {
                    // TODO : Fix this - Unable to assign a value to the property 
                    var eventToUpdate = await _context.Events
                        .Where(x => x.ID == Guid.Parse(EventID.ToString()))
                            .FirstOrDefaultAsync();

                    if (Input.Image != null)
                    {
                        byte[] storableImage = await _ImageRepository.ConvertToByteArray(Input.Image);

                        await _eventController.UpdateEventAsync(new EventsModel
                        {
                            Name = Input.Name,
                            Repeats = Frequency,
                            Description = Input.Description,
                            Date = Input.Date,
                            User = User.Identity.Name,
                            Address = Input.Address,
                            StartTime = Input.StartTime,
                            EndTime = Input.EndTime,
                            Church = Input.Church,
                            Link = Input.Link
                        }, storableImage);                            
                    }

                    else if (Input.Image == null)
                    {
                        await _eventController.UpdateEventAsync(new EventsModel
                        {
                            Name = Input.Name,
                            Repeats = Frequency,
                            Description = Input.Description,
                            Date = Input.Date,
                            User = User.Identity.Name,
                            Address = Input.Address,
                            StartTime = Input.StartTime,
                            EndTime = Input.EndTime,
                            Church = Input.Church,
                            Link = Input.Link
                        }, eventToUpdate.ImageArray);
                    }

                    StatusMessage = $"Event '{eventToUpdate.Name}' successfully updated...";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

            return RedirectToPage("./Index");
        }
    }
}
