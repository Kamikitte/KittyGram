using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
	public class DataContext : DbContext
	{
		public DataContext(DbContextOptions<DataContext> options) : base(options)
		{

		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>().HasIndex(f => f.Email).IsUnique();

			modelBuilder.Entity<Avatar>().ToTable(nameof(Avatars));
			modelBuilder.Entity<PostContent>().ToTable(nameof(PostContents));

			//Использовано TPH, так как при TPT не удалось создать двойной уникальный индекс,
			//состоящий из юзера в таблице Likes и поста/коммента с таблиц LikesPost и LikesComment
			//modelBuilder.Entity<LikePost>().ToTable(nameof(LikesPost));
			modelBuilder.Entity<LikePost>().HasIndex(c => new { c.LikerId, c.PostId }).IsUnique();
			//modelBuilder.Entity<LikeComment>().ToTable(nameof(LikesComment));
			modelBuilder.Entity<LikeComment>().HasIndex(c => new { c.LikerId, c.CommentId }).IsUnique();

			//TODO: сообразить связь m:m 
			modelBuilder.Entity<User>()
				.HasMany(c => c.Subscribers)
				.WithMany(s => s.Publishers)
				.UsingEntity(j => j.ToTable("Subscriptions"));
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
			=> optionsBuilder.UseNpgsql(b => b.MigrationsAssembly("Api"));

		public DbSet<User> Users => Set<User>();
		public DbSet<UserSession> UserSessions => Set<UserSession>();
		public DbSet<Attach> Attaches => Set<Attach>();
		public DbSet<Avatar> Avatars => Set<Avatar>();
		public DbSet<Post> Posts => Set<Post>();
		public DbSet<PostContent> PostContents => Set<PostContent>();
		public DbSet<Comment> Comments => Set<Comment>();
		public DbSet<Like> Likes => Set<Like>();
		public DbSet<LikePost> LikesPost => Set<LikePost>();
		public DbSet<LikeComment> LikesComment => Set<LikeComment>();
	}
}
