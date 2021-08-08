using System;

namespace Fortexx.Models.Api {
    public record SMSDelivery {
        public string Timestamp { get; init; }
        public int Request { get; init; }
        public string Status { get; init; }
        public string Message { get; init; }
        public int Att { get; init; }
        public int Id { get; init; }
    }
}