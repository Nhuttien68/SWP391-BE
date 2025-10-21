using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace EVMarketPlace.Services.Implements
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new();
        private readonly SortedList<string, string> _responseData = new();

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData[key] = value;
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData[key] = value;
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
        }

        /// <summary>
        /// Tạo URL thanh toán - Data được URL encode
        /// </summary>
        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();

            //  Sắp xếp theo thứ tự alphabet
            foreach (var kvp in _requestData.OrderBy(x => x.Key))
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    data.Append(WebUtility.UrlEncode(kvp.Key));
                    data.Append("=");
                    data.Append(WebUtility.UrlEncode(kvp.Value));
                    data.Append("&");
                }
            }

            // Bỏ ký tự & cuối cùng
            var querystring = data.ToString();
            if (querystring.Length > 0)
            {
                querystring = querystring.Remove(querystring.Length - 1, 1);
            }

            // Tạo secure hash từ querystring đã encode
            var vnpSecureHash = HmacSHA512(vnpHashSecret, querystring);

            // Ghép URL cuối cùng
            return $"{baseUrl}?{querystring}&vnp_SecureHash={vnpSecureHash}";
        }

        /// <summary>
        /// Validate chữ ký khi nhận response - Data KHÔNG encode
        /// </summary>
        public bool ValidateSignature(string inputHash, string secretKey)
        {
            //  Loại bỏ vnp_SecureHash và vnp_SecureHashType trước khi tính
            var data = new SortedList<string, string>(_responseData);
            data.Remove("vnp_SecureHash");
            data.Remove("vnp_SecureHashType");

            var rspRaw = new StringBuilder();

            //  Ghép data theo format: key1=value1&key2=value2 (KHÔNG URL encode)
            foreach (var kvp in data.OrderBy(x => x.Key))
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    rspRaw.Append(kvp.Key);
                    rspRaw.Append("=");
                    rspRaw.Append(kvp.Value);
                    rspRaw.Append("&");
                }
            }

            // Bỏ ký tú & cuối
            var rawData = rspRaw.ToString();
            if (rawData.Length > 0)
            {
                rawData = rawData.Remove(rawData.Length - 1, 1);
            }

            //  Tính hash từ raw data (không encode)
            var myChecksum = HmacSHA512(secretKey, rawData);

            // So sánh hash (case-insensitive)
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Tính HMAC SHA512
        /// </summary>
        private static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }
    }
}