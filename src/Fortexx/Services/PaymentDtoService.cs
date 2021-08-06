using Fortexx.Models;
using Fortexx.Models.Api;


namespace Fortexx.Services {
    public class PaymentDtoService : IDtoService<Payment, PaymentDto> {
        public PaymentDto GetDto(Payment model) {
            return new PaymentDto {
                Id = model.Id,
                PaymentId = model.PaymentId,
                PaymentDate = model.PaymentDate,
                PaymentType = model.PaymentType,
                Value = model.Value,
                Currency = model.Currency,
                User = model.User,
                ServerId = model.ServerId,
                ServerName = model.Server?.Name ?? "?",
                ProductId = model.ProductId,
                ProductName = model.Product?.Name ?? "?",
                MainInfo = model.MainInfo,
                OtherInfo = model.OtherInfo,
                Status = model.Status,
                Activated = model.Activated
            };
        }

        public Payment GetModel(PaymentDto dto) {
            return new Payment{
                Id = dto.Id,
                PaymentId = dto.PaymentId,
                PaymentDate = dto.PaymentDate,
                PaymentType = dto.PaymentType,
                Value = dto.Value,
                Currency = dto.Currency,
                User = dto.User,
                ServerId = dto.ServerId,
                ProductId = dto.ProductId,
                MainInfo = dto.MainInfo,
                OtherInfo = dto.OtherInfo,
                Status = dto.Status,
                Activated = dto.Activated
            };
        }
    }
}