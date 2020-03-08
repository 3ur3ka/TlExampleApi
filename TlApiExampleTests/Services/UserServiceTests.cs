using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using TlApiExample.Entities;
using TlApiExample.Services;
using Xunit;

namespace TlApiExampleTests
{
    public class UserServiceTests
    {
        private readonly Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        private readonly Mock<IAuthenticationService> authServiceMock = new Mock<IAuthenticationService>();
        private readonly Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
        private HttpContext context;

        private readonly IUserService userService;

        public UserServiceTests()
        {
            SetupAuthServices();

            // Service under test
            userService = new UserService(httpContextAccessorMock.Object);
        }

        private void SetupAuthServices()
        {
            authServiceMock
                .Setup(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.FromResult((object)null));

            authServiceMock
                .Setup(_ => _.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.FromResult((object)null));

            serviceProviderMock
                .Setup(_ => _.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            context = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };

            httpContextAccessorMock.Setup(ca => ca.HttpContext).Returns(context);
        }

        [Fact]
        public async Task TestInvalidCredentialsAsync()
        {
            // Arrange

            // Act
            User user = await userService.AuthenticateAsync("test", "test");

            // Assert
            Assert.True(user == null);
            authServiceMock.Verify(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Never);
        }

        [Fact]
        public async Task TestInvalidPasswordAsync()
        {
            // Arrange

            // Act
            User user = await userService.AuthenticateAsync("john", "test");

            // Assert
            Assert.True(user == null);
            authServiceMock.Verify(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Never);

        }

        [Fact]
        public async Task TestMissingUsernameAsync()
        {
            // Arrange

            // Act
            User user = await userService.AuthenticateAsync(null, "test");

            // Assert
            Assert.True(user == null);
            authServiceMock.Verify(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Never);

        }

        [Fact]
        public async Task TestMissingPasswordAsync()
        {
            // Arrange

            // Act
            User user = await userService.AuthenticateAsync("test", null);

            // Assert
            Assert.True(user == null);
            authServiceMock.Verify(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Never);

        }

        [Fact]
        public async Task TestMissingCredentialsAsync()
        {
            // Arrange

            // Act
            User user = await userService.AuthenticateAsync(null, null);

            // Assert
            Assert.True(user == null);
            authServiceMock.Verify(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Never);

        }

        [Fact]
        public async Task TestValidCredentialsAsync()
        {
            // Arrange

            // Act
            User user = await userService.AuthenticateAsync("john", "doe");

            // Assert
            Assert.True(user != null);
            Assert.True(user.Username == "john");
            Assert.True(user.PasswordHash == null);
            Assert.True(user.PasswordSalt == null);
            authServiceMock.Verify(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Once);

        }

        [Fact]
        public async Task TestLogoutWhenNotLoggedIn()
        {
            // Arrange

            // Act
            await userService.Logout();

            // Assert
            authServiceMock.Verify(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Never);
            authServiceMock.Verify(_ => _.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Once);
        }

        [Fact]
        public async Task TestLogoutWhenUserIsLoggedIn()
        {
            // Arrange
            await userService.AuthenticateAsync("john", "doe");

            // Act
            await userService.Logout();

            // Assert
            authServiceMock.Verify(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Once);
            authServiceMock.Verify(_ => _.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Once);

        }


    }
}
