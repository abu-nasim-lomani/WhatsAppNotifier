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

        public async Task SendSalesReportJob()
        {
            _logger.LogInformation("Starting sales report job");
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var whatsAppService = scope.ServiceProvider.GetRequiredService<WhatsAppService>();
                    await whatsAppService.SendStaticSalesReport();
                }
                _logger.LogInformation("Completed sales report job successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in sales report job: {ex.Message}");
            }
        }
    }
}