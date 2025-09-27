using EVMarketPlace.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace EVMarketPlace.Repositories.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Post> Posts => Set<Post>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map bảng posts
            modelBuilder.Entity<Post>(e =>
            {
                e.ToTable("posts");
                e.HasKey(x => x.PostId);
                e.Property(x => x.PostId).HasColumnName("post_id");
                e.Property(x => x.UserId).HasColumnName("user_id");
                e.Property(x => x.Type).HasColumnName("type").HasMaxLength(10);
                e.Property(x => x.Title).HasColumnName("title").HasMaxLength(200);
                e.Property(x => x.Description).HasColumnName("description");
                e.Property(x => x.Price).HasColumnName("price").HasColumnType("decimal(18,2)");
                e.Property(x => x.IsActive).HasColumnName("is_active");
                e.Property(x => x.CreatedAt).HasColumnName("created_at");
            });
        }
    }
}
