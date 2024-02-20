namespace DAL.Entities;

public sealed class LikePost : Like
{
	public Guid PostId { get; set; }
}