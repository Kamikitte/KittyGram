using Api.Migrations;
using Api.Models;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Api.Services
{
	public class PostService
	{
		private readonly DataContext _context;
		private readonly UserService _userService;
		private readonly AttachService _attachService;
		private Func<AttachModel, string?>? _linkContentGenerator;
		private readonly IMapper _mapper;
		public PostService(DataContext context, UserService userService, IMapper mapper, AttachService attachService)
		{
			_context = context;
			_userService = userService;
			_attachService = attachService;
			_mapper = mapper;
		}
		public void SetLinkGenerator(Func<AttachModel, string?> linkContentGenerator)
		{
			_linkContentGenerator = linkContentGenerator;
		}

		public async Task<Guid> CreatePost(Guid userId, List<MetadataModel> metaAttaches)
		{
			var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
			if (user == null)
				throw new Exception("user not found");
			var post = new Post
			{
				Id = Guid.NewGuid(),
				Author = user,
				CreatingDate = DateTime.UtcNow,
				Attaches = new List<PostAttach>()			
			};
			await _context.Posts.AddAsync(post);
			foreach (var meta in metaAttaches)
			{
				var path = _attachService.SaveMetaToAttaches(meta);
				var postAttach = new PostAttach
				{
					Id = Guid.NewGuid(),
					Name = meta.Name,
					MimeType = meta.MimeType,
					FilePath = path,
					Size = meta.Size,
					Author = user,
					PostId = post.Id
				};
				await _context.PostAttaches.AddAsync(postAttach);
			}
			await _context.SaveChangesAsync();
			return post.Id;
		}

		public AttachModel GetContent(Guid contentId)
		{
			var dbAttach = _context.Attaches.FirstOrDefault(x => x.Id == contentId);
			return _mapper.Map<AttachModel>(dbAttach);
		}

		public async Task<PostModel> GetPost(Guid postId)
		{
			var post = await _context.Posts
				.Include(x=>x.Attaches)
				.Include(x=>x.Author)
				.FirstOrDefaultAsync(x => x.Id == postId);
			if (post == null)
			{
				throw new Exception("post not found");
			}
			var result = new PostModel
			{
				Author = _mapper.Map<UserModel>(post.Author),
				Id = post.Id,
				Attaches = post.Attaches.Select(x =>
					new AttachWithLinkModel(_mapper.Map<AttachModel>(x), _linkContentGenerator)).ToList()
			};
			return result;
		}
		public async Task AddComment(CreateCommentModel model)
		{
			var comment = _mapper.Map<Comment>(model);
			await _context.Comments.AddAsync(comment);
			await _context.SaveChangesAsync();
		}
		public List<CommentModel> GetCommentsFromPost(Guid postId)
		{
			var dbComments = _context.Comments.Where(x => x.PostId == postId).ToList();
			var commentsList = _mapper.Map<List<Comment>, List<CommentModel>> (dbComments);
			return commentsList;
		}
	}
}
