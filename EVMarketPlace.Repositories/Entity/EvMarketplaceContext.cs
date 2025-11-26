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

    public virtual DbSet<PostPackage> PostPackages { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleBrand> VehicleBrands { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }

    public virtual DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }

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
       => optionsBuilder.UseSqlServer(GetConnectionString("DefaultConnection"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auction>(entity =>
        {
            entity.HasKey(e => e.AuctionId).HasName("PK__Auctions__51004A4CD5CC3890");

            entity.HasIndex(e => e.PostId, "UQ__Auctions__AA126019E80657AE").IsUnique();

            entity.Property(e => e.AuctionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.BidStep).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CurrentPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Post).WithOne(p => p.Auction)
                .HasForeignKey<Auction>(d => d.PostId)
                .HasConstraintName("FK__Auctions__PostId__123EB7A3");

            entity.HasOne(d => d.Winner).WithMany(p => p.Auctions)
                .HasForeignKey(d => d.WinnerId)
                .HasConstraintName("FK__Auctions__Winner__1332DBDC");
        });

        modelBuilder.Entity<AuctionBid>(entity =>
        {
            entity.HasKey(e => e.BidId).HasName("PK__AuctionB__4A733D92646D4423");

            entity.Property(e => e.BidId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.BidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BidTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Auction).WithMany(p => p.AuctionBids)
                .HasForeignKey(d => d.AuctionId)
                .HasConstraintName("FK__AuctionBi__Aucti__17F790F9");

            entity.HasOne(d => d.User).WithMany(p => p.AuctionBids)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__AuctionBi__UserI__18EBB532");
        });

        modelBuilder.Entity<Battery>(entity =>
        {
            entity.HasKey(e => e.BatteryId).HasName("PK__Batterie__5710805E394CE9A7");

            entity.HasIndex(e => e.PostId, "UQ__Batterie__AA1260198A74CCA3").IsUnique();

            entity.Property(e => e.BatteryId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Condition).HasMaxLength(50);

            entity.HasOne(d => d.Brand).WithMany(p => p.Batteries)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__Batteries__Brand__6EF57B66");

            entity.HasOne(d => d.Post).WithOne(p => p.Battery)
                .HasForeignKey<Battery>(d => d.PostId)
                .HasConstraintName("FK__Batteries__PostI__6E01572D");
        });

        modelBuilder.Entity<BatteryBrand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__BatteryB__DAD4F05E86FD8F4D");

            entity.Property(e => e.BrandId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B0AAF48007D");

            entity.Property(e => e.CartItemId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__CartItems__CartI__04E4BC85");

            entity.HasOne(d => d.Post).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__CartItems__PostI__05D8E0BE");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("PK__Favorite__CE74FAD553EFFDB6");

            entity.Property(e => e.FavoriteId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Post).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__Favorites__PostI__797309D9");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Favorites__UserI__787EE5A0");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Posts__AA126018CA0AC44B");

            entity.Property(e => e.PostId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
            entity.Property(e => e.PackagePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.Package).WithMany(p => p.Posts)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Posts__PackageId__6383C8BA");

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Posts__UserId__628FA481");
        });

        modelBuilder.Entity<PostImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__PostImag__7516F70C5B5EEACB");

            entity.Property(e => e.ImageId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Post).WithMany(p => p.PostImages)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__PostImage__PostI__73BA3083");
        });

        modelBuilder.Entity<PostPackage>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("PK__PostPack__322035CCB0814179");

            entity.Property(e => e.PackageId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PackageName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79CE64212738");

            entity.Property(e => e.ReviewId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReviewTargetType).HasMaxLength(20);

            entity.HasOne(d => d.Post).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__Reviews__PostId__2180FB33");

            entity.HasOne(d => d.ReviewedUser).WithMany(p => p.ReviewReviewedUsers)
                .HasForeignKey(d => d.ReviewedUserId)
                .HasConstraintName("FK__Reviews__Reviewe__208CD6FA");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.ReviewReviewers)
                .HasForeignKey(d => d.ReviewerId)
                .HasConstraintName("FK__Reviews__Reviewe__1F98B2C1");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK__Reviews__Transac__1EA48E88");
        });

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Shopping__51BCD7B799030F7F");

            entity.HasIndex(e => e.UserId, "UQ__Shopping__1788CC4D8FEFEC89").IsUnique();

            entity.Property(e => e.CartId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.User).WithOne(p => p.ShoppingCart)
                .HasForeignKey<ShoppingCart>(d => d.UserId)
                .HasConstraintName("FK__ShoppingC__UserI__7F2BE32F");
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PK__SystemSe__54372B1D807E33FB");

            entity.HasIndex(e => e.SettingKey, "UQ__SystemSe__01E719ADDA599E9C").IsUnique();

            entity.Property(e => e.SettingId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.SettingValue).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.SystemSettings)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__SystemSet__Updat__4BAC3F29");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__55433A6B496ED5D8");

            entity.Property(e => e.TransactionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CommissionAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CommissionRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.ReceiverAddress).HasMaxLength(255);
            entity.Property(e => e.ReceiverName).HasMaxLength(100);
            entity.Property(e => e.ReceiverPhone).HasMaxLength(20);
            entity.Property(e => e.SellerReceived).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Buyer).WithMany(p => p.TransactionBuyers)
                .HasForeignKey(d => d.BuyerId)
                .HasConstraintName("FK__Transacti__Buyer__0A9D95DB");

            entity.HasOne(d => d.Cart).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__Transacti__CartI__0D7A0286");

            entity.HasOne(d => d.Post).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__Transacti__PostI__0C85DE4D");

            entity.HasOne(d => d.Seller).WithMany(p => p.TransactionSellers)
                .HasForeignKey(d => d.SellerId)
                .HasConstraintName("FK__Transacti__Selle__0B91BA14");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C76EB9331");

            entity.HasIndex(e => e.Phone, "UQ__Users__5C7E359EF2034901").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534D261080E").IsUnique();

            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
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
            entity.HasKey(e => e.VehicleId).HasName("PK__Vehicles__476B549209795B79");

            entity.HasIndex(e => e.PostId, "UQ__Vehicles__AA12601907B40EBA").IsUnique();

            entity.Property(e => e.VehicleId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Model).HasMaxLength(100);

            entity.HasOne(d => d.Brand).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__Vehicles__BrandI__693CA210");

            entity.HasOne(d => d.Post).WithOne(p => p.Vehicle)
                .HasForeignKey<Vehicle>(d => d.PostId)
                .HasConstraintName("FK__Vehicles__PostId__68487DD7");
        });

        modelBuilder.Entity<VehicleBrand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__VehicleB__DAD4F05E2CB44306");

            entity.Property(e => e.BrandId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.WalletId).HasName("PK__Wallets__84D4F90E0E5968F2");

            entity.HasIndex(e => e.UserId, "UQ__Wallets__1788CC4D4177269D").IsUnique();

            entity.Property(e => e.WalletId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Balance)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithOne(p => p.Wallet)
                .HasForeignKey<Wallet>(d => d.UserId)
                .HasConstraintName("FK__Wallets__UserId__412EB0B6");
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.WalletTransactionId).HasName("PK__WalletTr__7184AEEFB7B82153");

            entity.Property(e => e.WalletTransactionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BalanceAfter).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BalanceBefore).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.ReferenceId).HasMaxLength(200);
            entity.Property(e => e.TransactionType).HasMaxLength(50);

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WalletTra__Walle__45F365D3");
        });

        modelBuilder.Entity<WithdrawalRequest>(entity =>
        {
            entity.HasKey(e => e.WithdrawalId).HasName("PK__Withdraw__7C842C6E655F1CE6");

            entity.Property(e => e.WithdrawalId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdminNote).HasMaxLength(500);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BankAccountName).HasMaxLength(100);
            entity.Property(e => e.BankAccountNumber).HasMaxLength(50);
            entity.Property(e => e.BankName).HasMaxLength(100);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.ProcessedAt).HasColumnType("datetime");
            entity.Property(e => e.RequestedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("PENDING");

            entity.HasOne(d => d.ProcessedByNavigation).WithMany(p => p.WithdrawalRequestProcessedByNavigations)
                .HasForeignKey(d => d.ProcessedBy)
                .HasConstraintName("FK__Withdrawa__Proce__534D60F1");

            entity.HasOne(d => d.User).WithMany(p => p.WithdrawalRequestUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Withdrawa__UserI__5165187F");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WithdrawalRequests)
                .HasForeignKey(d => d.WalletId)
                .HasConstraintName("FK__Withdrawa__Walle__52593CB8");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
