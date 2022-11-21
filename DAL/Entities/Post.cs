﻿namespace DAL.Entities
{
	public class Post
	{
		public Guid Id { get; set; }
		public virtual ICollection<PostContent> PostContents { get; set; } = null!;
		public Guid AuthorId { get; set; }
		public string? Description { get; set; }
		public DateTimeOffset CreatingDate { get; set; }
		public virtual ICollection<Comment>? Comments { get; set; }
		public virtual User Author { get; set; } = null!;
	}
}
