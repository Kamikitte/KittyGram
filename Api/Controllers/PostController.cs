using Api.Consts;
using Api.Models.Comment;
using Api.Models.Post;
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
	public class PostController : ControllerBase
	{
		private readonly PostService _postService;
		private readonly UserService _userService;
		public PostController(PostService postService, UserService userService, LinkGeneratorService links)
		{
			_postService = postService;
			_userService = userService;
			links.LinkAvatarGenerator = x => Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new
			{
				userId = x.Id
			});
			links.LinkContentGenerator = x => Url.ControllerAction<AttachController>(nameof(AttachController.GetPostContent), new
			{
				postContentId = x.Id
			});
		}

		[HttpPost]
		public async Task CreatePost(CreatePostRequest request)
		{
			if (!request.AuthorId.HasValue)
			{
				var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
				if (userId == default)
					throw new Exception("not authorized");
				request.AuthorId = userId;
			}
			await _postService.CreatePost(request);			
		}

		//[HttpGet]
		//public async Task<PostModel> GetPost(Guid postId) => await _postService.GetPost(postId);

		[HttpGet]
		public async Task<List<PostModel>> GetPosts(int skip = 0, int take = 10) => await _postService.GetPosts(skip, take);
		
		[HttpPost]
		public async Task AddComment(CreateCommentRequest request)
		{
			var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
			if (Guid.TryParse(userIdString, out var userId))
			{
				var model = new CreateCommentModel
				{
					AuthorId = userId,
					PostId = request.PostId,
					Text = request.Text
				};
				await _postService.AddComment(model);
			}
			else
				throw new Exception("you are not authorized");
		}

		[HttpGet]
		public async Task<List<CommentModel>> GetCommentsFromPost(Guid postId) => await _postService.GetCommentsFromPost(postId);
	}
}
