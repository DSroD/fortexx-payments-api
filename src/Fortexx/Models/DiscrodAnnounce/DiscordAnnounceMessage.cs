using System;
using System.Collections.Generic;

namespace Fortexx.Models.DiscordAnnounce {
    public record DiscordAnnounceMessage {
        public string Content { get; init; }
        public List<DiscordEmbed> Embeds { get; init; }
    }
}