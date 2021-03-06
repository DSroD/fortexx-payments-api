using Fortexx.Models;
using Fortexx.Models.Api;


namespace Fortexx.Services {
    public class ServerDtoService : IDtoService<GameServer, GameServerDto> {

        public GameServerDto GetDto(GameServer model) {
            return new GameServerDto {
                Id = model.Id,
                Name = model.Name,
                CodeName = model.CodeName,
                Game = model.Game,
                IconURL = model.IconURL,
                Information = model.Information
            };
        }

        public GameServer GetModel(GameServerDto dto) {
            return new GameServer {
                Id = dto.Id,
                Name = dto.Name,
                CodeName = dto.CodeName,
                Game = dto.Game,
                IconURL = dto.IconURL,
                Information = dto.Information
            };
        }
    }
}