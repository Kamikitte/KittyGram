using Api.Exceptions;
using Api.Models.Attach;
using Api.Models.Comment;
using Api.Models.Like;
using Api.Models.Post;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public sealed class PostService
{
    private readonly DataContext context;
    private readonly IMapper mapper;

    public PostService(DataContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    public async Task CreatePost(CreatePostRequest request)
    {
        var model = mapper.Map<CreatePostModel>(request);

        model.Contents.ForEach(x =>
        {
            x.AuthorId = model.AuthorId;
            x.FilePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Attaches",
                x.TempId.ToString());
            var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), x.TempId.ToString()));
            
            if (!tempFi.Exists)
            {
                return;
            }

            var destFi = new FileInfo(x.FilePath);
            if (destFi.Directory is { Exists: false })
            {
                destFi.Directory.Create();
            }

            File.Move(tempFi.FullName, x.FilePath, true);
        });

        var dbModel = mapper.Map<Post>(model);
        await context.Posts.AddAsync(dbModel);
        await context.SaveChangesAsync();
    }

    public async Task<PostModel> GetPostById(Guid postId)
    {
        var post = await context.Posts
            .Include(x => x.Author).ThenInclude(x => x.Avatar)
            .Include(x => x.PostContents)
            .Include(x => x.Comments)
            .Include(x => x.Likes).AsNoTracking()
            .Where(x => x.Id == postId)
            .Select(x => mapper.Map<PostModel>(x))
            .FirstOrDefaultAsync();
        
        if (post == null)
        {
            throw new PostNotFoundException();
        }

        return post;
    }

    public Task<List<PostModel>> GetPosts(int skip, int take) =>
        context.Posts
            .AsNoTracking()
            .Include(x => x.PostContents)
            .Include(x => x.Author).ThenInclude(x => x.Avatar)
            .OrderByDescending(x => x.CreatingDate).Skip(skip).Take(take)
            .Select(x => mapper.Map<PostModel>(x))
            .ToListAsync();

    public async Task<AttachModel> GetPostContent(Guid postContentId)
    {
        var result = await context.PostContents.FirstOrDefaultAsync(x => x.Id == postContentId);
        return mapper.Map<AttachModel>(result);
    }

    public async Task AddComment(CreateCommentModel model)
    {
        var comment = mapper.Map<Comment>(model);
        await context.Comments.AddAsync(comment);
        await context.SaveChangesAsync();
    }

    public async Task<List<CommentModel>> GetCommentsFromPost(Guid postId)
    {
        var dbComments = await context.Comments.Where(x => x.PostId == postId).ToListAsync();
        var commentsList = mapper.Map<List<Comment>, List<CommentModel>>(dbComments);
        return commentsList;
    }

    //TODO перенести лайки в отдельный контроллер
    public async Task AddLikeToPost(LikePostRequestModel model)
    {
        var like = mapper.Map<LikePost>(model);
        await context.LikesPost.AddAsync(like);
        await context.SaveChangesAsync();
    }

    public async Task RemoveLikeFromPost(LikePostRequestModel model)
    {
        var like = await context.LikesPost.FirstOrDefaultAsync(x =>
            x.LikerId == model.LikerId && x.PostId == model.PostId);
        if (like == null)
        {
            throw new Exception("Like is not here");
        }

        context.LikesPost.Remove(like);
        await context.SaveChangesAsync();
    }

    public async Task AddLikeToComment(LikeCommentRequestModel model)
    {
        var like = mapper.Map<LikeComment>(model);
        await context.LikesComment.AddAsync(like);
        await context.SaveChangesAsync();
    }

    public async Task RemoveLikeFromComment(LikeCommentRequestModel model)
    {
        var like = await context.LikesComment.FirstOrDefaultAsync(x =>
            x.LikerId == model.LikerId && x.CommentId == model.CommentId);
        
        if (like == null)
        {
            throw new Exception("Like is not here");
        }

        context.LikesComment.Remove(like);
        await context.SaveChangesAsync();
    }

    public async Task<List<PostModel>> GetFeed(int skip, int take, Guid userId)
    {
        var publishers = (await context.Users
                .Include(x => x.Publishers)
                .FirstOrDefaultAsync(x => x.Id == userId))?
            .Publishers;
        var publishersId = new List<Guid>();

        if (publishers != null)
        {
            publishersId.AddRange(publishers.Select(publisher => publisher.Id));
        }

        return await context.Posts
            .AsNoTracking()
            .Include(x => x.PostContents)
            .Include(x => x.Author).ThenInclude(x => x.Avatar)
            .Where(x => publishersId.Contains(x.AuthorId))
            .OrderByDescending(x => x.CreatingDate).Skip(skip).Take(take)
            .Select(x => mapper.Map<PostModel>(x))
            .ToListAsync();
    }
}