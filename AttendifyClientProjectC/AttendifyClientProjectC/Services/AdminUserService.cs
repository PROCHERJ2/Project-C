using System.Net.Http.Json;
using AttendifySharedProjectC.Models;
using static AttendifyClientProjectC.Pages.Admin;

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

        public async Task<List<RoleDto>> GetRoles()
        {
            return await _httpClient.GetFromJsonAsync<List<RoleDto>>("https://localhost:7059/api/adminuser/getroles");
        }

        public async Task<bool> ChangeUserRoleAsync(string userId, string newRoleId)
        {
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7059/api/adminuser/change-role", new { UserId = userId, NewRoleId = newRoleId });
            return response.IsSuccessStatusCode;
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

        public async Task<bool> RemoveUserAsync(string userId)
        {
            var response = await _httpClient.DeleteAsync($"https://localhost:7059/api/adminuser/remove-user/{userId}");
            return response.IsSuccessStatusCode;
        }


    }
}
