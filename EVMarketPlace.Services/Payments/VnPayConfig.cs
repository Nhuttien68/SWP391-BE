namespace EVMarketPlace.Services
{
    public class VnPayConfig
    {
        public string TmnCode { get; set; } = string.Empty;
        public string HashSecret { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string IpnUrl { get; set; } = string.Empty;

        //  Thêm các properties bị thiếu
        public string Version { get; set; } = "2.1.0";
        public string Command { get; set; } = "pay";
        public string CurrCode { get; set; } = "VND";
        public string Locale { get; set; } = "vn";
        public string OrderType { get; set; } = "other";
        public string TimeZoneId { get; set; } = "SE Asia Standard Time";
        public int PaymentTimeoutMinutes { get; set; } = 15;
        public string SuccessResponseCode { get; set; } = "00";
    }
}