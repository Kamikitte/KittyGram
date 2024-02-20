using Api.Models.Attach;
using Api.Services;
using Common.Constants;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[ApiExplorerSettings(GroupName = "Api")]
[Authorize]
public class AttachController : ControllerBase
{
    private readonly AttachService attachService;
    private readonly PostService postService;
    private readonly UserService userService;

    public AttachController(AttachService attachService, UserService userService, PostService postService)
    {
        this.attachService = attachService;
        this.userService = userService;
        this.postService = postService;
    }

    [HttpPost]
    public Task<List<MetadataModel>> UploadFiles([FromForm] List<IFormFile> files) => AttachService.UploadFiles(files);


    [HttpGet]
    [Route("{userId:guid}")]
    public async Task<FileStreamResult> GetUserAvatar(Guid userId, bool download = false) =>
        RenderAttach(await userService.GetUserAvatar(userId), download);

    [HttpGet]
    public Task<FileStreamResult> GetCurrentUserAvatar(bool download = false) =>
        GetUserAvatar(User.GetClaimValue<Guid>(ClaimNames.Id), download);

    [HttpGet]
    [Route("{postContentId}")]
    public async Task<FileResult> GetPostContent(Guid postContentId, bool download = false) =>
        RenderAttach(await postService.GetPostContent(postContentId), download);


    private FileStreamResult RenderAttach(AttachModel attach, bool download)
    {
        var fs = new FileStream(attach.FilePath, FileMode.Open);
        var extension = Path.GetExtension(attach.Name);

        return download ? File(fs, attach.MimeType, $"{attach.Id}{extension}") : File(fs, attach.MimeType);
    }
}