using Api.Exceptions;
using Api.Models.Attach;
using Api.Models.Subscription;
using Api.Models.User;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class UserService
	{
		private readonly IMapper _mapper;
		private readonly DataContext _context;
		public UserService(IMapper mapper, DataContext context)
		{
			_mapper = mapper;
			_context = context;
		}

		public async Task<Guid> CreateUser(CreateUserModel model)
		{
			var dbUser = _mapper.Map<User>(model);
			var t = await _context.Users.AddAsync(dbUser);
			await _context.SaveChangesAsync();
			return t.Entity.Id;
		}

		public async Task<bool> CheckUserExist(string email)
		{
			return await _context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
		}

		public async Task AddAvatarToUser(Guid userId, MetadataModel meta, string filePath)
		{
			var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == userId);
			if (user != null)
			{
				var avatar = new Avatar
				{
					Author = user,
					MimeType = meta.MimeType,
					FilePath = filePath,
					Name = meta.Name,
					Size = meta.Size
				};
				user.Avatar = avatar;
				await _context.SaveChangesAsync();
			}
		}

		public async Task<AttachModel> GetUserAvatar(Guid userId)
		{
			var user = await GetUserById(userId);
			var attach = _mapper.Map<AttachModel>(user.Avatar);
			return attach;
		}

		public async Task DeleteUser(Guid id)
		{
			var dbUser = await GetUserById(id);
			if (dbUser != null)
			{
				_context.Users.Remove(dbUser);
				await _context.SaveChangesAsync();
			}
		}

		private async Task<User> GetUserById(Guid id)
		{
			var user = await _context.Users.Include(x => x.Avatar).Include(x => x.Posts).FirstOrDefaultAsync(x => x.Id == id);
			if (user == null || user == default)
				throw new UserNotFoundException();

			return user;
		}

		public async Task<UserAvatarModel> GetUser(Guid id) =>
			 _mapper.Map<User, UserAvatarModel>(await GetUserById(id));

		public async Task<IEnumerable<UserAvatarModel>> GetUsers() =>
			await _context.Users.AsNoTracking()
			.Include(x => x.Avatar)
			.Include(x => x.Posts)
			.Select(x => _mapper.Map<UserAvatarModel>(x))
			.ToListAsync();

		//TODO перенести подписки в отдельный контроллер
		public async Task SubscribeToUser(SubscriptionModel model)
		{
			var subscriber = await GetUserById(model.SubscriberId);
			subscriber.Publishers.Add(await GetUserById(model.PublisherId));
			_context.Users.Update(subscriber);
			await _context.SaveChangesAsync();
		}

		public async Task UnsubscribeFromUser(SubscriptionModel model)
		{
			var user = await _context.Users.Include(x => x.Publishers).FirstOrDefaultAsync(x => x.Id == model.SubscriberId);
			if (user == null || user == default)
				throw new UserNotFoundException();
			var publisher = user.Publishers.FirstOrDefault(x=>x.Id==model.PublisherId);
			if (publisher == null || publisher == default)
				throw new UserNotFoundException();
			user.Publishers.Remove(publisher);

			_context.Users.Update(user);
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<UserAvatarModel>> GetSubscribers(Guid userId)
		{
			var user = await _context.Users.Include(x=>x.Subscribers).FirstOrDefaultAsync(x=>x.Id==userId);
			if (user == null || user == default)
				throw new UserNotFoundException();
			return user.Subscribers.Select(x => _mapper.Map<UserAvatarModel>(x));
		}

		public async Task<IEnumerable<UserAvatarModel>> GetPublishers(Guid userId)
		{
			var user = await _context.Users.Include(x => x.Publishers).FirstOrDefaultAsync(x => x.Id == userId);
			if (user == null || user == default)
				throw new UserNotFoundException();
			return user.Publishers.Select(x => _mapper.Map<UserAvatarModel>(x));
		}
	}
}
