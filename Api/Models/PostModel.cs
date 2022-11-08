using DAL.Entities;

namespace Api.Models
{
	public class PostModel
	{
		public Guid Id { get; set; }
		public virtual List<AttachWithLinkModel> Attaches { get; set; } = null!;
		public virtual UserModel Author { get; set; } = null!;
	}
}
