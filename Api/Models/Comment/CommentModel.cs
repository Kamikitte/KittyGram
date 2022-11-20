namespace Api.Models.Comment
{
    public class CommentModel
    {
        public Guid AuthorId { get; set; }
        public DateTimeOffset CreatingDate { get; set; }
        public string Text { get; set; } = null!;
    }
}
