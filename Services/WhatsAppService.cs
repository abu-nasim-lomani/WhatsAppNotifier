using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using WhatsAppNotifier.Models;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace WhatsAppNotifier.Services
{
    public class WhatsAppService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly string _accessToken;
        private readonly string _phoneNumberId;
        private readonly List<string> _recipientNumbers;
        private static readonly List<WhatsAppMessageModel> _messageHistory = new List<WhatsAppMessageModel>();

        public WhatsAppService(IConfiguration configuration, ILogger<WhatsAppService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _accessToken = _configuration["WhatsApp:AccessToken"];
            _phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];

            // Get recipient numbers from configuration
            _recipientNumbers = _configuration.GetSection("WhatsApp:RecipientNumbers").Get<List<string>>();

            // Fallback if the new config format isn't found
            if (_recipientNumbers == null || !_recipientNumbers.Any())
            {
                _recipientNumbers = new List<string>();
                var singleRecipient = _configuration["WhatsApp:RecipientNumber"];
                if (!string.IsNullOrEmpty(singleRecipient))
                {
                    _recipientNumbers.Add(singleRecipient);
                }
            }

            _logger.LogInformation($"Initialized WhatsAppService with {_recipientNumbers.Count} recipients");
        }

        public async Task<List<WhatsAppMessageModel>> SendWhatsAppMessages()
        {
            var results = new List<WhatsAppMessageModel>();
            string messageText = $"Test message sent at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            foreach (var recipientNumber in _recipientNumbers)
            {
                var message = await SendSingleWhatsAppMessage(recipientNumber, messageText);
                results.Add(message);
            }

            return results;
        }

        private async Task<WhatsAppMessageModel> SendSingleWhatsAppMessage(string recipientNumber, string messageText)
        {
            var message = new WhatsAppMessageModel
            {
                Id = Guid.NewGuid().ToString(),
                Message = messageText,
                RecipientNumber = recipientNumber,
                SentTime = DateTime.Now
            };

            var client = new RestClient($"https://graph.facebook.com/v17.0/{_phoneNumberId}/messages");
            var request = new RestRequest { Method = Method.Post };

            request.AddHeader("Authorization", $"Bearer {_accessToken}");
            request.AddHeader("Content-Type", "application/json");

            // Use template message for better deliverability
            var body = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = recipientNumber,
                type = "template",
                template = new
                {
                    name = "hello_world",
                    language = new { code = "en_US" }
                }
            };

            // Uncomment below and comment the template above if you want to use free-form text
            // (Only works if within 24-hour window)
            /*
            var body = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = recipientNumber,
                type = "text",
                text = new { preview_url = false, body = messageText }
            };
            */

            request.AddJsonBody(JsonConvert.SerializeObject(body));

            try
            {
                var response = await client.ExecuteAsync(request);
                _logger.LogInformation($"API Response for {recipientNumber}: {response.Content}");

                if (response.IsSuccessful)
                {
                    var responseData = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    message.Id = responseData?.messages[0]?.id;
                    message.Status = "sent";
                    message.IsSent = true;
                    _logger.LogInformation($"Message sent successfully to {recipientNumber}: {message.Id}");
                }
                else
                {
                    message.Status = "failed";
                    message.Error = response.Content;
                    _logger.LogError($"Failed to send message to {recipientNumber}: {response.Content}");
                }
            }
            catch (Exception ex)
            {
                message.Status = "error";
                message.Error = ex.Message;
                _logger.LogError($"Error sending message to {recipientNumber}: {ex.Message}");
            }

            _messageHistory.Add(message);
            if (_messageHistory.Count > 100)
            {
                _messageHistory.RemoveAt(0);
            }

            return message;
        }

        public async Task<WhatsAppMessageModel> SendWhatsAppMessageWithRetry(int maxRetries = 3)
        {
            List<WhatsAppMessageModel> messages = null;
            int attempts = 0;
            bool allSuccess = false;

            while (!allSuccess && attempts < maxRetries)
            {
                attempts++;
                messages = await SendWhatsAppMessages();
                allSuccess = messages.All(m => m.IsSent);

                if (!allSuccess && attempts < maxRetries)
                {
                    // Wait before retrying (exponential backoff)
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempts)));
                }
            }

            // Return the first message for backward compatibility
            // (the calling code expects a single message)
            return messages?.FirstOrDefault();
        }

        public void UpdateMessageStatus(string messageId, string status)
        {
            var message = _messageHistory.FirstOrDefault(m => m.Id == messageId);
            if (message != null)
            {
                message.Status = status;
                if (status == "delivered")
                {
                    message.IsDelivered = true;
                    message.DeliveredTime = DateTime.Now;
                }
                _logger.LogInformation($"Updated message {messageId} status to {status}");
            }
        }

        public List<WhatsAppMessageModel> GetMessageHistory()
        {
            return _messageHistory.OrderByDescending(m => m.SentTime).ToList();
        }
    }
}