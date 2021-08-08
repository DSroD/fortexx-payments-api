using System;

namespace Fortexx.Models.Api {
    // MobilniPlatby.cz
    public record SMSPayment {
        public int Id { get; init; }
        public string Timestamp { get; init; }
        public int Phone { get; init; }
        public string Sms { get; init; }
        public int Shortcode { get; init; }
        public string Country { get; init; }
        public string Operator { get; init; }
        public int Att { get; init; }
    }

}