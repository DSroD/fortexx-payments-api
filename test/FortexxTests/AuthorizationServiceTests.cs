using Xunit;
using Moq;

using Microsoft.Extensions.Configuration;

using Fortexx.Services;

namespace FortexxTests {

    public class AuthorizazionServiceTests {

        private IConfiguration _configuration;

        public AuthorizazionServiceTests() {
            _configuration = Utilities.BuildTestConfiguration();
        }

        [Fact]
        public void HasLimitedViewTest() {
            var authServ = new AuthorizazionService(_configuration);

            var result = authServ.HasLimitedView("notAKey");
            Assert.False(result);

            result = authServ.HasLimitedView("limitedKey");
            Assert.True(result);

            result = authServ.HasLimitedView("key");
            Assert.True(result);

            result = authServ.HasLimitedView("superUserKey");
            Assert.True(result);
        }

        [Fact]
        public void HasFullViewTest() {
            var authServ = new AuthorizazionService(_configuration);

            var result = authServ.HasFullView("notAKey");
            Assert.False(result);

            result = authServ.HasFullView("limitedKey");
            Assert.False(result);

            result = authServ.HasFullView("key");
            Assert.True(result);

            result = authServ.HasFullView("superUserKey");
            Assert.True(result);
        }

        [Fact]
        public void HasSuperUserViewTest() {
            var authServ = new AuthorizazionService(_configuration);

            var result = authServ.HasSuperUserView("notAKey");
            Assert.False(result);

            result = authServ.HasSuperUserView("limitedKey");
            Assert.False(result);

            result = authServ.HasSuperUserView("key");
            Assert.False(result);

            result = authServ.HasSuperUserView("superUserKey");
            Assert.True(result);
        }


    }

}