using System;
using System.Collections.Generic;

namespace Fortexx.Models {
    public class Product {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CodeName { get; set; }
        public float PriceEur { get; set; }
        public float PriceCzk { get; set; }
        public int GameServerId { get; set; }
        public GameServer GameServer { get; set; }
        public string Information { get; set; }
        public virtual List<Payment> Payments { get; set; }
    }
}