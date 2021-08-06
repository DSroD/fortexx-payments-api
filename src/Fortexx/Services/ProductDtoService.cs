using Fortexx.Models;
using Fortexx.Models.Api;


namespace Fortexx.Services {
    public class ProductDtoService : IDtoService<Product, ProductDto> {

        public ProductDto GetDto(Product model) {
            return new ProductDto {
                Id = model.Id,
                Name = model.Name,
                PriceEur = model.PriceEur,
                PriceCzk = model.PriceCzk,
                GameServerId = model.GameServerId,
                GameServer = model.GameServer?.Name ?? "?",
                Information = model.Information
            };
        }

        public Product GetModel(ProductDto dto) {
            return new Product {
                Id = dto.Id,
                Name = dto.Name,
                PriceEur = dto.PriceEur,
                PriceCzk = dto.PriceCzk,
                GameServerId = dto.GameServerId,
                Information = dto.Information
            };
        }
    }
}