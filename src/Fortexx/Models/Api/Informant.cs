using System;

namespace Fortexx.Models.Api {

    public record Informant {

        public int Id { get; init; }
        public string Nickname { get; init; }
        public string Country { get; init; }
        public string Type { get; init; }
        public string Info { get; init; }
        public int? ProductId { get; init; }
    }

}