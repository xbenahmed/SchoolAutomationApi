using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;

namespace SchoolAutomationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator,Teacher")]
    public class NotificationsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public NotificationsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification([FromBody] NotificationDto notificationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var smtpUser = _configuration["Smtp:User"];
            var smtpPass = _configuration["Smtp:Pass"];
            var fromEmail = _configuration["Smtp:FromEmail"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass) || string.IsNullOrEmpty(fromEmail))
                return StatusCode(500, "SMTP configuration missing.");

            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = notificationDto.Subject,
                    Body = notificationDto.Body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(notificationDto.ToEmail);

                await client.SendMailAsync(mailMessage);
                return Ok("Notification sent.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to send notification: {ex.Message}");
            }
        }
    }

    public class NotificationDto
    {
        public required string ToEmail { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }
    }
}