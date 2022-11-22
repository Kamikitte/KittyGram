namespace DAL.Entities
{
	public class LikePost : Like
	{
		public Guid PostId { get; set; }
	}
}
