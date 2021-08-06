using System;

using Microsoft.Extensions.Configuration;

namespace Fortexx.Services {

    public class AuthorizazionService : IAuthorizationService {
        
        private readonly IConfiguration _configuration;

        public AuthorizazionService(IConfiguration configuration) {
            _configuration = configuration;
        }

        public bool HasLimitedView(string key) {
            return HasFullView(key) || (key == _configuration["Route:LimitedKey"]);
        }

        public bool HasFullView(string key) {
            return HasSuperUserView(key) || (key == _configuration["Route:Key"]);
        }

        public bool HasSuperUserView(string key) {
            return (key == _configuration["Route:SuperUserKey"]);
        }

    }

}