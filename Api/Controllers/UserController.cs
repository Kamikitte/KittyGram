using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly UserService _userService;
		public UserController(UserService userService)
		{
			_userService = userService;
		}

		[HttpPost]
		public async Task CreateUser(CreateUserModel model)
		{
			if (await _userService.CheckUserExist(model.Email))
				throw new Exception("user is exist");
			await _userService.CreateUser(model);
		}

		[HttpPost]
		[Authorize]
		public async Task AddAvatarToUser(MetadataModel model)
		{
			var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
			if (Guid.TryParse(userIdString, out var userId))
			{
				var tempFI = new FileInfo(Path.Combine(Path.GetTempPath(), model.TempId.ToString()));
				if(!tempFI.Exists)
				{
					throw new Exception("file not found");
				}
				else
				{
					var path = Path.Combine(Directory.GetCurrentDirectory(), "Attaches", model.TempId.ToString());
					var destFI = new FileInfo(path);
					if(destFI.Directory != null && !destFI.Directory.Exists)
						destFI.Directory.Create();
					System.IO.File.Copy(tempFI.FullName, path, true);
					await _userService.AddAvatarToUser(userId, model, path);
				}
			}
			else
				throw new Exception("you are not authorized");
		}

		[HttpGet]
		public async Task<FileResult> GetUserAvatar(Guid userId)
		{
			
			var attach = await _userService.GetUserAvatar(userId);

			return File(System.IO.File.ReadAllBytes(attach.FilePath), attach.MimeType);
		}

		[HttpGet]
		public async Task<FileResult> DownloadAvatar(Guid userId)
		{
			var attach = await _userService.GetUserAvatar(userId);
			HttpContext.Response.ContentType = attach.MimeType;
			FileContentResult result = new FileContentResult(System.IO.File.ReadAllBytes(attach.FilePath), attach.MimeType)
			{
				FileDownloadName = attach.Name
			};
			return result;
		}

		[HttpGet]
		[Authorize]
		public async Task<List<UserModel>> GetUsers() => await _userService.GetUsers();

		[HttpGet]
		[Authorize]
		public async Task<UserModel> GetCurrentUser()
		{
			var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
			if (Guid.TryParse(userIdString, out var userId))
			{
				return await _userService.GetUser(userId);
			}
			else
				throw new Exception("you are not authorized");
		}
	}
}
