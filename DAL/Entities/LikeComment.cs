namespace DAL.Entities;

public class LikeComment : Like
{
	public Guid CommentId { get; set; }
	
	public virtual Comment Comment { get; set; } = null!;
}