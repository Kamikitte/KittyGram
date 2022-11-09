namespace Api.Models
{
	public class PostModel
	{
		public Guid Id { get; set; }
		public virtual List<AttachWithLinkModel> Attaches { get; set; } = null!;
		public string? Description { get; set; }
		public virtual UserModel Author { get; set; } = null!;
		public DateTimeOffset CreatingDate { get; set; }
	}
}
