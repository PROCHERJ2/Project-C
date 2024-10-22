using System.Net.Http.Json;
using AttendifySharedProjectC.Models;

namespace AttendifyClientProjectC.Services
{
    public class AdminUserService
    {
        private readonly HttpClient _httpClient;

        public AdminUserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<UserDto>> GetUsers()
        {
            return await _httpClient.GetFromJsonAsync<List<UserDto>>("https://localhost:7059/api/adminuser/getusers");
        }

        public async Task<List<Pages.Admin.RoleVerificationDto>> GetUserRoleVerifications()
        {
            return await _httpClient.GetFromJsonAsync<List<Pages.Admin.RoleVerificationDto>>("https://localhost:7059/api/adminuser/user-role-verifications");
        }

        public async Task<bool> AcceptRequestAsync(string userId)
        {
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7059/api/adminuser/accept-request", userId);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DenyRequestAsync(string userId)
        {
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7059/api/adminuser/deny-request", userId);
            return response.IsSuccessStatusCode;
        }


    }
}
