using System;

namespace Fortexx.Models.Api
{
    public record PaymentDto {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentType { get; set; }
        public float Value { get; set; }
        public string Currency { get; set; }
        public string User { get; set; }
        public int ServerId { get; set; }
        public string? ServerName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string MainInfo { get; set; }
        public string OtherInfo { get; set; }
        public string Status { get; set; }
        public bool Activated { get; set; }
    }
}