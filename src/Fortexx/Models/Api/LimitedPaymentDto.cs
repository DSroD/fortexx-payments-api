using System;

namespace Fortexx.Models.Api {

    public record LimitedPaymentDto {
        public int Id { get; init; }
        public DateTime PaymentDate { get; init; }
        public string PaymentType { get; init; }
        public float Value { get; init; }
        public string Currency { get; init; }
        public string User { get; init; }
        public int? ServerId { get; init; }
        public string? ServerName { get; init; }
        public int? ProductId { get; init; }
        public string? ProductName { get; init; }
        public bool Activated { get; init; }
    }

}
