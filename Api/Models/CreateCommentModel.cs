namespace Api.Models
{
	public class CreateCommentModel
	{
		public Guid AuthorId { get; set; }
		public Guid PostId { get; set; }
		public string Text { get; set; } = null!;
	}

	public class CreateCommentRequest
	{
		public Guid PostId { get; set; }
		public string Text { get; set; } = null!;
	}
}
