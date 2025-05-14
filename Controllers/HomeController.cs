using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WhatsAppNotifier.Models;
using WhatsAppNotifier.Services;

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
            ViewBag.PhoneNumberId = _whatsAppService.GetPhoneNumberId();
            ViewBag.BusinessAccountId = _whatsAppService.GetBusinessAccountId();
            var messageHistory = _whatsAppService.GetMessageHistory();
            return View(messageHistory);
        }

        [HttpPost]
        public async Task<IActionResult> SendSalesReport()
        {
            _logger.LogInformation("Manual sales report requested");
            var results = await _whatsAppService.SendStaticSalesReport();
            _logger.LogInformation($"Sent {results.Count} sales reports");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateAccessToken(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["Error"] = "Access token cannot be empty";
                return RedirectToAction("Index");
            }

            _whatsAppService.UpdateAccessToken(accessToken);
            TempData["Success"] = "Access token updated successfully";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdatePhoneNumberId(string phoneNumberId)
        {
            if (string.IsNullOrEmpty(phoneNumberId))
            {
                TempData["Error"] = "Phone Number ID cannot be empty";
                return RedirectToAction("Index");
            }

            _whatsAppService.UpdatePhoneNumberId(phoneNumberId);
            TempData["Success"] = "Phone Number ID updated successfully";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateBusinessAccountId(string businessAccountId)
        {
            if (string.IsNullOrEmpty(businessAccountId))
            {
                TempData["Error"] = "WhatsApp Business Account ID cannot be empty";
                return RedirectToAction("Index");
            }

            _whatsAppService.UpdateBusinessAccountId(businessAccountId);
            TempData["Success"] = "WhatsApp Business Account ID updated successfully";
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