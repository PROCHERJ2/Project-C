using AttendifySharedProjectC.Models;
using System.Net.Http.Json;

namespace AttendifyClientProjectC.Services
{
    public class AdminEventService
    {
        private readonly HttpClient _httpClient;

        public AdminEventService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CreateEventDto>> GetAllEvents()
        {
            return await _httpClient.GetFromJsonAsync<List<CreateEventDto>>("https://localhost:7059/api/adminevents/all");
        }

        public async Task<bool> CreateEventAsync(CreateEventDto newEvent)
        {
            Console.WriteLine("Inside Create Event");
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7059/api/adminevents/create", newEvent);

            //var responseContent = await response.Content.ReadAsStringAsync();
            //Console.WriteLine($"Full response of createEvent Async: {responseContent}");                                  //this is useful btw, its how you can check
                                                                                                                            //what exactly the error is that you get from 
            return response.IsSuccessStatusCode;                                                                            //the server, if you send it wrong data for example
        }


        //This logger method is no longer relevant, but il leave it in just in case.
        public async Task<bool> LogEventAsync(CreateEventDto newEvent)
        {
            Console.WriteLine("Inside Create Event Service");
            Console.WriteLine($"Event ID: {newEvent.Id}");
            Console.WriteLine($"Event Name: {newEvent.Name}");
            Console.WriteLine($"Event Description: {newEvent.Description}");
            //Console.WriteLine($"Event Image: {newEvent.Image ?? "No image provided"}");

            if (newEvent.EventDays != null && newEvent.EventDays.Any())
            {
                foreach (var eventDay in newEvent.EventDays)
                {
                    Console.WriteLine($"Event Day: {eventDay.Day}");
                    Console.WriteLine($"Start Time: {eventDay.StartTime}");
                    Console.WriteLine($"End Time: {eventDay.EndTime}");
                }
            }
            else
            {
                Console.WriteLine("No event days provided.");
            }

            var response = await _httpClient.PostAsJsonAsync("https://localhost:7059/api/adminevents/createlog", newEvent);

            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Content: {response.Content}");

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Contentasstring: {responseContent}");


            return response.IsSuccessStatusCode;
        }


        public async Task<bool> RemoveEventAsync(Guid eventId)
        {
            var response = await _httpClient.DeleteAsync($"https://localhost:7059/api/adminevents/{eventId}");
            return response.IsSuccessStatusCode;
        }
    }
}
