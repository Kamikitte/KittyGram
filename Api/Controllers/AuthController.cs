﻿using Api.Models.Token;
using Api.Models.User;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
	[ApiController]
	[ApiExplorerSettings(GroupName ="Auth")]
	public class AuthController : ControllerBase
	{
		private readonly AuthService _authService;
		private readonly UserService _userService;

		public AuthController(UserService userService, AuthService authService)
		{
			_userService = userService;
			_authService = authService;
		}
		[HttpPost]
		public async Task RegisterUser(CreateUserModel model)
		{
			if (await _userService.CheckUserExist(model.Email))
				throw new Exception("user is exist");
			await _userService.CreateUser(model);
		}

		[HttpPost]
		public async Task<TokenModel> Token(TokenRequestModel model) 
			=> await _authService.GetToken(model.Login, model.Password);

		[HttpPost]
		public async Task<TokenModel> RefreshToken(RefreshTokenRequestModel model) 
			=> await _authService.GetTokenByRefreshToken(model.RefreshToken);
	}
}
