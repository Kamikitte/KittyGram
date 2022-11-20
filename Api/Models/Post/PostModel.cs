using Api.Models.Attach;
using Api.Models.User;

namespace Api.Models.Post
{
    public class PostModel
    {
        public Guid Id { get; set; }
        public List<AttachWithLinkModel> Contents { get; set; } = null!;
        public string? Description { get; set; }
        public UserAvatarModel Author { get; set; } = null!;
        public DateTimeOffset CreatingDate { get; set; }
    }
}
