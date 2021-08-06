using System;

namespace Fortexx.Models.Api {

    public record SMSPayment {
        public int Id { get; init; }
        public string Sms { get; init; }
        public int ShortCode { get; init; }
    }

}