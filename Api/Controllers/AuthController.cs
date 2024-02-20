using Api.Models.Token;
using Api.Models.User;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[ApiExplorerSettings(GroupName = "Auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService authService;
    private readonly UserService userService;

    public AuthController(UserService userService, AuthService authService)
    {
        this.userService = userService;
        this.authService = authService;
    }

    [HttpPost]
    public async Task RegisterUser(CreateUserModel model)
    {
        if (await userService.CheckUserExist(model.Email))
        {
            throw new Exception("user is exist");
        }

        await userService.CreateUser(model);
    }

    [HttpPost]
    public Task<TokenModel> Token(TokenRequestModel model) =>
        authService.GetToken(model.Login, model.Password);

    [HttpPost]
    public Task<TokenModel> RefreshToken(RefreshTokenRequestModel model) =>
        authService.GetTokenByRefreshToken(model.RefreshToken);
}