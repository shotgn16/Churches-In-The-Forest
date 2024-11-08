using ForestChurches.Components.AutoEvents;
using ForestChurches.Components.ImageHandler;
using ForestChurches.Components.LogReader;
using ForestChurches.Data;
using ForestChurches.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ForestChurches.Pages.Admin.Events
{
    [Authorize(Policy = "EventManagement.Edit")]
    public class EditModel : PageModel
    {
        private readonly ILogger<EditModel> _logger;
        private ForestChurchesContext _context;
        private ImageInterface _ImageRepository;
        private EventInterface _eventController;

        public EventsModel currentEvent { get; set; }
        public List<SelectListItem> UserList { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public EventsModel Input { get; set; }

        [BindProperty]
        public string SelectedUsername { get; set; }

        public EditModel(ILogger<EditModel> logger, ForestChurchesContext context, ImageInterface imageRepository, EventInterface eventController)
        {
            _context = context;
            _logger = logger;
            _eventController = eventController;
        }

        public async void OnGet(string id)
        {
            try
            {
                currentEvent = await _context.Events
                    .Where(x => x.ID == Guid.Parse(id))
                        .FirstOrDefaultAsync();

                Input = new EventsModel
                {
                    Name = currentEvent.Name,
                    Description = currentEvent.Description,
                    Date = currentEvent.Date,
                    User = currentEvent.User,
                    Address = currentEvent.Address,
                    StartTime = currentEvent.StartTime,
                    EndTime = currentEvent.EndTime,
                    Church = currentEvent.Church,
                    Link = currentEvent.Link,
                    SelectedUsername = currentEvent.User
                };

                var users = _context.Users.ToList();

                UserList = users.Select(user => new SelectListItem{
                    Text = user.UserName,
                    Value = user.UserName,
                    Selected = user.UserName == currentEvent?.User
                }).ToList();


                // Set SelectedUsername to the current event's user, if available
                SelectedUsername = currentEvent?.User;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }

        // TODO : Add support for adding images when editing events (NEEDS TO BE TESTED)
        public async Task<IActionResult> OnPostUpdateEvent(string id)
        {
            try
            {
                if (Input != null)
                {
                    var eventToUpdate = await _context.Events
                        .Where(x => x.ID == Guid.Parse(id))
                            .FirstOrDefaultAsync();

                    if (Input.Image != null)
                    {
                        byte[] storableImage = await _ImageRepository.ConvertToByteArray(Input.Image);

                        await _eventController.UpdateEventAsync(Input, storableImage);
                    }

                    else if (Input.Image == null)
                    {
                        // TODO : Test the image function
                        await _eventController.UpdateEventAsync(Input, eventToUpdate.ImageArray);
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
