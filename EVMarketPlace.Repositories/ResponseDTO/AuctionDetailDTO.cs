using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.ResponseDTO
{
    public class AuctionBidHistoryItemDTO
    {
        public Guid BidId { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public decimal? BidAmount { get; set; }
        public DateTime? BidTime { get; set; }
    }

    public class AuctionPostSummaryDTO
    {
        public Guid PostId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<string>? ImageUrls { get; set; }
    }

    public class AuctionDetailDTO
    {
        public Guid AuctionId { get; set; }
        public Guid? PostId { get; set; }
        public decimal? StartPrice { get; set; }
        public decimal? CurrentPrice { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Status { get; set; }
        public AuctionPostSummaryDTO? Post { get; set; }
        public List<AuctionBidHistoryItemDTO> AuctionBids { get; set; } = new();
    }
}
