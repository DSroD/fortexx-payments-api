using System;
using System.Collections.Generic;

namespace Fortexx.Models {
    public record Product {
        public int Id { get; init; }
        public string Name { get; init; }
        public float PriceEur { get; init; }
        public float PriceCzk { get; init; }
        public int GameServerId { get; init; }
        public GameServer GameServer { get; init; }
        public string Information { get; init; }
        public virtual List<Payment> Payments { get; init; }
    }
}