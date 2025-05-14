using System;

namespace WhatsAppNotifier.Models
{
    public class WhatsAppMessageModel
    {
        public string Id { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime SentTime { get; set; }
        public DateTime? DeliveredTime { get; set; }
        public string Message { get; set; }
        public string RecipientNumber { get; set; }
        public string Error { get; set; }
        public bool IsDelivered { get; set; }
        public bool IsSent { get; set; }
    }
}