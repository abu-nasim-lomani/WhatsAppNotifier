using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using WhatsAppNotifier.Models;
using WhatsAppNotifier.Services;

namespace WhatsAppNotifier.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly WhatsAppService _whatsAppService;

        public WebhookController(WhatsAppService whatsAppService)
        {
            _whatsAppService = whatsAppService;
        }

        [HttpGet]
        public IActionResult Get([FromQuery(Name = "hub.mode")] string mode,
                              [FromQuery(Name = "hub.challenge")] string challenge,
                              [FromQuery(Name = "hub.verify_token")] string token)
        {
            // Your verification logic here
            // For example, verify token against a stored token
            // If verified, return the challenge
            return Ok(challenge);
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var notification = JsonConvert.DeserializeObject<MessageStatusModel>(body);

            if (notification?.Entries != null) // Changed from Entry to Entries
            {
                foreach (var entry in notification.Entries) // Changed from Entry to Entries
                {
                    foreach (var change in entry.Changes)
                    {
                        if (change.Value?.Statuses != null)
                        {
                            foreach (var status in change.Value.Statuses)
                            {
                                _whatsAppService.UpdateMessageStatus(status.Id, status.StatusState); // Changed from Status to StatusState
                            }
                        }
                    }
                }
            }

            return Ok();
        }
    }
}