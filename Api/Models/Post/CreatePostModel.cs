using Api.Models.Attach;

namespace Api.Models.Post;

public sealed class CreatePostModel
{
    public Guid Id { get; set; }
    
    public List<MetadataLinkModel> Contents { get; set; } = new();
    
    public string? Description { get; set; }
    
    public Guid AuthorId { get; set; }
}

public sealed class CreatePostRequest
{
    public Guid? AuthorId { get; set; }
    
    public List<MetadataModel> Contents { get; set; } = new();
    
    public string? Description { get; set; }
}