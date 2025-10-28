using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.RequestDTO
{
    internal class AuctionDTO
    {
    }
    public class CreateAuctionRequest
    {
        public Guid PostId { get; set; }
        public decimal StartPrice { get; set; }
        public DateTime EndTime { get; set; } // UTC expected
    }
    public class UpdateTransactionRequest
    {
        public string ReceiverName { get; set; } = null!;
        public string ReceiverPhone { get; set; } = null!;
        public string ReceiverAddress { get; set; } = null!;
        public string Note { get; set; } = null!;
    }
    public class PlaceBidRequest
    {
        public Guid AuctionId { get; set; }
        public decimal BidAmount { get; set; }
    }

    public class AuctionCloseResultDTO
    {
        public Guid AuctionId { get; set; }
        public decimal FinalPrice { get; set; }
        public Guid? WinnerId { get; set; }
        public string Status { get; set; } = null!;

    }

}
