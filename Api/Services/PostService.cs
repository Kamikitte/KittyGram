using Api.Models.Attach;
using Api.Models.Comment;
using Api.Models.Post;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
	public class PostService
	{
		private readonly DataContext _context;
		private readonly IMapper _mapper;
		public PostService(DataContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task CreatePost(CreatePostRequest request)
		{
			var model = _mapper.Map<CreatePostModel>(request);

			model.Contents.ForEach(x =>
			{
				x.AuthorId = model.AuthorId;
				x.FilePath = Path.Combine(
					Directory.GetCurrentDirectory(),
					"Attaches",
					x.TempId.ToString());
				var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), x.TempId.ToString()));
				if (tempFi.Exists)
				{
					var destFi = new FileInfo(x.FilePath);
					if (destFi.Directory != null && !destFi.Directory.Exists)
						destFi.Directory.Create();
					File.Move(tempFi.FullName, x.FilePath, true);
				}
			});

			var dbModel = _mapper.Map<Post>(model);
			await _context.Posts.AddAsync(dbModel);
			await _context.SaveChangesAsync();
		}
		//public async Task<PostModel> GetPost(Guid postId)
		//{
		//	var post = await _context.Posts
		//		.AsNoTracking()
		//		.Include(x => x.PostContents)
		//		.Include(x => x.Author).ThenInclude(x=>x.Avatar)
		//		.FirstOrDefaultAsync(x => x.Id == postId);
		//	if (post == null)
		//	{
		//		throw new Exception("post not found");
		//	}
		//	var result = new PostModel
		//	{
		//		Author = new UserAvatarModel(_mapper.Map<UserModel>(post.Author), post.Author.Avatar == null ? null : _linkAvatarGenerator),
		//		Id = post.Id,
		//		Description = post.Description,
		//		Contents = post.PostContents.Select(x =>
		//			new AttachExternalModel(_mapper.Map<AttachModel>(x), _linkContentGenerator)).ToList()
		//	};
		//	return result;
		//}
		public async Task<List<PostModel>> GetPosts(int skip, int take)
		{
			var posts = await _context.Posts
				.AsNoTracking()
				.Include(x => x.PostContents)
				.Include(x => x.Author).ThenInclude(x=> x.Avatar)
				.OrderByDescending(x=>x.CreatingDate).Skip(skip).Take(take)
				.Select(x => _mapper.Map<PostModel>(x))
				.ToListAsync();			
			return posts;
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
