using System;
using System.Collections.Generic;

namespace Fortexx.Models {
    public class GameServer {
        public int Id {get; set; }
        public string Name { get; set; }
        public string CodeName { get; set; }
        public string Game { get; set; }
        public string IconURL { get; set; }
        public string Information { get; set; }
        public virtual List<Product> Products { get; set; }
        public virtual List<Payment> Payments { get; set; }

    }
}