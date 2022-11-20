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
	}
}
