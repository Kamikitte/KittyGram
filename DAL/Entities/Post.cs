namespace DAL.Entities
{
	public class Post
	{
		public Guid Id { get; set; }
		public virtual ICollection<PostAttach> Attaches { get; set; } = null!;
		public virtual User Author { get; set; } = null!;
	}
}
