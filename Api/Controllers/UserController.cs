using Api.Consts;
using Api.Models.Attach;
using Api.Models.Subscription;
using Api.Models.User;
using Api.Services;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[ApiExplorerSettings(GroupName = "Api")]
	[Authorize]
	public class UserController : ControllerBase
	{
		private readonly UserService _userService;
		private readonly AttachService _attachService;
		public UserController(UserService userService, AttachService attachService, LinkGeneratorService links)
		{
			_userService = userService;
			_attachService = attachService;
			links.LinkContentGenerator = x => Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new
			{
				userId = x.Id
			});
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
		public async Task<IEnumerable<UserAvatarModel>> GetUsers() => await _userService.GetUsers();

		[HttpGet]
		public async Task<UserAvatarModel> GetCurrentUser()
		{
			var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
			if (userId == default)
				throw new Exception("you are not authorized");
			return await _userService.GetUser(userId);
		}

		[HttpGet]
		public async Task<UserAvatarModel> GetUserById(Guid userId) =>
			await _userService.GetUser(userId);

		[HttpPost]
		public async Task SubscribeToUser(Guid publisherId)
		{
			var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
			if (userId == default)
				throw new Exception("you are not authorized");
			var model = new SubscriptionModel
			{
				PublisherId = publisherId,
				SubscriberId = userId
			};
			await _userService.SubscribeToUser(model);
		}

		[HttpPost]
		public async Task UnsubscribeFromUser(Guid publisherId)
		{
			var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
			if (userId == default)
				throw new Exception("you are not authorized");
			var model = new SubscriptionModel
			{
				PublisherId = publisherId,
				SubscriberId = userId
			};
			await _userService.UnsubscribeFromUser(model);
		}

		[HttpGet]
		public async Task<IEnumerable<UserAvatarModel>> GetSubscribers(Guid userId) =>
			await _userService.GetSubscribers(userId);

		[HttpGet]
		public async Task<IEnumerable<UserAvatarModel>> GetPublishers(Guid userId) =>
			await _userService.GetPublishers(userId);
	}
}
