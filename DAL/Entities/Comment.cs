namespace DAL.Entities
{
	public class Comment
	{
		public Guid Id { get; set; }
		public Guid AuthorId { get; set; }
		public Guid PostId { get; set; }
		public DateTimeOffset CreatingDate { get; set; }
		public string Text { get; set; } = null!;
	}
}
