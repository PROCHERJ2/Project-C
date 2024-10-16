using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using AttendifySharedProjectC.Models;

namespace AttendifyServerProjectC.Controllers
{
    [ApiController]
    [Route("api/email")]
    public class EmailController : ControllerBase
    {
        [HttpPost("send-verification-email")]
        public async Task<IActionResult> SendVerificationEmail([FromBody] EmailRequestModel emailRequest)
        {
            await SendEmailAsync(emailRequest.ToEmail, emailRequest.Subject, emailRequest.Body);
            return Ok("Verification email sent successfully.");
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            //using (var smtpClient = new SmtpClient("smtp.office365.com"))
            //using (var smtpClient = new SmtpClient("smtp-mail.outlook.com"))
            using (var smtpClient = new SmtpClient("smtp.office365.com"))
            {
                smtpClient.Port = 587;
                //smtpClient.EnableSsl = true;
                //smtpClient.Credentials.GetCredential
                //smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential("AttendifySupp0rt@hotmail.com", "kdhaifcaumtakbuu");  //This is the App password, not the actual one
                smtpClient.EnableSsl = true;
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("AttendifySupp0rt@hotmail.com"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }

        //public class EmailRequest
        //{
        //    public string ToEmail { get; set; }
        //    public string Subject { get; set; }
        //    public string Body { get; set; }
        //}
    }
}
