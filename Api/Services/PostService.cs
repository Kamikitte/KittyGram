﻿using Api.Exceptions;
using Api.Models.Attach;
using Api.Models.Comment;
using Api.Models.Like;
using Api.Models.Post;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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
		public async Task<PostModel> GetPostById(Guid postId)
		{
			var post = await _context.Posts
				.Include(x => x.Author).ThenInclude(x => x.Avatar)
				.Include(x => x.PostContents)
				.Include(x=>x.Comments)
				.Include(x => x.Likes).AsNoTracking()
				.Where(x => x.Id == postId)
				.Select(x => _mapper.Map<PostModel>(x))
				.FirstOrDefaultAsync();
			if (post == null)
				throw new PostNotFoundException();
			var test = await _context.Posts.Include(x => x.Likes).FirstOrDefaultAsync(x => x.Id == postId);
			return post;
		}

		public async Task<List<PostModel>> GetPosts(int skip, int take) =>
			await _context.Posts
				.AsNoTracking()
				.Include(x => x.PostContents)
				.Include(x => x.Author).ThenInclude(x => x.Avatar)
				.OrderByDescending(x => x.CreatingDate).Skip(skip).Take(take)
				.Select(x => _mapper.Map<PostModel>(x))
				.ToListAsync();

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
			var commentsList = _mapper.Map<List<Comment>, List<CommentModel>>(dbComments);
			return commentsList;
		}

		//TODO перенести лайки в отдельный контроллер
		public async Task AddLikeToPost(LikePostRequestModel model)
		{
			var like = _mapper.Map<LikePost>(model);
			await _context.LikesPost.AddAsync(like);
			await _context.SaveChangesAsync();
		}

		public async Task RemoveLikeFromPost(LikePostRequestModel model)
		{
			var like = await _context.LikesPost.FirstOrDefaultAsync(x => x.LikerId == model.LikerId && x.PostId == model.PostId);
			if (like == null)
				throw new Exception("Like is not here");
			_context.LikesPost.Remove(like);
			await _context.SaveChangesAsync();
		}

		public async Task AddLikeToComment(LikeCommentRequestModel model)
		{
			var like = _mapper.Map<LikeComment>(model);
			await _context.LikesComment.AddAsync(like);
			await _context.SaveChangesAsync();
		}

		public async Task RemoveLikeFromComment(LikeCommentRequestModel model)
		{
			var like = await _context.LikesComment.FirstOrDefaultAsync(x => x.LikerId == model.LikerId && x.CommentId == model.CommentId);
			if (like == null)
				throw new Exception("Like is not here");
			_context.LikesComment.Remove(like);
			await _context.SaveChangesAsync();
		}

		public async Task<List<PostModel>> GetFeed(int skip, int take, Guid userId)
		{
			var publishers = (await _context.Users
				.Include(x => x.Publishers)
				.FirstOrDefaultAsync(x => x.Id == userId))?
				.Publishers;
			var publishersId = new List<Guid>();
			foreach(var publisher in publishers)
				publishersId.Add(publisher.Id);

			return await _context.Posts
				.AsNoTracking()
				.Include(x => x.PostContents)
				.Include(x => x.Author).ThenInclude(x => x.Avatar)
				.Where(x => publishersId.Contains(x.AuthorId))
				.OrderByDescending(x => x.CreatingDate).Skip(skip).Take(take)
				.Select(x => _mapper.Map<PostModel>(x))
				.ToListAsync();
		}
	}
}
