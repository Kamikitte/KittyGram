namespace DAL.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTimeOffset BirthDate { get; set; }

        public virtual Avatar? Avatar { get; set; }
        public virtual ICollection<Post>? Posts { get; set; }
        public virtual ICollection<UserSession>? Sessions { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<Like>? Likes { get; set; }
		public virtual ICollection<User> Subscribers { get; set; } = new List<User>();
        public virtual ICollection<User> Publishers { get; set; } = new List<User>();
	}
}
