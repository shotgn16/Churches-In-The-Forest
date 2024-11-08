using ForestChurches.Data;
using ForestChurches.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ForestChurches.Pages.Admin.Events
{
    [Authorize(Policy = "EventManagement.Read")]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private ForestChurchesContext _context;

        public List<EventsModel> Events { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public IndexModel(ILogger<IndexModel> logger, ForestChurchesContext context)
        {
            _logger = logger;
            _context = context;

            Events = _context.Events.ToList();
        }

        //[Authorize(Policy = "EventManagement.Delete")]
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
                    StatusMessage = $"Event '{eventToDelete.Name}' successfully deleted!";
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
    }
}
