namespace Api.Models.Attach;

public sealed class AttachModel
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    
    public string MimeType { get; set; } = null!;
    
    public string FilePath { get; set; } = null!;
}

public sealed class AttachExternalModel
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    
    public string MimeType { get; set; } = null!;
    
    public string? ContentLink { get; set; }
}