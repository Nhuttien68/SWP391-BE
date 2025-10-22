using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.ResponseDTO
{
    public class WalletResponeseDto
    {
        public Guid? WalletId { get; set; }
        public Guid? UserId { get; set; }
        public decimal? Balance { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? Status { get; set; } 
    }

    public class WalletTopUpResponeseDto
    {
        public Guid? WalletId { get; set; }
        public decimal AmountTopUp { get; set; }
        public decimal NewBalance { get; set; }
        public decimal OldBalance { get; set; }
        public DateTime TopUpDate { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;

    }
}
