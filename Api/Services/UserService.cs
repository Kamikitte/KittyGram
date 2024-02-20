using Api.Exceptions;
using Api.Models.Attach;
using Api.Models.Subscription;
using Api.Models.User;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public sealed class UserService
{
    private readonly DataContext context;
    private readonly IMapper mapper;

    public UserService(IMapper mapper, DataContext context)
    {
        this.mapper = mapper;
        this.context = context;
    }

    public async Task<Guid> CreateUser(CreateUserModel model)
    {
        var dbUser = mapper.Map<User>(model);
        var t = await context.Users.AddAsync(dbUser);
        await context.SaveChangesAsync();
        return t.Entity.Id;
    }

    public Task<bool> CheckUserExist(string email)
    {
        return context.Users.AnyAsync(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task AddAvatarToUser(Guid userId, MetadataModel meta, string filePath)
    {
        var user = await context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == userId);
        if (user != null)
        {
            var avatar = new Avatar
            {
                Author = user,
                MimeType = meta.MimeType,
                FilePath = filePath,
                Name = meta.Name,
                Size = meta.Size,
            };
            user.Avatar = avatar;
            await context.SaveChangesAsync();
        }
    }

    public async Task<AttachModel> GetUserAvatar(Guid userId)
    {
        var user = await GetUserById(userId);
        var attach = mapper.Map<AttachModel>(user.Avatar);
        return attach;
    }

    public async Task DeleteUser(Guid id)
    {
        var dbUser = await GetUserById(id);
        context.Users.Remove(dbUser);
        await context.SaveChangesAsync();
    }

    private async Task<User> GetUserById(Guid id)
    {
        var user = await context.Users.Include(x => x.Avatar).Include(x => x.Posts)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user is null)
        {
            throw new UserNotFoundException();
        }

        return user;
    }

    public async Task<UserAvatarModel> GetUser(Guid id) =>
        mapper.Map<User, UserAvatarModel>(await GetUserById(id));

    public async Task<IEnumerable<UserAvatarModel>> GetUsers() =>
        await context.Users.AsNoTracking()
            .Include(x => x.Avatar)
            .Include(x => x.Posts)
            .Select(x => mapper.Map<UserAvatarModel>(x))
            .ToListAsync();

    //TODO перенести подписки в отдельный контроллер
    public async Task SubscribeToUser(SubscriptionModel model)
    {
        var subscriber = await GetUserById(model.SubscriberId);
        subscriber.Publishers.Add(await GetUserById(model.PublisherId));
        context.Users.Update(subscriber);
        await context.SaveChangesAsync();
    }

    public async Task UnsubscribeFromUser(SubscriptionModel model)
    {
        var user = await context.Users.Include(x => x.Publishers).FirstOrDefaultAsync(x => x.Id == model.SubscriberId);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var publisher = user.Publishers.FirstOrDefault(x => x.Id == model.PublisherId);
        if (publisher == null)
        {
            throw new UserNotFoundException();
        }

        user.Publishers.Remove(publisher);

        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserAvatarModel>> GetSubscribers(Guid userId)
    {
        var user = await context.Users.Include(x => x.Subscribers).FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        return user.Subscribers.Select(x => mapper.Map<UserAvatarModel>(x));
    }

    public async Task<IEnumerable<UserAvatarModel>> GetPublishers(Guid userId)
    {
        var user = await context.Users.Include(x => x.Publishers).FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        return user.Publishers.Select(x => mapper.Map<UserAvatarModel>(x));
    }
}