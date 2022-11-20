using Api.Consts;
using Api.Models.Attach;
using Api.Models.User;
using Api.Services;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize]
	public class UserController : ControllerBase
	{
		private readonly UserService _userService;
		private readonly AttachService _attachService;
		public UserController(UserService userService, AttachService attachService)
		{
			_userService = userService;
			_attachService = attachService;
			if (userService != null)
				_userService.SetLinkGenerator(x =>
				Url.Action(nameof(GetUserAvatar), new { userId = x.Id, download = false }));
		}

		[HttpPost]
		public async Task AddAvatarToUser(MetadataModel model)
		{
			var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
			if (userId != default)
			{
				var path = _attachService.SaveMetaToAttaches(model);

				await _userService.AddAvatarToUser(userId, model, path);
				
			}
			else
				throw new Exception("you are not authorized");
		}

		[HttpGet]
		public async Task<FileStreamResult> GetUserAvatar(Guid userId, bool download = false)
		{
			
			var attach = await _userService.GetUserAvatar(userId);
			var fs = new FileStream(attach.FilePath, FileMode.Open);
			if (download)
				return File(fs, attach.MimeType, attach.Name);
			else
				return File(fs, attach.MimeType);
		}

		[HttpGet]
		public async Task<FileStreamResult> GetCurrentUserAvatar(bool download = false)
		{
			var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
			if (userId != default)
				return await GetUserAvatar(userId, download);		
            else
                throw new Exception("you are not authorized");
		}

		[HttpGet]
		public async Task<IEnumerable<UserAvatarModel>> GetUsers() => await _userService.GetUsers();

		[HttpGet]
		public async Task<UserAvatarModel> GetCurrentUser()
		{
			var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
			if (userId != default)
			{
				return await _userService.GetUser(userId);
			}
			else
				throw new Exception("you are not authorized");
		}
	}
}
