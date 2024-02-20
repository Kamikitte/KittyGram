namespace Api.Models.Comment;

public sealed class CommentModel
{
    public Guid AuthorId { get; set; }
    
    public DateTimeOffset CreatingDate { get; set; }
    
    public string Text { get; set; } = null!;
    
    public int LikesCount { get; set; }
}