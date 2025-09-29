using EVMarketPlace.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace EVMarketPlace.Repositories.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Bảng users
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasKey(x => x.UserId);
                
                e.Property(x => x.UserId)
                    .HasColumnName("user_id");
                    
                e.Property(x => x.FullName)
                    .HasColumnName("full_name")
                    .HasMaxLength(100)
                    .IsRequired();
                    
                e.Property(x => x.Email)
                    .HasColumnName("email")
                    .HasMaxLength(100)
                    .IsRequired();
                    
                e.Property(x => x.Phone)
                    .HasColumnName("phone")
                    .HasMaxLength(20);
                    
                e.Property(x => x.PasswordHash)
                    .HasColumnName("password_hash")
                    .HasMaxLength(255)
                    .IsRequired();
                    
                e.Property(x => x.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("GETUTCDATE()");
                    
                e.Property(x => x.IsActive)
                    .HasColumnName("is_active")
                    .HasDefaultValue(true);
                
                // Add unique index for email
                e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("IX_users_email_unique");
                
                // Add index for phone
                e.HasIndex(x => x.Phone).HasDatabaseName("IX_users_phone");
            });

            // Bảng posts
            modelBuilder.Entity<Post>(e =>
            {
                e.ToTable("posts");
                e.HasKey(p => p.PostId);

                e.Property(p => p.PostId)
                    .HasColumnName("post_id");

                e.Property(p => p.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                e.Property(p => p.Type)
                    .HasColumnName("type")
                    .HasMaxLength(10)
                    .IsRequired();

                e.Property(p => p.Title)
                    .HasColumnName("title")
                    .HasMaxLength(200)
                    .IsRequired();

                e.Property(p => p.Description)
                    .HasColumnName("description")
                    .HasMaxLength(2000)
                    .IsRequired();

                e.Property(p => p.Price)
                    .HasColumnName("price")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                e.Property(p => p.IsActive)
                    .HasColumnName("is_active")
                    .IsRequired();

                e.Property(p => p.CreatedAt)
                    .HasColumnName("created_at")    
                    .HasDefaultValueSql("GETDATE()");

                // Add indexes
                e.HasIndex(p => p.UserId).HasDatabaseName("IX_posts_user_id");
                e.HasIndex(p => p.Type).HasDatabaseName("IX_posts_type");
                e.HasIndex(p => p.IsActive).HasDatabaseName("IX_posts_is_active");
                e.HasIndex(p => p.CreatedAt).HasDatabaseName("IX_posts_created_at");

                // Foreign key relationship
                e.HasOne(p => p.User)
                 .WithMany(u => u.Posts)
                 .HasForeignKey(p => p.UserId)
                 .OnDelete(DeleteBehavior.NoAction)
                 .HasConstraintName("FK_posts_users");
            });

            // Tạm thời bỏ qua mấy entity khác để không lỗi
            modelBuilder.Ignore<Auction>();
            modelBuilder.Ignore<AuctionBid>();
            modelBuilder.Ignore<Battery>();
            modelBuilder.Ignore<BatteryBrand>();
            modelBuilder.Ignore<ShoppingCart>();
            modelBuilder.Ignore<CartItem>();
            modelBuilder.Ignore<Contract>();
            modelBuilder.Ignore<Favorite>();
            modelBuilder.Ignore<Review>();
            modelBuilder.Ignore<Transaction>();
            modelBuilder.Ignore<Vehicle>();
            modelBuilder.Ignore<VehicleBrand>();
            modelBuilder.Ignore<Wallet>();
        }
    }
}
