using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using TaskManager.Controllers;
using TaskManager.DTOs.Auth;
using TaskManager.InterfaceService;
using TaskManager.Models.Response;
using TaskManager.Services.Interfaces;
using Xunit;

public class AuthControllerTests
{
    //It is widely used to write and run automated tests in .NET Core and .NET applications
    //It's the default test framework for ASP.NET Core
    //Supports theory-based testing (parameterized tests)
  
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly Mock<ILogger<AuthController>> _loggerMock = new();
    //These lines create mock objects for dependencies of AuthController, using the Moq library


    private AuthController CreateController() =>
        new(_authServiceMock.Object, _loggerMock.Object, _refreshTokenServiceMock.Object);
    //This method creates an instance of AuthController using the mocked dependencies. This is useful for keeping the test code clean and DRY

    [Fact]
    public async Task RegisterUser_ReturnsBadRequest_WhenDtoIsNull()
    {
        var controller = CreateController();

        var result = await controller.RegisterUser(null);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task LoginUser_ReturnsOk_WhenLoginSuccessful()
    {
        var dto = new LoginRequestDTO { Username = "aslam@techmind.com", Password = "aslam@1234" };

        _authServiceMock.Setup(s => s.LoginUserAsync(dto, It.IsAny<string>()))
            .ReturnsAsync(new Response
            {
                ResponseCode = 0,
                ResponseDescription = "Login successful."
            });

        var controller = CreateController();

        var result = await controller.LoginUser(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<Response>(okResult.Value);
        Assert.Equal(0, response.ResponseCode);
    }
}
