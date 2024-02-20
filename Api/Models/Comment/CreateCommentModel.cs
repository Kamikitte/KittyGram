namespace Api.Models.Comment;

public sealed class CreateCommentModel
{
    public Guid AuthorId { get; set; }
    
    public Guid PostId { get; set; }
    
    public string Text { get; set; } = null!;
}

public sealed class CreateCommentRequest
{
    public Guid PostId { get; set; }
    
    public string Text { get; set; } = null!;
}