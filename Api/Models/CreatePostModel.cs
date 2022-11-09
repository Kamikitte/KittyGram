namespace Api.Models
{
	public class CreatePostModel
	{
		public List<MetadataModel> Attaches { get; set; }
		public string? Description { get; set; }
		public Guid AuthorId { get; set; }
	}

	public class CreatePostRequest
	{
		public List<MetadataModel> Attaches { get; set; }
		public string? Description { get; set; }
	}
}
