using AttendifySharedProjectC.Models;
using System.Net.Http.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace AttendifyClientProjectC.Services
{
    public class TestService
    {
        private readonly HttpClient _httpClient;

        public TestService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TestModel> SendTestModel(TestModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7059/api/test", model);
            //var response = await _httpClient.PostAsJsonAsync("api/test", model);
            return await response.Content.ReadFromJsonAsync<TestModel>();
        }
    }
}
