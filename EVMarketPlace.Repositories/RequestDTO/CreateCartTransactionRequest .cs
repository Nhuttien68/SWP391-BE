using System.ComponentModel.DataAnnotations;

namespace EVMarketPlace.Repositories.RequestDTO
{
    // Request thanh toán toàn bộ giỏ hàng
    public class CreateCartTransactionRequest
    {
        [Required(ErrorMessage = "CartId không được để trống")]
        public Guid CartId { get; set; }

        [Required(ErrorMessage = "Phương thức thanh toán không được để trống")]
        public string PaymentMethod { get; set; }

        [Required(ErrorMessage = "Tên người nhận không được để trống")]
        [MaxLength(100)]
        public string ReceiverName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone]
        [MaxLength(20)]
        public string ReceiverPhone { get; set; }

        [Required(ErrorMessage = "Địa chỉ nhận hàng không được để trống")]
        [MaxLength(255)]
        public string ReceiverAddress { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}