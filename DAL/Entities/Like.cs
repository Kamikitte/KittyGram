namespace DAL.Entities
{
	public class Like
	{
		public Guid Id { get; set; }
		public Guid LikerId { get; set; }
		public DateTimeOffset CreateTime { get; set; }
		public virtual User Liker { get; set; } = null!;
	}
}
