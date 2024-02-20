using Api.Models.Comment;
using Api.Models.Like;
using Api.Models.Post;
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
public class PostController : ControllerBase
{
    private readonly PostService postService;

    public PostController(PostService postService, LinkGeneratorService links)
    {
        this.postService = postService;
        links.LinkAvatarGenerator = x => Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar),
            new
            {
                userId = x.Id,
            });
        links.LinkContentGenerator = x => Url.ControllerAction<AttachController>(
            nameof(AttachController.GetPostContent), new
            {
                postContentId = x.Id,
            });
    }

    //TODO сделать проверку вводимого вручную юзера
    [HttpPost]
    public Task CreatePost(CreatePostRequest request)
    {
        if (request.AuthorId.HasValue)
        {
            return postService.CreatePost(request);
        }

        var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
        if (userId == Guid.Empty)
        {
            throw new Exception("not authorized");
        }

        request.AuthorId = userId;

        return postService.CreatePost(request);
    }

    [HttpGet]
    public Task<PostModel> GetPostById(Guid postId) => postService.GetPostById(postId);

    [HttpGet]
    public Task<List<PostModel>> GetPosts(int skip = 0, int take = 10) => postService.GetPosts(skip, take);

    [HttpGet]
    public Task<List<PostModel>> GetFeed(int skip = 0, int take = 10)
    {
        var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
        if (userId == Guid.Empty)
        {
            throw new Exception("not authorized");
        }

        return postService.GetFeed(skip, take, userId);
    }

    [HttpPost]
    public Task AddComment(CreateCommentRequest request)
    {
        var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            throw new Exception("you are not authorized");
        }

        var model = new CreateCommentModel
        {
            AuthorId = userId,
            PostId = request.PostId,
            Text = request.Text,
        };

        return postService.AddComment(model);
    }

    [HttpGet]
    public Task<List<CommentModel>> GetCommentsFromPost(Guid postId) =>
        postService.GetCommentsFromPost(postId);

    [HttpPost]
    public Task AddLikeToPost(Guid postId)
    {
        var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
        if (userId == Guid.Empty)
        {
            throw new Exception("not authorized");
        }

        var model = new LikePostRequestModel
        {
            LikerId = userId,
            PostId = postId,
        };

        return postService.AddLikeToPost(model);
    }

    [HttpPost]
    public Task RemoveLikeFromPost(Guid postId)
    {
        var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
        if (userId == Guid.Empty)
        {
            throw new Exception("not authorized");
        }

        var model = new LikePostRequestModel
        {
            LikerId = userId,
            PostId = postId,
        };

        return postService.RemoveLikeFromPost(model);
    }

    [HttpPost]
    public Task AddLikeToComment(Guid postId)
    {
        var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
        if (userId == Guid.Empty)
        {
            throw new Exception("not authorized");
        }

        var model = new LikeCommentRequestModel
        {
            LikerId = userId,
            CommentId = postId,
        };

        return postService.AddLikeToComment(model);
    }

    [HttpPost]
    public Task RemoveLikeFromComment(Guid postId)
    {
        var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
        if (userId == Guid.Empty)
        {
            throw new Exception("not authorized");
        }

        var model = new LikeCommentRequestModel
        {
            LikerId = userId,
            CommentId = postId,
        };

        return postService.RemoveLikeFromComment(model);
    }
}