using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class EvMarketplaceContext : DbContext
{
    public EvMarketplaceContext()
    {
    }

    public EvMarketplaceContext(DbContextOptions<EvMarketplaceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auction> Auctions { get; set; }

    public virtual DbSet<AuctionBid> AuctionBids { get; set; }

    public virtual DbSet<Battery> Batteries { get; set; }

    public virtual DbSet<BatteryBrand> BatteryBrands { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostImage> PostImages { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleBrand> VehicleBrands { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=DESKTOP-DNV1FA0\\TIEN;Database=EV_Marketplace;User Id=sa;Password=12345;TrustServerCertificate=True;");
    public static string GetConnectionString(string connectionStringName)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        string? connectionString = config.GetConnectionString(connectionStringName);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in appsettings.json");
        }

        return connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString("DefaultConnection")).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auction>(entity =>
        {
            entity.HasKey(e => e.AuctionId).HasName("PK__auctions__2FF786404735F3E5");

            entity.ToTable("auctions");

            entity.HasIndex(e => e.PostId, "UQ__auctions__3ED78767B09DBCA9").IsUnique();

            entity.Property(e => e.AuctionId)
                .ValueGeneratedNever()
                .HasColumnName("auction_id");
            entity.Property(e => e.CurrentPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("current_price");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.StartPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("start_price");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.HasOne(d => d.Post).WithOne(p => p.Auction)
                .HasForeignKey<Auction>(d => d.PostId)
                .HasConstraintName("FK__auctions__post_i__5441852A");
        });

        modelBuilder.Entity<AuctionBid>(entity =>
        {
            entity.HasKey(e => e.BidId).HasName("PK__auction___3DF0459662A19073");

            entity.ToTable("auction_bids");

            entity.Property(e => e.BidId)
                .ValueGeneratedNever()
                .HasColumnName("bid_id");
            entity.Property(e => e.AuctionId).HasColumnName("auction_id");
            entity.Property(e => e.BidAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("bid_amount");
            entity.Property(e => e.BidTime)
                .HasColumnType("datetime")
                .HasColumnName("bid_time");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Auction).WithMany(p => p.AuctionBids)
                .HasForeignKey(d => d.AuctionId)
                .HasConstraintName("FK__auction_b__aucti__571DF1D5");

            entity.HasOne(d => d.User).WithMany(p => p.AuctionBids)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__auction_b__user___5812160E");
        });

        modelBuilder.Entity<Battery>(entity =>
        {
            entity.HasKey(e => e.BatteryId).HasName("PK__batterie__31C8DB8E792E01E5");

            entity.ToTable("batteries");

            entity.HasIndex(e => e.PostId, "UQ__batterie__3ED7876778CC4A5D").IsUnique();

            entity.Property(e => e.BatteryId)
                .ValueGeneratedNever()
                .HasColumnName("battery_id");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.Condition)
                .HasMaxLength(50)
                .HasColumnName("condition");
            entity.Property(e => e.PostId).HasColumnName("post_id");

            entity.HasOne(d => d.Brand).WithMany(p => p.Batteries)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__batteries__brand__4CA06362");

            entity.HasOne(d => d.Post).WithOne(p => p.Battery)
                .HasForeignKey<Battery>(d => d.PostId)
                .HasConstraintName("FK__batteries__post___4BAC3F29");
        });

        modelBuilder.Entity<BatteryBrand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__battery___5E5A8E27F8ED4FA8");

            entity.ToTable("battery_brands");

            entity.Property(e => e.BrandId)
                .ValueGeneratedNever()
                .HasColumnName("brand_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__cart_ite__5D9A6C6E73445C24");

            entity.ToTable("cart_items");

            entity.Property(e => e.CartItemId)
                .ValueGeneratedNever()
                .HasColumnName("cart_item_id");
            entity.Property(e => e.AddedAt)
                .HasColumnType("datetime")
                .HasColumnName("added_at");
            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.PostId).HasColumnName("post_id");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__cart_item__cart___693CA210");

            entity.HasOne(d => d.Post).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__cart_item__post___6A30C649");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("PK__favorite__46ACF4CB27F872F9");

            entity.ToTable("favorites");

            entity.Property(e => e.FavoriteId)
                .ValueGeneratedNever()
                .HasColumnName("favorite_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Post).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__favorites__post___5070F446");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__favorites__user___4F7CD00D");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__posts__3ED787668F1A14E2");

            entity.ToTable("posts");

            entity.Property(e => e.PostId)
                .ValueGeneratedNever()
                .HasColumnName("post_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__posts__user_id__4316F928");
        });

        modelBuilder.Entity<PostImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__post_ima__DC9AC9550DFFBAE7");

            entity.ToTable("post_images");

            entity.Property(e => e.ImageId)
                .ValueGeneratedNever()
                .HasColumnName("image_id");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.UploadedAt)
                .HasColumnType("datetime")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Post).WithMany(p => p.PostImages)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__post_imag__post___6D0D32F4");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__reviews__60883D904490453E");

            entity.ToTable("reviews");

            entity.HasIndex(e => e.TransactionId, "UQ__reviews__85C600AEEAB26A6C").IsUnique();

            entity.Property(e => e.ReviewId)
                .ValueGeneratedNever()
                .HasColumnName("review_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ReviewerId)
                .HasConstraintName("FK__reviews__reviewe__628FA481");

            entity.HasOne(d => d.Transaction).WithOne(p => p.Review)
                .HasForeignKey<Review>(d => d.TransactionId)
                .HasConstraintName("FK__reviews__transac__619B8048");
        });

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__shopping__2EF52A276ED7A938");

            entity.ToTable("shopping_cart");

            entity.HasIndex(e => e.UserId, "UQ__shopping__B9BE370E016E6B31").IsUnique();

            entity.Property(e => e.CartId)
                .ValueGeneratedNever()
                .HasColumnName("cart_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.ShoppingCart)
                .HasForeignKey<ShoppingCart>(d => d.UserId)
                .HasConstraintName("FK__shopping___user___66603565");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__transact__85C600AFF1A2B40C");

            entity.ToTable("transactions");

            entity.Property(e => e.TransactionId)
                .ValueGeneratedNever()
                .HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.ContractFile)
                .HasMaxLength(255)
                .HasColumnName("contract_file");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.SignedAt)
                .HasColumnType("datetime")
                .HasColumnName("signed_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.HasOne(d => d.Buyer).WithMany(p => p.TransactionBuyers)
                .HasForeignKey(d => d.BuyerId)
                .HasConstraintName("FK__transacti__buyer__5AEE82B9");

            entity.HasOne(d => d.Post).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__transacti__post___5CD6CB2B");

            entity.HasOne(d => d.Seller).WithMany(p => p.TransactionSellers)
                .HasForeignKey(d => d.SellerId)
                .HasConstraintName("FK__transacti__selle__5BE2A6F2");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__users__B9BE370FDACAC85A");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "UQ__users__AB6E6164B7587B3D").IsUnique();

            entity.HasIndex(e => e.Phone, "UQ__users__B43B145FA9FD3F8E").IsUnique();

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__vehicles__F2947BC1BC66F8B9");

            entity.ToTable("vehicles");

            entity.HasIndex(e => e.PostId, "UQ__vehicles__3ED787673D6B1F31").IsUnique();

            entity.Property(e => e.VehicleId)
                .ValueGeneratedNever()
                .HasColumnName("vehicle_id");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.Mileage).HasColumnName("mileage");
            entity.Property(e => e.Model)
                .HasMaxLength(100)
                .HasColumnName("model");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.Brand).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__vehicles__brand___47DBAE45");

            entity.HasOne(d => d.Post).WithOne(p => p.Vehicle)
                .HasForeignKey<Vehicle>(d => d.PostId)
                .HasConstraintName("FK__vehicles__post_i__46E78A0C");
        });

        modelBuilder.Entity<VehicleBrand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__vehicle___5E5A8E27CDA7A76F");

            entity.ToTable("vehicle_brands");

            entity.Property(e => e.BrandId)
                .ValueGeneratedNever()
                .HasColumnName("brand_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.WalletId).HasName("PK__wallets__0EE6F041600C0AD3");

            entity.ToTable("wallets");

            entity.HasIndex(e => e.UserId, "UQ__wallets__B9BE370E9F3441F7").IsUnique();

            entity.Property(e => e.WalletId)
                .ValueGeneratedNever()
                .HasColumnName("wallet_id");
            entity.Property(e => e.Balance)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("balance");
            entity.Property(e => e.LastUpdated)
                .HasColumnType("datetime")
                .HasColumnName("last_updated");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.Wallet)
                .HasForeignKey<Wallet>(d => d.UserId)
                .HasConstraintName("FK__wallets__user_id__3C69FB99");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
