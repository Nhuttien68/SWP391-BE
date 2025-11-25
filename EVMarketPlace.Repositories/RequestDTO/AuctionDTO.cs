using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Required(ErrorMessage ="PostId Is Not Alloew null")]
        public Guid PostId { get; set; }
        [Required(ErrorMessage = "PostStartPriced Is Not Alloew null")]
        public decimal StartPrice { get; set; }
        [Required(ErrorMessage = "BidStep Is Not Alloew null")]
        public decimal BidStep { get; set; }
        [Required(ErrorMessage = "EndTime Is Not Alloew null")]
   

        public DateTime EndTime { get; set; } // UTC expected
    }
    public class UpdateTransactionRequest
    {
        [Required(ErrorMessage = "AuctionId Is Not Alloew null")]
        public string ReceiverName { get; set; } = null!;
        [Required(ErrorMessage = "ReceiverPhone Is Not Alloew null")]
        public string ReceiverPhone { get; set; } = null!;
        [Required(ErrorMessage = "ReceiverAddress Is Not Alloew null")]
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
        public decimal FinalAmount { get; set; }
        public Guid? WinnerId { get; set; }
        public string Status { get; set; } = null!;

    }

}
