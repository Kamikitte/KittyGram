using Api.Models.Attach;

namespace Api.Models.Post
{
    public class CreatePostModel
    {
        public Guid Id { get; set; }
        public List<MetaWithPath> Contents { get; set; } = new List<MetaWithPath>();
        public string? Description { get; set; }
        public Guid AuthorId { get; set; }
    }

    public class CreatePostRequest
    {
        public List<MetadataModel> Contents { get; set; } = new List<MetadataModel>();
        public string? Description { get; set; }
    }
}
