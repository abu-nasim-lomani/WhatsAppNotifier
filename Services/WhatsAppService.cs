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
        private string _accessToken;
        private string _phoneNumberId;
        private string _businessAccountId;
        private readonly List<string> _recipientNumbers;
        private static readonly List<WhatsAppMessageModel> _messageHistory = new List<WhatsAppMessageModel>();

        public WhatsAppService(IConfiguration configuration, ILogger<WhatsAppService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _accessToken = _configuration["WhatsApp:AccessToken"];
            _phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];
            _businessAccountId = _configuration["WhatsApp:WhatsAppBusinessAccountId"];

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
            _logger.LogInformation($"Using Phone Number ID: {_phoneNumberId}");
            _logger.LogInformation($"Using Business Account ID: {_businessAccountId}");
        }

        public async Task<List<WhatsAppMessageModel>> SendDailySalesReport(
            string date = null,
            decimal totalSales = 0,
            int totalCustomers = 0,
            int newCustomers = 0,
            int grandTotalCustomers = 0,
            int totalVisited = 0,
            int visitedNoPurchase = 0,
            int newCustomersNoPurchase = 0)
        {
            // If no date provided, use current date
            if (string.IsNullOrEmpty(date))
            {
                date = DateTime.Now.ToString("dd MMM yyyy");
            }

            // Format the sales report message
            string messageText =
                $"🗓 Daily Sales and Customer Report\n" +
                $"Date: {date}\n" +
                $"Total Sales: {totalSales:N2} BDT\n" +
                $"👥 Customer Summary\n" +
                $"Total Customers: {totalCustomers}\n" +
                $"New Customers: {newCustomers}\n" +
                $"Grand Total Customers (Cumulative): {grandTotalCustomers}\n" +
                $"🧴 Treatment Activity\n" +
                $"Total Visited for Treatment: {totalVisited}\n" +
                $"Visited for Treatment but Did Not Purchase: {visitedNoPurchase}\n" +
                $"New Customers Consulted but Did Not Purchase: {newCustomersNoPurchase}";

            var results = new List<WhatsAppMessageModel>();

            // Send the message to all recipients
            foreach (var recipientNumber in _recipientNumbers)
            {
                var message = await SendTextMessage(recipientNumber, messageText);
                results.Add(message);
            }

            return results;
        }

        public async Task<List<WhatsAppMessageModel>> SendStaticSalesReport()
        {
            // Static sample data as per your example
            string date = DateTime.Now.ToString("dd MMM yyyy");
            decimal totalSales = 1402127.52M;
            int totalCustomers = 358;
            int newCustomers = 113;
            int grandTotalCustomers = 510;
            int totalVisited = 309;
            int visitedNoPurchase = 169;
            int newCustomersNoPurchase = 36;

            return await SendDailySalesReport(
                date,
                totalSales,
                totalCustomers,
                newCustomers,
                grandTotalCustomers,
                totalVisited,
                visitedNoPurchase,
                newCustomersNoPurchase);
        }

        public async Task<WhatsAppMessageModel> SendTextMessage(string recipientNumber, string messageText)
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

            var body = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = recipientNumber,
                type = "text",
                text = new { preview_url = false, body = messageText }
            };

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

        public void UpdateAccessToken(string newToken)
        {
            _accessToken = newToken;
            _logger.LogInformation("Access token updated in-memory");
        }

        public void UpdatePhoneNumberId(string newPhoneNumberId)
        {
            _phoneNumberId = newPhoneNumberId;
            _logger.LogInformation($"Phone Number ID updated in-memory to: {newPhoneNumberId}");
        }

        public void UpdateBusinessAccountId(string newBusinessAccountId)
        {
            _businessAccountId = newBusinessAccountId;
            _logger.LogInformation($"WhatsApp Business Account ID updated in-memory to: {newBusinessAccountId}");
        }

        public List<WhatsAppMessageModel> GetMessageHistory()
        {
            return _messageHistory.OrderByDescending(m => m.SentTime).ToList();
        }

        public string GetPhoneNumberId() => _phoneNumberId;
        public string GetBusinessAccountId() => _businessAccountId;
    }
}