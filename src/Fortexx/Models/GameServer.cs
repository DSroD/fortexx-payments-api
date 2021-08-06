using System;
using System.Collections.Generic;

namespace Fortexx.Models {
    public record GameServer {
        public int Id {get; init; }
        public string Name { get; init; }
        public string Game { get; init; }
        public string IconURL { get; init; }
        public string Information { get; init; }
        public virtual List<Product> Products { get; init; }
        public virtual List<Payment> Payments { get; init; }

    }
}