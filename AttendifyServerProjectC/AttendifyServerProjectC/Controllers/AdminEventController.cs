using Microsoft.AspNetCore.Mvc;
using AttendifySharedProjectC.Models;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AttendifyServerProjectC.Controllers
{
    [ApiController]
    [Route("api/adminevents")]
    public class AdminEventController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminEventController> _logger;
        public AdminEventController(ApplicationDbContext context, ILogger<AdminEventController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto newEventDto)
        {

            if (string.IsNullOrWhiteSpace(newEventDto.Image))
            {
                newEventDto.Image = "Placeholder";  // This is so that the fucking code doesnt crap its pants if there isnt an image
            }

            _logger.LogInformation("Inside CreateEvent");
            if (newEventDto == null || string.IsNullOrEmpty(newEventDto.Name))
                return BadRequest("Event name is required.");

            _logger.LogInformation("Before creating the new event");
            var newEvent = new Event
            {
                Id = Guid.NewGuid(),
                Name = newEventDto.Name,
                Description = newEventDto.Description,
                //Image = newEventDto.Image,
                Participants = new List<EventParticipant>() 
            };
            if(newEventDto.Image == "Placeholder") 
            { 
                newEvent.Image = new byte[0];
            }

            _logger.LogInformation("before eventdays");
            newEvent.EventDays = newEventDto.EventDays.Select(eventDayDto => new EventDay
            {
                Id = Guid.NewGuid(),
                EventId = newEvent.Id,
                Date = DateTime.SpecifyKind(eventDayDto.Day, DateTimeKind.Utc),
                StartTime = TimeOnly.Parse(eventDayDto.StartTime), 
                EndTime = TimeOnly.Parse(eventDayDto.EndTime) 
            }).ToList();
            _logger.LogInformation("before saving context");

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return Ok(newEvent);
        }

        //we can remove this logger, its not important anymore. Created it, because i had some trouble sending certain
        //info to the controller. Basically, TimeOnly is a piece of shit. You have to convert it every time when you send it
        //from the client to the server, or the server to the client.

        [HttpPost("createlog")]      
        public async Task<IActionResult> CreateEventLogger([FromBody] CreateEventDto newEventDto)
        {

            if (string.IsNullOrWhiteSpace(newEventDto.Image))
            {
                newEventDto.Image = "Placeholder";  
            }

            _logger.LogInformation("Received a request to create an event");
            _logger.LogInformation("Event Name: {Name}", newEventDto.Name);
            _logger.LogInformation("Event Description: {Description}", newEventDto.Description);
            _logger.LogInformation("Event Image: {Image}", newEventDto.Image ?? "No image provided");

            if (newEventDto.EventDays != null && newEventDto.EventDays.Any())
            {
                foreach (var eventDay in newEventDto.EventDays)
                {
                    _logger.LogInformation("Event Day: {Day}", eventDay.Day);
                    _logger.LogInformation("Start Time: {StartTime}", eventDay.StartTime);
                    _logger.LogInformation("End Time: {EndTime}", eventDay.EndTime);
                }
            }
            else
            {
                _logger.LogInformation("No event days provided.");
            }

            return Ok("Event data received and logged.");
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<CreateEventDto>>> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.EventDays)
                .Include(e => e.Participants)
                .ToListAsync();

            var eventDtos = events.Select(e => new CreateEventDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Image = e.Image != null ? Convert.ToBase64String(e.Image) : null,  
                EventDays = e.EventDays.Select(ed => new EventDayDto
                {
                    Day = ed.Date,
                    StartTime = ed.StartTime.ToString("HH:mm"),  
                    EndTime = ed.EndTime.ToString("HH:mm")     
                }).ToList()
            }).ToList();

            return Ok(eventDtos);
        }

        [HttpGet("{eventId}")]
        public async Task<ActionResult<Event>> GetEventById(Guid eventId)
        {
            var eventDetails = await _context.Events
                .Include(e => e.EventDays)
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventDetails == null)
                return NotFound("Event not found.");

            return Ok(eventDetails);
        }

        [HttpPut("{eventId}")]
        public async Task<IActionResult> UpdateEvent(Guid eventId, [FromBody] Event updatedEvent)
        {
            var eventInDb = await _context.Events.FindAsync(eventId);
            if (eventInDb == null)
                return NotFound("Event not found.");

            eventInDb.Name = updatedEvent.Name;
            eventInDb.Description = updatedEvent.Description;
            eventInDb.Image = updatedEvent.Image;

            await _context.SaveChangesAsync();
            return Ok(eventInDb);
        }

        [HttpDelete("{eventId}")]
        public async Task<IActionResult> DeleteEvent(Guid eventId)
        {
            var eventInDb = await _context.Events.FindAsync(eventId);
            if (eventInDb == null)
                return NotFound("Event not found.");

            _context.Events.Remove(eventInDb);
            await _context.SaveChangesAsync();
            return Ok("Event deleted.");
        }
    }
}
