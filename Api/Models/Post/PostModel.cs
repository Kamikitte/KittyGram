﻿using Api.Models.Attach;
using Api.Models.User;

namespace Api.Models.Post;

public sealed class PostModel
{
    public Guid Id { get; set; }
    
    public List<AttachExternalModel> Contents { get; set; } = null!;
    
    public string? Description { get; set; }
    
    public UserAvatarModel Author { get; set; } = null!;
    
    public DateTimeOffset CreatingDate { get; set; }
    
    public int CommentsCount { get; set; }
    
    public int LikesCount { get; set; }
}