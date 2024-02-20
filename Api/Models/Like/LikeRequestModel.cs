namespace Api.Models.Like;

public class LikeRequestModel
{
    public Guid LikerId { get; set; }
}

public sealed class LikePostRequestModel : LikeRequestModel
{
    public Guid PostId { get; set; }
}

public sealed class LikeCommentRequestModel : LikeRequestModel
{
    public Guid CommentId { get; set; }
}