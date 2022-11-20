using Api.Models.Attach;
using Api.Models.Comment;
using Api.Models.Post;
using Api.Models.User;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
	public class PostService
	{
		private readonly DataContext _context;
		private Func<AttachModel, string?>? _linkContentGenerator;
		private Func<UserModel, string?>? _linkAvatarGenerator;
		private readonly IMapper _mapper;
		public PostService(DataContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}
		public void SetLinkGenerator(Func<AttachModel, string?> linkContentGenerator, Func<UserModel, string?> linkAvatarGenerator)
		{
			_linkContentGenerator = linkContentGenerator;
			_linkAvatarGenerator = linkAvatarGenerator;
		}

		public async Task CreatePost(CreatePostModel model)
		{
			var dbModel = _mapper.Map<Post>(model);

			await _context.Posts.AddAsync(dbModel);
			await _context.SaveChangesAsync();
		}
		public async Task<PostModel> GetPost(Guid postId)
		{
			var post = await _context.Posts
				.AsNoTracking()
				.Include(x => x.PostContents)
				.Include(x => x.Author).ThenInclude(x=>x.Avatar)
				.FirstOrDefaultAsync(x => x.Id == postId);
			if (post == null)
			{
				throw new Exception("post not found");
			}
			var result = new PostModel
			{
				Author = new UserAvatarModel(_mapper.Map<UserModel>(post.Author), post.Author.Avatar == null ? null : _linkAvatarGenerator),
				Id = post.Id,
				Description = post.Description,
				Contents = post.PostContents.Select(x =>
					new AttachWithLinkModel(_mapper.Map<AttachModel>(x), _linkContentGenerator)).ToList()
			};
			return result;
		}
		public async Task<List<PostModel>> GetPosts(int skip, int take)
		{
			var posts = await _context.Posts
				.AsNoTracking()
				.Include(x => x.PostContents)
				.Include(x => x.Author).ThenInclude(x=> x.Avatar)
				.Take(take)
				.Skip(skip)
				.ToListAsync();
			var result = posts.Select(post =>			
				new PostModel
				{
					Author = new UserAvatarModel(_mapper.Map<UserModel>(post.Author), post.Author.Avatar==null?null:_linkAvatarGenerator),
					Description = post.Description,
					Id = post.Id,
					Contents = post.PostContents.Select(x => 
						new AttachWithLinkModel(_mapper.Map<AttachModel>(x), _linkContentGenerator)).ToList()
				}).ToList();
			
			return result;
		}				
		public async Task<AttachModel> GetPostContent(Guid postContentId)
		{
			var result = await _context.PostContents.FirstOrDefaultAsync(x => x.Id == postContentId);
			return _mapper.Map<AttachModel>(result);
		}
		public async Task AddComment(CreateCommentModel model)
		{
			var comment = _mapper.Map<Comment>(model);
			await _context.Comments.AddAsync(comment);
			await _context.SaveChangesAsync();
		}
		public async Task<List<CommentModel>> GetCommentsFromPost(Guid postId)
		{
			var dbComments = await _context.Comments.Where(x => x.PostId == postId).ToListAsync();
			var commentsList = _mapper.Map<List<Comment>, List<CommentModel>> (dbComments);
			return commentsList;
		}
	}
}
