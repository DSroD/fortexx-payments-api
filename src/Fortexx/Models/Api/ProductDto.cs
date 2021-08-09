using System;

namespace Fortexx.Models.Api {
    public record ProductDto {
        public int Id { get; init; }
        public string Name { get; init; }
        public string CodeName { get; init; }
        public float PriceEur { get; init; }
        public float PriceCzk { get; init; }
        public int GameServerId { get; init; }
        public string GameServer { get; init; }
        public string Information { get; init; }
    }
}