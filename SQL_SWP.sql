-- =============================================
-- CREATE DATABASE
-- =============================================
CREATE DATABASE EV_Marketplace;
GO
USE EV_Marketplace;
GO

-- =============================================
-- USERS
-- =============================================
CREATE TABLE Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Phone NVARCHAR(20) UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20), -- Admin | Member
    CreatedAt DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) -- Active | Inactive | Deleted
);

-- =============================================
-- WALLETS
-- =============================================
CREATE TABLE Wallets (
    WalletId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER UNIQUE,
    Balance DECIMAL(18,2) DEFAULT 0,
    LastUpdated DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- =============================================
-- WALLET TRANSACTIONS
-- =============================================
CREATE TABLE WalletTransactions (
    WalletTransactionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    WalletId UNIQUEIDENTIFIER NOT NULL,
    TransactionType NVARCHAR(50) NOT NULL, -- TOPUP | WITHDRAW | DEDUCT | POSTING_FEE | REFUND
    Amount DECIMAL(18,2),
    BalanceBefore DECIMAL(18,2),
    BalanceAfter DECIMAL(18,2),
    ReferenceId NVARCHAR(200),
    PaymentMethod NVARCHAR(50), -- VNPAY | WALLET | BANK
    Description NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (WalletId) REFERENCES Wallets(WalletId)
);

-- =============================================
-- WITHDRAWAL REQUESTS
-- =============================================
CREATE TABLE WithdrawalRequests (
    WithdrawalId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    WalletId UNIQUEIDENTIFIER,
    Amount DECIMAL(18,2),
    BankName NVARCHAR(100),
    BankAccountNumber NVARCHAR(50),
    BankAccountName NVARCHAR(100),
    Status NVARCHAR(50) DEFAULT 'PENDING',
    RequestedAt DATETIME DEFAULT GETDATE(),
    ProcessedAt DATETIME NULL,
    ProcessedBy UNIQUEIDENTIFIER NULL,
    Note NVARCHAR(500) NULL,
    AdminNote NVARCHAR(500) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (WalletId) REFERENCES Wallets(WalletId),
    FOREIGN KEY (ProcessedBy) REFERENCES Users(UserId)
);

-- =============================================
-- VEHICLE & BATTERY BRANDS
-- =============================================
CREATE TABLE VehicleBrands (
    BrandId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
	 Status NVARCHAR(50) ,
);

CREATE TABLE BatteryBrands (
    BrandId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
	 Status NVARCHAR(50) ,
);

-- =============================================
-- POST PACKAGES (GÓI ĐĂNG BÀI)
-- =============================================
CREATE TABLE PostPackages (
    PackageId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PackageName NVARCHAR(100) NOT NULL,
    DurationInDays INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- =============================================
-- POSTS (BÀI ĐĂNG + GÓI + HẠN SỬ DỤNG)
-- =============================================
CREATE TABLE Posts (
    PostId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    PackageId UNIQUEIDENTIFIER NOT NULL,
    PackagePrice DECIMAL(18,2) NOT NULL, -- giá tại thời điểm mua gói

    Type NVARCHAR(50), -- vehicle | battery
    Title NVARCHAR(200),
    Description NVARCHAR(MAX),
    Price DECIMAL(18,2),

    CreatedAt DATETIME DEFAULT GETDATE(),
    ExpireAt DATETIME NOT NULL, -- được tính theo gói
    Status NVARCHAR(20), -- Active | Hidden | Sold | Deleted | Expired

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (PackageId) REFERENCES PostPackages(PackageId)
);

-- =============================================
-- VEHICLES (Chi tiết xe)
-- =============================================
CREATE TABLE Vehicles (
    VehicleId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PostId UNIQUEIDENTIFIER UNIQUE,
    BrandId UNIQUEIDENTIFIER,
    Model NVARCHAR(100),
    Year INT,
    Mileage INT,
    FOREIGN KEY (PostId) REFERENCES Posts(PostId),
    FOREIGN KEY (BrandId) REFERENCES VehicleBrands(BrandId)
);

-- =============================================
-- BATTERIES (Chi tiết pin)
-- =============================================
CREATE TABLE Batteries (
    BatteryId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PostId UNIQUEIDENTIFIER UNIQUE,
    BrandId UNIQUEIDENTIFIER,
    Capacity INT,
    [Condition] NVARCHAR(50),
    FOREIGN KEY (PostId) REFERENCES Posts(PostId),
    FOREIGN KEY (BrandId) REFERENCES BatteryBrands(BrandId)
);

-- =============================================
-- POST IMAGES
-- =============================================
CREATE TABLE PostImages (
    ImageId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PostId UNIQUEIDENTIFIER,
    ImageUrl NVARCHAR(500),
    UploadedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PostId) REFERENCES Posts(PostId)
);

-- =============================================
-- FAVORITES
-- =============================================
CREATE TABLE Favorites (
    FavoriteId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER,
    PostId UNIQUEIDENTIFIER,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (PostId) REFERENCES Posts(PostId)
);

-- =============================================
-- SHOPPING CART
-- =============================================
CREATE TABLE ShoppingCarts (
    CartId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER UNIQUE,
    CreatedAt DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE CartItems (
    CartItemId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CartId UNIQUEIDENTIFIER,
    PostId UNIQUEIDENTIFIER,
    Quantity INT DEFAULT 1,
    AddedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CartId) REFERENCES ShoppingCarts(CartId),
    FOREIGN KEY (PostId) REFERENCES Posts(PostId)
);

-- =============================================
-- TRANSACTIONS
-- =============================================
CREATE TABLE Transactions (
    TransactionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BuyerId UNIQUEIDENTIFIER,
    SellerId UNIQUEIDENTIFIER,
    PostId UNIQUEIDENTIFIER,
    CartId UNIQUEIDENTIFIER,
    Amount DECIMAL(18,2),
    PaymentMethod NVARCHAR(50),
    Status NVARCHAR(20),
    CreatedAt DATETIME DEFAULT GETDATE(),
    ReceiverName NVARCHAR(100),
    ReceiverPhone NVARCHAR(20),
    ReceiverAddress NVARCHAR(255),
    Note NVARCHAR(255),
    FOREIGN KEY (BuyerId) REFERENCES Users(UserId),
    FOREIGN KEY (SellerId) REFERENCES Users(UserId),
    FOREIGN KEY (PostId) REFERENCES Posts(PostId),
    FOREIGN KEY (CartId) REFERENCES ShoppingCarts(CartId)
);

-- =============================================
-- AUCTIONS (ĐẤU GIÁ + BƯỚC NHẢY)
-- =============================================
CREATE TABLE Auctions (
    AuctionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PostId UNIQUEIDENTIFIER UNIQUE,
    StartPrice DECIMAL(18,2),
    CurrentPrice DECIMAL(18,2),
    BidStep DECIMAL(18,2), -- bước nhảy
    EndTime DATETIME,
    Status NVARCHAR(20), -- Active | Ended
    WinnerId UNIQUEIDENTIFIER NULL,
    FOREIGN KEY (PostId) REFERENCES Posts(PostId),
    FOREIGN KEY (WinnerId) REFERENCES Users(UserId)
);

-- =============================================
-- AUCTION BIDS (LỊCH SỬ ÁP GIÁ)
-- =============================================
CREATE TABLE AuctionBids (
    BidId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AuctionId UNIQUEIDENTIFIER,
    UserId UNIQUEIDENTIFIER,
    BidAmount DECIMAL(18,2),
    BidTime DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AuctionId) REFERENCES Auctions(AuctionId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- =============================================
-- REVIEWS
-- =============================================
CREATE TABLE Reviews (
    ReviewId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TransactionId UNIQUEIDENTIFIER,
    ReviewerId UNIQUEIDENTIFIER,
    ReviewedUserId UNIQUEIDENTIFIER,
    PostId UNIQUEIDENTIFIER,
    ReviewTargetType NVARCHAR(20), -- Seller | Post
    Rating INT CHECK (Rating BETWEEN 1 AND 5),
    Comment NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TransactionId) REFERENCES Transactions(TransactionId),
    FOREIGN KEY (ReviewerId) REFERENCES Users(UserId),
    FOREIGN KEY (ReviewedUserId) REFERENCES Users(UserId),
    FOREIGN KEY (PostId) REFERENCES Posts(PostId)
);
GO
