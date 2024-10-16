using System.Net.Http.Json;
using AttendifySharedProjectC.Models;

namespace AttendifyClientProjectC.Services
{
    public class EmailService
    {
        private readonly HttpClient _httpClient;

        public EmailService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendVerificationEmailAsync(string email, string token)
        {
            var emailRequest = new EmailRequestModel
            {
                ToEmail = email,
                Subject = "Account Verification Code",
                Body = $"<p>Your verification token is: <strong>{token}</strong></p>"
            };

            //var response = await _httpClient.PostAsJsonAsync("api/email/send-verification-email", emailRequest);
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7059/api/email/send-verification-email", emailRequest);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to send verification email.");
            }
        }
    }
}
