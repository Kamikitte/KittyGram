using Api.Models;
using Api.Services;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class PostController : ControllerBase
	{
		private readonly PostService _postService;
		private readonly UserService _userService;
		public PostController(PostService postService, UserService userService)
		{
			_postService = postService;
			_userService = userService;
			_postService.SetLinkGenerator(
				linkContentGenerator: x => Url.Action(nameof(GetContent), new
				{
					contentId = x.Id
				}));
		}
		[HttpPost]
		[Authorize]
		public async Task<Guid> CreatePost(List<MetadataModel> metaAttaches)
		{
			var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
			if (Guid.TryParse(userIdString, out var userId))
			{
				return await _postService.CreatePost(userId, metaAttaches);
			}
			else
				throw new Exception("you are not authorized");
		}
		[HttpGet]
		public async Task<PostModel> GetPost(Guid postId) => await _postService.GetPost(postId);
		[HttpGet]
		public async Task<FileResult> GetContent(Guid contentId)
		{
			var content = await _postService.GetContent(contentId);
			return File(System.IO.File.ReadAllBytes(content.FilePath), content.MimeType);
		}
	}
}
