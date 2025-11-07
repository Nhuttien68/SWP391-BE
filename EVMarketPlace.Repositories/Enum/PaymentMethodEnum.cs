namespace EVMarketPlace.Repositories.Enum
{
    /// <summary>
    /// Phương thức thanh toán hỗ trợ
    /// </summary>
    public enum PaymentMethodEnum
    {
        WALLET,           // Ví điện tử hệ thống
        VNPAY,            // Thanh toán VNPay
        COD,              // Tiền mặt khi nhận hàng
        BANK_TRANSFER     // Chuyển khoản ngân hàng
    }
}