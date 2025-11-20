using ASI.Basecode.Services.Interfaces;
using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<EmailNotificationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendOrderConfirmationEmailAsync(string email, string customerName, string orderNumber, decimal totalAmount)
        {
            var templateData = new
            {
                customerName = customerName,
                orderNumber = orderNumber,
                totalAmount = totalAmount.ToString("F2"),
                orderDate = DateTime.Now.ToString("MMMM dd, yyyy h:mm tt")
            };

            var (subject, message) = GetEmailTemplate("OrderConfirmation", templateData);

            return await SendUniversalEmailAsync(email, customerName, subject, message, new
            {
                email_type_display = "Order Confirmation",
                order_number = orderNumber,
                total_amount = totalAmount.ToString("F2"),
                order_date = DateTime.Now.ToString("MMMM dd, yyyy h:mm tt")
            }, "order_confirmation");
        }

        public async Task<bool> SendOrderStatusUpdateEmailAsync(string email, string customerName, string orderNumber, string oldStatus, string newStatus)
        {
            var statusMessage = GetStatusMessage(newStatus);

            var templateData = new
            {
                customerName = customerName,
                orderNumber = orderNumber,
                oldStatus = oldStatus,
                newStatus = newStatus,
                statusMessage = statusMessage
            };

            var (subject, message) = GetEmailTemplate("OrderStatusUpdate", templateData);

            return await SendUniversalEmailAsync(email, customerName, subject, message, new
            {
                email_type_display = "Order Status Update",
                order_number = orderNumber,
                old_status = oldStatus,
                new_status = newStatus,
                status_message = statusMessage
            }, "order_status_update");
        }

        public async Task<bool> SendOrderReadyNotificationEmailAsync(string email, string customerName, string orderNumber)
        {
            var templateData = new
            {
                customerName = customerName,
                orderNumber = orderNumber
            };

            var (subject, message) = GetEmailTemplate("OrderReady", templateData);

            return await SendUniversalEmailAsync(email, customerName, subject, message, new
            {
                email_type_display = "Order Ready for Delivery",
                order_number = orderNumber,
                status_message = "Your delicious food is prepared and ready. Our delivery partner will pick it up shortly and deliver it to you."
            }, "order_ready");
        }

        public async Task<bool> SendOrderDeliveredNotificationEmailAsync(string email, string customerName, string orderNumber)
        {
            var templateData = new
            {
                customerName = customerName,
                orderNumber = orderNumber
            };

            var (subject, message) = GetEmailTemplate("OrderDelivered", templateData);

            return await SendUniversalEmailAsync(email, customerName, subject, message, new
            {
                email_type_display = "Order Out for Delivery",
                order_number = orderNumber,
                status_message = "Your order is on the way! Please confirm receipt within 2 hours once it's delivered."
            }, "order_delivered");
        }

        public async Task<bool> SendOrderCancelledNotificationEmailAsync(string email, string customerName, string orderNumber, string reason)
        {
            var templateData = new
            {
                customerName = customerName,
                orderNumber = orderNumber,
                reason = reason
            };

            var (subject, message) = GetEmailTemplate("OrderCancelled", templateData);

            return await SendUniversalEmailAsync(email, customerName, subject, message, new
            {
                email_type_display = "Order Cancellation",
                order_number = orderNumber,
                cancellation_reason = reason
            }, "order_cancelled");
        }

        /// <summary>
        /// Gets email template from configuration and replaces placeholders with actual values
        /// </summary>
        private (string Subject, string Body) GetEmailTemplate(string templateName, object templateData)
        {
            try
            {
                var subjectTemplate = _configuration[$"EmailTemplates:{templateName}:Subject"];
                var bodyTemplate = _configuration[$"EmailTemplates:{templateName}:Body"];

                if (string.IsNullOrEmpty(subjectTemplate) || string.IsNullOrEmpty(bodyTemplate))
                {
                    _logger.LogWarning("Email template '{TemplateName}' not found in configuration. Using fallback.", templateName);
                    return GetFallbackTemplate(templateName, templateData);
                }

                // Replace placeholders in both subject and body
                var subject = ReplacePlaceholders(subjectTemplate, templateData);
                var body = ReplacePlaceholders(bodyTemplate, templateData);

                return (subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email template '{TemplateName}'. Using fallback.", templateName);
                return GetFallbackTemplate(templateName, templateData);
            }
        }

        /// <summary>
        /// Replaces placeholders in template string with actual values
        /// </summary>
        private string ReplacePlaceholders(string template, object templateData)
        {
            var result = template;

            // Use reflection to get all properties from templateData object
            var properties = templateData.GetType().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(templateData)?.ToString() ?? string.Empty;
                var placeholder = "{" + property.Name + "}";
                result = result.Replace(placeholder, value);
            }

            // Replace common escape sequences
            result = result.Replace("\\n", "\n")
                          .Replace("\\t", "\t")
                          .Replace("\\r", "\r");

            return result;
        }

        /// <summary>
        /// Gets status message from configuration or returns default
        /// </summary>
        private string GetStatusMessage(string status)
        {
            var configKey = status.Replace(" ", ""); // Remove spaces for config key
            var message = _configuration[$"StatusMessages:{configKey}"];

            if (!string.IsNullOrEmpty(message))
            {
                return message;
            }

            // Fallback to hardcoded messages
            return status switch
            {
                "Confirmed" => "Your order has been confirmed and will start preparation soon.",
                "Preparing" => "Your order is now being prepared by our kitchen staff. It won't be long now!",
                "Ready" => "Your order is ready! Our delivery partner will pick it up and deliver it to you shortly.",
                "On the Way" => "Your order is on the way! You have 2 hours to confirm receipt once it's delivered.",
                "Received" => "Thank you for confirming receipt of your order. We hope you enjoyed your meal!",
                _ => $"Your order status has been updated to: {status}"
            };
        }

        /// <summary>
        /// Provides fallback templates when configuration is missing
        /// </summary>
        private (string Subject, string Body) GetFallbackTemplate(string templateName, object templateData)
        {
            var customerName = GetPropertyValue(templateData, "customerName") ?? "Valued Customer";
            var orderNumber = GetPropertyValue(templateData, "orderNumber") ?? "Unknown";

            return templateName switch
            {
                "OrderConfirmation" => (
                    $"Order Confirmation - Your AlliEats Order #{orderNumber}",
                    $"Dear {customerName},\n\nThank you for placing your order with AlliEats! Your order #{orderNumber} has been received.\n\nBest regards,\nThe AlliEats Team"
                ),
                "OrderStatusUpdate" => (
                    $"Order Update - Order #{orderNumber}",
                    $"Dear {customerName},\n\nYour order #{orderNumber} status has been updated.\n\nBest regards,\nThe AlliEats Team"
                ),
                "OrderReady" => (
                    $"Order Ready - Order #{orderNumber}",
                    $"Dear {customerName},\n\nYour order #{orderNumber} is ready!\n\nBest regards,\nThe AlliEats Team"
                ),
                "OrderDelivered" => (
                    $"Order Out for Delivery - Order #{orderNumber}",
                    $"Dear {customerName},\n\nYour order #{orderNumber} is on the way!\n\nBest regards,\nThe AlliEats Team"
                ),
                "OrderCancelled" => (
                    $"Order Cancelled - Order #{orderNumber}",
                    $"Dear {customerName},\n\nYour order #{orderNumber} has been cancelled.\n\nBest regards,\nThe AlliEats Team"
                ),
                _ => (
                    "AlliEats Notification",
                    $"Dear {customerName},\n\nThank you for choosing AlliEats!\n\nBest regards,\nThe AlliEats Team"
                )
            };
        }

        /// <summary>
        /// Helper method to get property value from object using reflection
        /// </summary>
        private string GetPropertyValue(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property?.GetValue(obj)?.ToString();
        }

        /// <summary>
        /// Sends email using EmailJS universal template
        /// </summary>
        private async Task<bool> SendUniversalEmailAsync(string email, string customerName, string subject, string message, object additionalParams, string emailType)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                // Get EmailJS configuration - use the universal template
                var emailJsServiceId = _configuration["EmailJS:ServiceId"];
                var emailJsTemplateId = _configuration["EmailJS:UniversalTemplateId"]
                                      ?? _configuration["EmailJS:TemplateId"]; // Fallback to OTP template
                var emailJsPublicKey = _configuration["EmailJS:PublicKey"];
                var emailJsPrivateKey = _configuration["EmailJS:PrivateKey"];

                _logger.LogInformation("Sending {EmailType} email to {Email}", emailType, email);

                // Check if configuration is missing
                if (string.IsNullOrEmpty(emailJsServiceId) || string.IsNullOrEmpty(emailJsTemplateId) || string.IsNullOrEmpty(emailJsPublicKey))
                {
                    _logger.LogError("EmailJS configuration is missing or incomplete for order notifications");
                    return false;
                }

                // Build base template parameters
                var templateParams = new
                {
                    to_email = email,
                    to_name = customerName,
                    subject = subject,
                    message = message,
                    app_name = "AlliEats",
                    app_name_lower = "allieats",
                    email_type = emailType
                };

                // Merge with additional parameters using dynamic object
                var allParams = MergeObjects(templateParams, additionalParams);

                var emailData = new
                {
                    service_id = emailJsServiceId,
                    template_id = emailJsTemplateId,
                    user_id = emailJsPublicKey,
                    accessToken = emailJsPrivateKey,
                    template_params = allParams
                };

                var json = JsonSerializer.Serialize(emailData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

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

        /// <summary>
        /// Merges two objects into a dynamic object for template parameters
        /// </summary>
        private dynamic MergeObjects(object baseObj, object additionalObj)
        {
            var result = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;

            // Add properties from base object
            foreach (var property in baseObj.GetType().GetProperties())
            {
                result[property.Name] = property.GetValue(baseObj);
            }

            // Add properties from additional object
            if (additionalObj != null)
            {
                foreach (var property in additionalObj.GetType().GetProperties())
                {
                    result[property.Name] = property.GetValue(additionalObj);
                }
            }

            return result;
        }

        /// <summary>
        /// Legacy method for backward compatibility
        /// </summary>
        private async Task<bool> SendEmailAsync(string email, string customerName, string subject, string message, string emailType)
        {
            return await SendUniversalEmailAsync(email, customerName, subject, message, null, emailType);
        }
    }
}