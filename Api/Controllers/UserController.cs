using Api.Models.Attach;
using Api.Models.Subscription;
using Api.Models.User;
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
public class UserController : ControllerBase
{
    private readonly AttachService attachService;
    private readonly UserService userService;

    public UserController(UserService userService, AttachService attachService, LinkGeneratorService links)
    {
        this.userService = userService;
        this.attachService = attachService;
        links.LinkContentGenerator = x => Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar),
            new
            {
                userId = x.Id,
            });
    }

    [HttpPost]
    public Task AddAvatarToUser(MetadataModel model)
    {
        var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
        if (userId == Guid.Empty)
        {
            throw new Exception("you are not authorized");
        }

        var path = attachService.SaveMetaToAttaches(model);

        return userService.AddAvatarToUser(userId, model, path);
    }

    [HttpGet]
    public Task<IEnumerable<UserAvatarModel>> GetUsers() => userService.GetUsers();

    [HttpGet]
    public Task<UserAvatarModel> GetCurrentUser()
    {
        var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
        if (userId == Guid.Empty)
        {
            throw new Exception("you are not authorized");
        }

        return userService.GetUser(userId);
    }

    [HttpGet]
    public Task<UserAvatarModel> GetUserById(Guid userId) =>
        userService.GetUser(userId);

    [HttpPost]
    public Task SubscribeToUser(Guid publisherId)
    {
        var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
        if (userId == Guid.Empty)
        {
            throw new Exception("you are not authorized");
        }

        var model = new SubscriptionModel
        {
            PublisherId = publisherId,
            SubscriberId = userId,
        };
        
        return userService.SubscribeToUser(model);
    }

    [HttpPost]
    public Task UnsubscribeFromUser(Guid publisherId)
    {
        var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
        if (userId == Guid.Empty)
        {
            throw new Exception("you are not authorized");
        }

        var model = new SubscriptionModel
        {
            PublisherId = publisherId,
            SubscriberId = userId,
        };
        
        return userService.UnsubscribeFromUser(model);
    }

    [HttpGet]
    public Task<IEnumerable<UserAvatarModel>> GetSubscribers(Guid userId) =>
        userService.GetSubscribers(userId);

    [HttpGet]
    public Task<IEnumerable<UserAvatarModel>> GetPublishers(Guid userId) =>
        userService.GetPublishers(userId);
}