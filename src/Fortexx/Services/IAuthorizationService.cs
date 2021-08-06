using System;

namespace Fortexx.Services {

    public interface IAuthorizationService {
        
        public bool HasLimitedView(string key);

        public bool HasFullView(string key);

        public bool HasSuperUserView(string key);

    }

    public enum ViewType{
        LIMITED,
        FULL,
        SUPERUSER
    }

}