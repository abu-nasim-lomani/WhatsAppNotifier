using System.Collections.Generic;

namespace WhatsAppNotifier.Models
{
    public class MessageStatusModel
    {
        public string Object { get; set; }
        public List<EntryItem> Entries { get; set; } // Renamed from Entry to Entries and type to EntryItem

        public class EntryItem // Renamed from Entry to EntryItem
        {
            public string Id { get; set; }
            public List<ChangeItem> Changes { get; set; } // Renamed from Change to ChangeItem
        }

        public class ChangeItem // Renamed from Change to ChangeItem
        {
            public ValueItem Value { get; set; } // Renamed from Value to ValueItem
            public string Field { get; set; }
        }

        public class ValueItem // Renamed from Value to ValueItem
        {
            public string MessagingProduct { get; set; }
            public MetadataItem Metadata { get; set; } // Renamed from Metadata to MetadataItem
            public List<StatusItem> Statuses { get; set; } // Renamed from Status to StatusItem
        }

        public class MetadataItem // Renamed from Metadata to MetadataItem
        {
            public string DisplayPhoneNumber { get; set; }
            public string PhoneNumberId { get; set; }
        }

        public class StatusItem // Renamed from Status to StatusItem
        {
            public string Id { get; set; }
            public string StatusState { get; set; } // Renamed from Status to StatusState
            public string Timestamp { get; set; }
            public string RecipientId { get; set; }
        }
    }
}