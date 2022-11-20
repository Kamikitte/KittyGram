using Api.Consts;
using Api.Models.Attach;
using Api.Models.Comment;
using Api.Models.Post;
using Api.Services;
using Common.Extensions;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize]
	public class PostController : ControllerBase
	{
		private readonly PostService _postService;
		private readonly UserService _userService;
		public PostController(PostService postService, UserService userService)
		{
			_postService = postService;
			_userService = userService;
			_postService.SetLinkGenerator(linkAvatarGenerator: x =>
				Url.Action(nameof(UserController.GetUserAvatar), "User", new
				{
					userId = x.Id,
					download = false
				}),
				linkContentGenerator: x => Url.Action(nameof(GetPostContent), new
				{
					postContentId = x.Id,
					download = false
				}));
		}

		[HttpPost]
		public async Task CreatePost(CreatePostRequest request)
		{
			var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
			if (userId == default)
				throw new Exception("not authorized");
			var model = new CreatePostModel
			{
				AuthorId = userId,
				Description = request.Description,
				Contents = request.Contents.Select(x => 
				new MetaWithPath(x, q => 
				Path.Combine(
					Directory.GetCurrentDirectory(), 
					"Attaches", 
					q.TempId.ToString()), userId)).ToList()
			};

			model.Contents.ForEach(x =>
			{
				var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), x.TempId.ToString()));
				if (tempFi.Exists)
				{
					var destFi = new FileInfo(x.FilePath);
					if (destFi.Directory != null && !destFi.Directory.Exists)
						destFi.Directory.Create();
					System.IO.File.Copy(tempFi.FullName, x.FilePath, true);
					tempFi.Delete();
				}
			});

			await _postService.CreatePost(model);
		}

		[HttpGet]
		public async Task<PostModel> GetPost(Guid postId) => await _postService.GetPost(postId);

		[HttpGet]
		public async Task<List<PostModel>> GetPosts(int skip = 0, int take = 10) => await _postService.GetPosts(skip, take);
		
		[HttpGet]
		public async Task<FileResult> GetPostContent(Guid postContentId, bool download = false)
		{
			var attach = await _postService.GetPostContent(postContentId);
			var fs = new FileStream(attach.FilePath, FileMode.Open);
			if (download)
				return File(fs, attach.MimeType, attach.Name);
			else
				return File(fs, attach.MimeType);
		}
		
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
