using System;
using System.Collections.Generic;

namespace Fortexx.Models.DiscordAnnounce {
    public record DiscordEmbed {
        public string Title { get; init; }
        public string Description { get; init; }
        public int Color { get; init; }
        
        public List<DiscordEmbedField> Embeds { get; init; }
    }
}