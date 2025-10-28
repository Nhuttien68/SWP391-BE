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
            entity.HasKey(e => e.AuctionId).HasName("PK__Auctions__51004A4CC54962A7");

            entity.HasIndex(e => e.PostId, "UQ__Auctions__AA1260199D404E6F").IsUnique();

            entity.Property(e => e.AuctionId).ValueGeneratedNever();
            entity.Property(e => e.CurrentPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Post).WithOne(p => p.Auction)
                .HasForeignKey<Auction>(d => d.PostId)
                .HasConstraintName("FK__Auctions__PostId__6D0D32F4");

            entity.HasOne(d => d.Winner).WithMany(p => p.Auctions)
                .HasForeignKey(d => d.WinnerId)
                .HasConstraintName("FK__Auctions__Winner__6E01572D");
        });

        modelBuilder.Entity<AuctionBid>(entity =>
        {
            entity.HasKey(e => e.BidId).HasName("PK__AuctionB__4A733D9279C134A2");

            entity.Property(e => e.BidId).ValueGeneratedNever();
            entity.Property(e => e.BidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BidTime).HasColumnType("datetime");

            entity.HasOne(d => d.Auction).WithMany(p => p.AuctionBids)
                .HasForeignKey(d => d.AuctionId)
                .HasConstraintName("FK__AuctionBi__Aucti__70DDC3D8");

            entity.HasOne(d => d.User).WithMany(p => p.AuctionBids)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__AuctionBi__UserI__71D1E811");
        });

        modelBuilder.Entity<Battery>(entity =>
        {
            entity.HasKey(e => e.BatteryId).HasName("PK__Batterie__5710805E6D548532");

            entity.HasIndex(e => e.PostId, "UQ__Batterie__AA1260194D7C6610").IsUnique();

            entity.Property(e => e.BatteryId).ValueGeneratedNever();
            entity.Property(e => e.Condition).HasMaxLength(50);

            entity.HasOne(d => d.Brand).WithMany(p => p.Batteries)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__Batteries__Brand__5070F446");

            entity.HasOne(d => d.Post).WithOne(p => p.Battery)
                .HasForeignKey<Battery>(d => d.PostId)
                .HasConstraintName("FK__Batteries__PostI__4F7CD00D");
        });

        modelBuilder.Entity<BatteryBrand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__BatteryB__DAD4F05E7A2322C5");

            entity.Property(e => e.BrandId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B0A5284E97D");

            entity.Property(e => e.CartItemId).ValueGeneratedNever();
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__CartItems__CartI__628FA481");

            entity.HasOne(d => d.Post).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__CartItems__PostI__6383C8BA");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("PK__Favorite__CE74FAD5C192BAED");

            entity.Property(e => e.FavoriteId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Post).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__Favorites__PostI__59063A47");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Favorites__UserI__5812160E");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Posts__AA12601864FF9BEB");

            entity.Property(e => e.PostId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Posts__UserId__46E78A0C");
        });

        modelBuilder.Entity<PostImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__PostImag__7516F70C1CEAB070");

            entity.Property(e => e.ImageId).ValueGeneratedNever();
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Post).WithMany(p => p.PostImages)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__PostImage__PostI__5441852A");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79CEA961F7DF");

            entity.HasIndex(e => e.TransactionId, "UQ__Reviews__55433A6A4AF53CA2").IsUnique();

            entity.Property(e => e.ReviewId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ReviewerId)
                .HasConstraintName("FK__Reviews__Reviewe__787EE5A0");

            entity.HasOne(d => d.Transaction).WithOne(p => p.Review)
                .HasForeignKey<Review>(d => d.TransactionId)
                .HasConstraintName("FK__Reviews__Transac__778AC167");
        });

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Shopping__51BCD7B7C596F96B");

            entity.HasIndex(e => e.UserId, "UQ__Shopping__1788CC4D8DDB7025").IsUnique();

            entity.Property(e => e.CartId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.User).WithOne(p => p.ShoppingCart)
                .HasForeignKey<ShoppingCart>(d => d.UserId)
                .HasConstraintName("FK__ShoppingC__UserI__5DCAEF64");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__55433A6B33BD9339");

            entity.Property(e => e.TransactionId).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.ReceiverAddress).HasMaxLength(255);
            entity.Property(e => e.ReceiverName).HasMaxLength(100);
            entity.Property(e => e.ReceiverPhone).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Buyer).WithMany(p => p.TransactionBuyers)
                .HasForeignKey(d => d.BuyerId)
                .HasConstraintName("FK__Transacti__Buyer__6754599E");

            entity.HasOne(d => d.Post).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__Transacti__PostI__693CA210");

            entity.HasOne(d => d.Seller).WithMany(p => p.TransactionSellers)
                .HasForeignKey(d => d.SellerId)
                .HasConstraintName("FK__Transacti__Selle__68487DD7");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C415D0E31");

            entity.HasIndex(e => e.Phone, "UQ__Users__5C7E359E7D4516D0").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105343BD9A432").IsUnique();

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__Vehicles__476B5492E73887DF");

            entity.HasIndex(e => e.PostId, "UQ__Vehicles__AA1260195ABEB1D2").IsUnique();

            entity.Property(e => e.VehicleId).ValueGeneratedNever();
            entity.Property(e => e.Model).HasMaxLength(100);

            entity.HasOne(d => d.Brand).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__Vehicles__BrandI__4BAC3F29");

            entity.HasOne(d => d.Post).WithOne(p => p.Vehicle)
                .HasForeignKey<Vehicle>(d => d.PostId)
                .HasConstraintName("FK__Vehicles__PostId__4AB81AF0");
        });

        modelBuilder.Entity<VehicleBrand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__VehicleB__DAD4F05EC658F288");

            entity.Property(e => e.BrandId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.WalletId).HasName("PK__Wallets__84D4F90E57C0AEF8");

            entity.HasIndex(e => e.UserId, "UQ__Wallets__1788CC4D86B3F486").IsUnique();

            entity.Property(e => e.WalletId).ValueGeneratedNever();
            entity.Property(e => e.Balance)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithOne(p => p.Wallet)
                .HasForeignKey<Wallet>(d => d.UserId)
                .HasConstraintName("FK__Wallets__UserId__3F466844");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
