using System;

namespace Fortexx.Models.Api {
    public record GameServerDto {
        public int Id {get; init; }
        public string Name { get; init; }

        public string CodeName { get; init; }
        public string Game { get; init; }
        public string? IconURL { get; init; }
        public string Information { get; init; }

    }
}