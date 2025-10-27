using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class OtpService : IOtpService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtpService> _logger;

        public OtpService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<OtpService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        public async Task<bool> SendOtpEmailAsync(string email, string name, string otp)
        {
            return await SendEmailAsync(email, name, otp, "email_verification", "Your AlliEats Verification Code");
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string name, string otp)
        {
            return await SendEmailAsync(email, name, otp, "password_reset", "Your AlliEats Password Reset Code");
        }

        private async Task<bool> SendEmailAsync(string email, string name, string otp, string emailType, string subject)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                // Get EmailJS configuration
                var emailJsServiceId = _configuration["EmailJS:ServiceId"];
                var emailJsTemplateId = _configuration["EmailJS:TemplateId"];
                var emailJsPublicKey = _configuration["EmailJS:PublicKey"];
                var emailJsPrivateKey = _configuration["EmailJS:PrivateKey"];

                _logger.LogInformation("Sending {EmailType} email to {Email}", emailType, email);

                // Check if configuration is missing
                if (string.IsNullOrEmpty(emailJsServiceId) || string.IsNullOrEmpty(emailJsTemplateId) || string.IsNullOrEmpty(emailJsPublicKey))
                {
                    _logger.LogError("EmailJS configuration is missing or incomplete");
                    return false;
                }

                var message = emailType == "password_reset"
                    ? $"You have requested to reset your password. Your verification code is: {otp}. This code will expire in 15 minutes."
                    : $"Your verification code is: {otp}. This code will expire in 15 minutes.";

                var emailData = new
                {
                    service_id = emailJsServiceId,
                    template_id = emailJsTemplateId,
                    user_id = emailJsPublicKey,
                    accessToken = emailJsPrivateKey,
                    template_params = new
                    {
                        to_email = email,
                        to_name = name,
                        otp_code = otp,
                        app_name = "AlliEats",
                        subject = subject,
                        message = message
                    }
                };

                var json = JsonSerializer.Serialize(emailData);
                _logger.LogInformation("EmailJS Request JSON: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.emailjs.com/api/v1.0/email/send", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("{EmailType} email sent successfully to {Email}", emailType, email);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send {EmailType} email. Status: {StatusCode}, Response: {Response}",
                        emailType, response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while sending {EmailType} email to {Email}", emailType, email);
                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout occurred while sending {EmailType} email to {Email}", emailType, email);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while sending {EmailType} email to {Email}", emailType, email);
                return false;
            }
        }

        public bool ValidateOtp(string providedOtp, string storedOtp, DateTime? expiry)
        {
            if (string.IsNullOrEmpty(providedOtp) || string.IsNullOrEmpty(storedOtp))
                return false;

            if (expiry == null || DateTime.Now > expiry)
                return false;

            return providedOtp.Equals(storedOtp, StringComparison.OrdinalIgnoreCase);
        }
    }
}