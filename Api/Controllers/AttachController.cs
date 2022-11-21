using Api.Consts;
using Api.Models.Attach;
using Api.Services;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize]
	public class AttachController : ControllerBase
	{
		private readonly AttachService _attachService;
		private readonly UserService _userService;
		private readonly PostService _postService;
		public AttachController(AttachService attachService, UserService userService, PostService postService)
		{
			_attachService = attachService;
			_userService = userService;
			_postService = postService;
		}

		[HttpPost]
		public async Task<List<MetadataModel>> UploadFiles([FromForm] List<IFormFile> files) => await _attachService.UploadFiles(files);


		[HttpGet]
		[Route("{userId}")]
		public async Task<FileStreamResult> GetUserAvatar(Guid userId, bool download = false) =>
			RenderAttach(await _userService.GetUserAvatar(userId), download);		

		[HttpGet]
		public async Task<FileStreamResult> GetCurrentUserAvatar(bool download = false) => 
			await GetUserAvatar(User.GetClaimValue<Guid>(ClaimNames.Id), download);

		[HttpGet]
		[Route("{postContentId}")]
		public async Task<FileResult> GetPostContent(Guid postContentId, bool download = false) =>
			RenderAttach(await _postService.GetPostContent(postContentId), download);
		

		private FileStreamResult RenderAttach(AttachModel attach, bool download)
		{
			var fs = new FileStream(attach.FilePath, FileMode.Open);
			var extension = Path.GetExtension(attach.Name);
			if (download)
				return File(fs, attach.MimeType, $"{attach.Id}{extension}");
			else
				return File(fs, attach.MimeType);
		}
	}
}