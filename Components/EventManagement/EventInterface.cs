using ForestChurches.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Paddings;

namespace ForestChurches.Components.AutoEvents
{
    public interface EventInterface
    {
        // This method will pull all events from the database
        // It will only pull events which are NOT marked for repeat...
        Task GetEvents();
        Task GetRepeatedEvents();

        // This method will iterate through the events and sort/delete those which have experied date/times (+24 hours)
        Task SortAndDeleteEvents();

        // This method will check repeated events and email users to ask if they still want events
        Task InformUsersAndDeleteEvents(List<EventsModel> eventsToDelete);

        // This method will check (monthly) if the user still needs the events that are registered to repeat
        Task CheckRepeatedEvents();
        Task UpdateEventAsync(EventsModel input, Byte[] Image);
        Task<Guid> GetEventIDAsync(EventsModel input);
    }
}
