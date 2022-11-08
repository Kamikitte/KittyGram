namespace DAL.Entities
{
	public class Avatar : Attach
	{
		public Guid OwnerId { get; set; }
		public virtual User Owner { get; set; } = null!;
	}
}
