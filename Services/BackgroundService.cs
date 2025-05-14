using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WhatsAppNotifier.Services;

namespace WhatsAppNotifier.Services
{
    public class BackgroundMessagingService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundMessagingService> _logger;

        public BackgroundMessagingService(IServiceProvider serviceProvider, ILogger<BackgroundMessagingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task SendWhatsAppMessageJob()
        {
            _logger.LogInformation("Starting WhatsApp message job");
            using (var scope = _serviceProvider.CreateScope())
            {
                var whatsAppService = scope.ServiceProvider.GetRequiredService<WhatsAppService>();
                await whatsAppService.SendWhatsAppMessageWithRetry();
            }
            _logger.LogInformation("Completed WhatsApp message job");
        }

        public void ScheduleRecurringJob()
        {
            _logger.LogInformation("Scheduling recurring WhatsApp message job");
            RecurringJob.AddOrUpdate(
                "send-whatsapp-message",
                () => SendWhatsAppMessageJob(),
                "*/10 * * * * *"); // Cron expression for every 30 seconds
        }
    }
}