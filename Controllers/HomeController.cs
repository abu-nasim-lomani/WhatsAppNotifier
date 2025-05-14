using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WhatsAppNotifier.Models;
using WhatsAppNotifier.Services;
using Microsoft.Extensions.Logging;

namespace WhatsAppNotifier.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WhatsAppService _whatsAppService;

        public HomeController(ILogger<HomeController> logger, WhatsAppService whatsAppService)
        {
            _logger = logger;
            _whatsAppService = whatsAppService;
        }

        public IActionResult Index()
        {
            var messageHistory = _whatsAppService.GetMessageHistory();
            return View(messageHistory);
        }

        [HttpPost]
        public async Task<IActionResult> SendTestMessage()
        {
            _logger.LogInformation("Manual test message requested");
            var results = await _whatsAppService.SendWhatsAppMessages();
            _logger.LogInformation($"Sent {results.Count} test messages");
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}