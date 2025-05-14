using System;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WhatsAppNotifier.Services;

namespace WhatsAppNotifier.Services
{
    public class SchedulerService
    {
        private readonly ILogger<SchedulerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly BackgroundMessagingService _backgroundMessagingService;
        private bool _isScheduled = false;

        public SchedulerService(
            ILogger<SchedulerService> logger,
            IConfiguration configuration,
            BackgroundMessagingService backgroundMessagingService)
        {
            _logger = logger;
            _configuration = configuration;
            _backgroundMessagingService = backgroundMessagingService;
        }

        public void ScheduleReportAt(TimeSpan timeOfDay)
        {
            // Stop any existing scheduled job first
            RemoveExistingSchedule();

            // Create a cron expression for specific time
            string hour = timeOfDay.Hours.ToString();
            string minute = timeOfDay.Minutes.ToString();
            string cronExpression = $"{minute} {hour} * * *"; // Format: "minute hour * * *"

            _logger.LogInformation($"Scheduling daily sales report job at {hour}:{minute} every day");

            RecurringJob.AddOrUpdate(
                "send-daily-sales-report",
                () => _backgroundMessagingService.SendSalesReportJob(),
                cronExpression);

            _isScheduled = true;

            _logger.LogInformation($"Job scheduled. Cron expression: {cronExpression}");
        }

        public void ScheduleReportEveryMinutes(int minutes)
        {
            // Stop any existing scheduled job first
            RemoveExistingSchedule();

            _logger.LogInformation($"Scheduling sales report job to run every {minutes} minutes");

            string cronExpression = $"*/{minutes} * * * *"; // Run every X minutes

            RecurringJob.AddOrUpdate(
                "send-daily-sales-report",
                () => _backgroundMessagingService.SendSalesReportJob(),
                cronExpression);

            _isScheduled = true;

            _logger.LogInformation($"Job scheduled. Cron expression: {cronExpression}");
        }

        public void ScheduleReportEverySeconds(int seconds)
        {
            // Stop any existing scheduled job first
            RemoveExistingSchedule();

            _logger.LogInformation($"Scheduling sales report job to run every {seconds} seconds");

            string cronExpression = $"*/{seconds} * * * * *"; // Run every X seconds

            RecurringJob.AddOrUpdate(
                "send-daily-sales-report",
                () => _backgroundMessagingService.SendSalesReportJob(),
                cronExpression);

            _isScheduled = true;

            _logger.LogInformation($"Job scheduled. Cron expression: {cronExpression}");
        }

        public void RemoveExistingSchedule()
        {
            if (_isScheduled)
            {
                _logger.LogInformation("Removing existing schedule");
                RecurringJob.RemoveIfExists("send-daily-sales-report");
                _isScheduled = false;
            }
        }

        public bool IsScheduled => _isScheduled;
    }
}