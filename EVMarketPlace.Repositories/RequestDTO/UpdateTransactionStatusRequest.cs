using System.ComponentModel.DataAnnotations;

namespace EVMarketPlace.Repositories.RequestDTO
{
    /// <summary>
    /// Request cập nhật trạng thái giao dịch
    /// </summary>
    public class UpdateTransactionStatusRequest
    {
        [Required]
        public Guid TransactionId { get; set; }

        [Required]
        public string Status { get; set; }
    }
}